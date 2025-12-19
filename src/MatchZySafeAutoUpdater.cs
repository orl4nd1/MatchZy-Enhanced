using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace MatchZy;

/// <summary>
/// MatchZy-aware CS2 auto-updater that NEVER restarts while a MatchZy match is in progress.
/// It only restarts when matchzy_tournament_status is in a safe state (idle/postgame/error).
/// Implemented as part of the main MatchZy plugin.
/// </summary>
public partial class MatchZy
{

    private const string SteamApiEndpoint =
        "https://api.steampowered.com/ISteamApps/UpToDateCheck/v0001/?appid=730&version={0}";

    // Timings (seconds)
    private const float DefaultUpdateCheckIntervalSeconds = 300f; // 5 minutes
    private const float ShutdownRetryDelaySeconds = 60f;          // 1 minute

    // State
    private static double _updateFoundTime;
    private static bool _updateAvailable;
    private static bool _restartRequired;
    private static int _requiredVersion;

    // Cvars we care about from MatchZy
    private static ConVar? _matchzyTournamentStatus;
    private static ConVar? _matchzyTournamentMatch;

    /// <summary>
    /// Initialize the MatchZy-safe auto-updater. Called from MatchZy.Load().
    /// </summary>
    private void InitializeMatchZySafeAutoUpdater()
    {
        _matchzyTournamentStatus = ConVar.Find("matchzy_tournament_status");
        _matchzyTournamentMatch = ConVar.Find("matchzy_tournament_match");

        RegisterListener<Listeners.OnGameServerSteamAPIActivated>(OnGameServerSteamAPIActivated);

        // Periodic check for updates
        AddTimer(DefaultUpdateCheckIntervalSeconds, CheckServerVersionTimer, TimerFlags.REPEAT);
    }

    private void OnGameServerSteamAPIActivated()
    {
        Logger.LogInformation("[MatchZySafeAutoUpdater] Steam API activated. MatchZy-safe update checks enabled.");
    }

    /// <summary>
    /// Timer callback that kicks off an async update check.
    /// </summary>
    private void CheckServerVersionTimer()
    {
        try
        {
            // Never perform update checks while a MatchZy match is in progress; this keeps
            // all Steam API polling and restart decisions strictly outside live matches.
            string status = GetMatchZyStatus();
            if (IsMatchInProgress(status))
            {
                return;
            }

            if (_restartRequired)
            {
                // Already committed to restarting; no need to keep hammering the Steam API.
                return;
            }

            _ = CheckServerVersionAndMaybeScheduleShutdownAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError("[MatchZySafeAutoUpdater] Error scheduling update check: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Performs the actual Steam UpToDateCheck and, if an update is available, schedules a
    /// shutdown that respects MatchZy's tournament status.
    /// </summary>
    private async Task CheckServerVersionAndMaybeScheduleShutdownAsync()
    {
        try
        {
            bool hasUpdate = await IsUpdateAvailableAsync();
            if (!hasUpdate)
            {
                return;
            }

            Server.NextFrame(ManageServerUpdate);
        }
        catch (Exception ex)
        {
            Logger.LogError("[MatchZySafeAutoUpdater] Error while checking for updates: {Message}", ex.Message);
        }
    }

    private void ManageServerUpdate()
    {
        if (!_updateAvailable)
        {
            _updateFoundTime = Server.CurrentTime;
            _updateAvailable = true;

            // Log a clear, machine-parseable marker for external server managers.
            // Your manager can watch for this exact string:
            //   [MATCHZY_UPDATE_AVAILABLE] required_version=<number>
            Logger.LogInformation("[MatchZySafeAutoUpdater] New CS2 update released (Required version: {Version})", _requiredVersion);
            Logger.LogInformation("[MATCHZY_UPDATE_AVAILABLE] required_version={Version}", _requiredVersion);
        }

        _restartRequired = true;

        // Try to shut down, but respect MatchZy’s status.
        TryShutdownRespectingMatchZy();
    }

    /// <summary>
    /// Attempts to shut down the server. If MatchZy reports a live/active match,
    /// we defer and reschedule instead of quitting.
    /// </summary>
    private void TryShutdownRespectingMatchZy()
    {
        if (!_restartRequired)
        {
            return;
        }

        string status = GetMatchZyStatus();
        string matchSlug = GetMatchZyMatchSlug();

        if (IsMatchInProgress(status))
        {
            Logger.LogInformation(
                "[MatchZySafeAutoUpdater] Update available (version {Version}), but MatchZy status is '{Status}' for match '{MatchSlug}'. Deferring shutdown.",
                _requiredVersion, status, string.IsNullOrEmpty(matchSlug) ? "<none>" : matchSlug
            );

            // Reschedule another check after a delay; we keep doing this until status is safe.
            AddTimer(ShutdownRetryDelaySeconds, TryShutdownRespectingMatchZy, TimerFlags.STOP_ON_MAPCHANGE);
            return;
        }

        Logger.LogInformation(
            "[MatchZySafeAutoUpdater] MatchZy status is '{Status}' (safe). Preparing server shutdown for CS2 update {Version}.",
            status, _requiredVersion
        );

        PrepareServerShutdown();
    }

    private string GetMatchZyStatus()
    {
        try
        {
            return _matchzyTournamentStatus?.GetPrimitiveValue<string>() ?? "idle";
        }
        catch
        {
            return "idle";
        }
    }

    private string GetMatchZyMatchSlug()
    {
        try
        {
            return _matchzyTournamentMatch?.GetPrimitiveValue<string>() ?? "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Treat these MatchZy statuses as "match in progress" and never restart during them.
    /// </summary>
    private static bool IsMatchInProgress(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return false;
        }

        status = status.ToLowerInvariant();
        return status is "loading"
                     or "warmup"
                     or "knife"
                     or "live"
                     or "paused"
                     or "halftime";
        // "postgame", "idle", "error" are considered safe to restart.
    }

    /// <summary>
    /// Kicks all human players and then runs "quit".
    /// </summary>
    private void PrepareServerShutdown()
    {
        var players = Utilities.GetPlayers()
            .Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false })
            .ToList();

        foreach (var player in players)
        {
            try
            {
                switch (player.Connected)
                {
                    case PlayerConnectedState.PlayerConnected:
                    case PlayerConnectedState.PlayerConnecting:
                    case PlayerConnectedState.PlayerReconnecting:
                        Server.ExecuteCommand(
                            $"kickid {player.UserId} Due to the game update (Version: {_requiredVersion}), the server is now restarting.");
                        break;
                }
            }
            catch
            {
                // Best effort; ignore failures for individual players.
            }
        }

        AddTimer(1.0f, ShutdownServer);
    }

    private void ShutdownServer()
    {
        // Second machine-parseable marker indicating that we are actually quitting now:
        //   [MATCHZY_UPDATE_SHUTDOWN] required_version=<number>
        Logger.LogInformation("[MatchZySafeAutoUpdater] Initiating server shutdown for CS2 update {Version}.", _requiredVersion);
        Logger.LogInformation("[MATCHZY_UPDATE_SHUTDOWN] required_version={Version}", _requiredVersion);
        Server.ExecuteCommand("quit");
    }

    /// <summary>
    /// Console command: manually check if the server is up to date and print the result.
    /// Does NOT schedule a restart; purely informational.
    /// </summary>
    [ConsoleCommand("matchzy_check_for_updates", "Check whether this CS2 server is up to date according to Steam.")]
    public void MatchZyCheckForUpdates(CCSPlayerController? player, CommandInfo command)
    {
        // Run the check on the next frame to keep the command handler light.
        Server.NextFrame(async () =>
        {
            string prefix = "[MatchZyUpToDate]";

            try
            {
                (bool upToDate, int requiredVersion) = await GetUpdateStatusAsync();

                string msg = upToDate
                    ? $"{prefix} Server is up to date."
                    : $"{prefix} Update available. Required version: {requiredVersion}. The auto-updater will restart once MatchZy is idle/postgame.";

                if (player != null && player.IsValid)
                {
                    player.PrintToChat($" {msg}");
                }
                else
                {
                    Logger.LogInformation(msg);
                }
            }
            catch (Exception ex)
            {
                string err = $"{prefix} Failed to check for updates: {ex.Message}";
                if (player != null && player.IsValid)
                {
                    player.PrintToChat($" {err}");
                }
                else
                {
                    Logger.LogError(err);
                }
            }
        });
    }

    /// <summary>
    /// Returns (upToDate, requiredVersion). Does NOT mutate restart state.
    /// </summary>
    private async Task<(bool upToDate, int requiredVersion)> GetUpdateStatusAsync()
    {
        string steamInfPatchVersion = await GetSteamInfPatchVersionAsync();

        if (string.IsNullOrWhiteSpace(steamInfPatchVersion))
        {
            throw new InvalidOperationException("steam.inf patch version could not be determined.");
        }

        using HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetAsync(string.Format(SteamApiEndpoint, steamInfPatchVersion));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Steam UpToDateCheck request failed with status {response.StatusCode}.");
        }

        var upToDateCheckResponse = await response.Content.ReadFromJsonAsync<UpToDateCheckResponse>();

        if (upToDateCheckResponse?.Response is not { Success: true } resp)
        {
            // If Steam says Success=false, treat it as "cannot determine".
            throw new InvalidOperationException("Steam UpToDateCheck did not return a successful response.");
        }

        return (resp.UpToDate, resp.RequiredVersion);
    }

    /// <summary>
    /// Checks for an update and updates internal state if one is found.
    /// </summary>
    private async Task<bool> IsUpdateAvailableAsync()
    {
        (bool upToDate, int requiredVersion) = await GetUpdateStatusAsync();

        if (upToDate)
        {
            return false;
        }

        _requiredVersion = requiredVersion;
        return true;
    }

    private async Task<string> GetSteamInfPatchVersionAsync()
    {
        string steamInfPath = Path.Combine(Server.GameDirectory, "csgo", "steam.inf");

        if (!File.Exists(steamInfPath))
        {
            Logger.LogError("[MatchZySafeAutoUpdater] steam.inf not found at {Path}.", steamInfPath);
            return string.Empty;
        }

        try
        {
            string steamInfContents = await File.ReadAllTextAsync(steamInfPath);
            Match match = PatchVersionRegex().Match(steamInfContents);

            if (match.Success)
            {
                return match.Groups["version"].Value;
            }

            Logger.LogError("[MatchZySafeAutoUpdater] Could not find PatchVersion key in {Path}.", steamInfPath);
            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogError("[MatchZySafeAutoUpdater] Error reading steam.inf: {Message}", ex.Message);
            return string.Empty;
        }
    }

    [GeneratedRegex(@"PatchVersion=(?<version>[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex PatchVersionRegex();
}

// --- Steam UpToDateCheck DTOs ---

public sealed class UpToDateCheckResponse
{
    [JsonPropertyName("response")]
    public UpToDateCheckInnerResponse? Response { get; set; }
}

public sealed class UpToDateCheckInnerResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("up_to_date")]
    public bool UpToDate { get; set; }

    [JsonPropertyName("required_version")]
    public int RequiredVersion { get; set; }
}


