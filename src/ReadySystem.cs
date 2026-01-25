using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace MatchZy;

public partial class MatchZy
{
    public Dictionary<CsTeam, bool> teamReadyOverride = new() {
        {CsTeam.Terrorist, false},
        {CsTeam.CounterTerrorist, false},
        {CsTeam.Spectator, false}
    };

    public bool allowForceReady = true;

    public bool IsTeamsReady()
    {
        return IsTeamReady((int)CsTeam.CounterTerrorist) && IsTeamReady((int)CsTeam.Terrorist);
    }

    public bool IsSpectatorsReady()
    {
        return IsTeamReady((int)CsTeam.Spectator);
    }

    public bool IsTeamReady(int team)
    {
        // if (matchStarted) return true;

        int minPlayers = GetPlayersPerTeam(team);
        int minReady = GetTeamMinReady(team);
        (int playerCount, int readyCount) = GetTeamPlayerCount(team, false);

        Log($"[IsTeamReady] team: {team} minPlayers:{minPlayers} minReady:{minReady} playerCount:{playerCount} readyCount:{readyCount}");

        if (team == (int)CsTeam.Spectator && minReady == 0)
        {
            return true;
        }

        if (readyAvailable && playerCount == 0)
        {
            // We cannot ready for veto with no players, regardless of force status or min_players_to_ready.
            return false;
        }

        if (playerCount == readyCount && playerCount >= minPlayers)
        {
            return true;
        }

        if (IsTeamForcedReady((CsTeam)team) && readyCount >= minReady)
        {
            return true;
        }

        return false;
    }

    public int GetPlayersPerTeam(int team)
    {
        if (team == (int)CsTeam.CounterTerrorist || team == (int)CsTeam.Terrorist) return matchConfig.PlayersPerTeam;
        if (team == (int)CsTeam.Spectator) return matchConfig.MinSpectatorsToReady;
        return 0;
    }

    public int GetTeamMinReady(int team)
    {
        if (team == (int)CsTeam.CounterTerrorist || team == (int)CsTeam.Terrorist) return matchConfig.MinPlayersToReady;
        if (team == (int)CsTeam.Spectator) return matchConfig.MinSpectatorsToReady;
        return 0;
    }

    public (int, int) GetTeamPlayerCount(int team, bool includeCoaches = false)
    {
        int playerCount = 0;
        int readyCount = 0;
        foreach (var key in playerData.Keys)
        {
            if (!playerData[key].IsValid) continue;
            if (playerData[key].TeamNum == team) {
                playerCount++;
                if (playerReadyStatus[key] == true) readyCount++;
            }
        }
        return (playerCount, readyCount);
    }

    public bool IsTeamForcedReady(CsTeam team) {
        return teamReadyOverride[team];
    }

    /// <summary>
    /// Checks if both teams have the minimum required players and marks all players as ready if auto-ready is enabled.
    /// Only marks players as ready when both teams have at least MinPlayersToReady players.
    /// </summary>
    public void CheckAndAutoReadyPlayers()
    {
        // Skip if auto-ready is disabled, ready system not available, match started, or not in match setup
        if (!autoReadyEnabled.Value || !readyAvailable || matchStarted || !isMatchSetup)
        {
            return;
        }

        // Skip auto-ready in simulation mode - it has its own ready logic
        if (isSimulationMode)
        {
            return;
        }

        int minPlayersPerTeam = GetTeamMinReady((int)CsTeam.CounterTerrorist);
        
        // Ensure at least 1 player per team is required (prevent auto-ready with 0 players)
        if (minPlayersPerTeam <= 0)
        {
            minPlayersPerTeam = 1;
        }

        (int ctPlayerCount, int ctReadyCount) = GetTeamPlayerCount((int)CsTeam.CounterTerrorist, false);
        (int tPlayerCount, int tReadyCount) = GetTeamPlayerCount((int)CsTeam.Terrorist, false);

        Log($"[CheckAndAutoReadyPlayers] CT: {ctPlayerCount}/{minPlayersPerTeam} min required, T: {tPlayerCount}/{minPlayersPerTeam} min required");

        // Check if both teams have at least the minimum required players (respects min_players_to_ready)
        bool bothTeamsHaveMinimum = ctPlayerCount >= minPlayersPerTeam && tPlayerCount >= minPlayersPerTeam;

        if (!bothTeamsHaveMinimum)
        {
            Log($"[CheckAndAutoReadyPlayers] Teams don't have minimum players yet - CT: {ctPlayerCount}/{minPlayersPerTeam}, T: {tPlayerCount}/{minPlayersPerTeam}");
            return;
        }

        // Both teams are filled - mark all players on both teams as ready
        bool anyPlayerMarkedReady = false;

        foreach (var key in playerData.Keys)
        {
            if (!playerData[key].IsValid) continue;
            
            var p = playerData[key];
            // Only mark players on CT or T teams, skip spectators
            if (p.TeamNum == (int)CsTeam.CounterTerrorist || p.TeamNum == (int)CsTeam.Terrorist)
            {
                // Only mark as ready if they're not already ready
                if (!playerReadyStatus.ContainsKey(key) || !playerReadyStatus[key])
                {
                    playerReadyStatus[key] = true;
                    anyPlayerMarkedReady = true;

                    PrintToPlayerChat(p, Localizer["matchzy.autoready.markedready"]);
                    ShowPlayerNotification(p, "✅ AUTO-READY<br>Type .unready to opt-out", "#00ff00", 16);
                    SendPlayerReadyEvent(p, true);

                    Log($"[CheckAndAutoReadyPlayers] Marked {p.PlayerName} (TeamNum={p.TeamNum}) as ready");
                }
            }
        }

        if (anyPlayerMarkedReady)
        {
            Log($"[CheckAndAutoReadyPlayers] Both teams have minimum players ({minPlayersPerTeam}) - marked all players as ready");
            CheckLiveRequired();
            HandleClanTags();
        }
    }

    [ConsoleCommand("css_forceready", "Force-readies the team")]
    public void OnForceReadyCommandCommand(CCSPlayerController? player, CommandInfo? command)
    {
        Log($"{readyAvailable} {isMatchSetup} {allowForceReady} {IsPlayerValid(player)}");
        if (!readyAvailable || !isMatchSetup || !allowForceReady || !IsPlayerValid(player)) return;

        int minReady = GetTeamMinReady(player!.TeamNum);
        (int playerCount, int readyCount) = GetTeamPlayerCount(player!.TeamNum, false);

        if (playerCount < minReady) 
        {
            // ReplyToUserCommand(player, $"You must have at least {minReady} player(s) on the server to ready up.");
            ReplyToUserCommand(player, Localizer["matchzy.rs.minreadyplayers", minReady]);
            return;
        }

        foreach (var key in playerData.Keys)
        {
            if (!playerData[key].IsValid) continue;
            if (playerData[key].TeamNum == player.TeamNum) {
                playerReadyStatus[key] = true;
                // ReplyToUserCommand(playerData[key], $"Your team was force-readied by {player.PlayerName}");
                ReplyToUserCommand(playerData[key], Localizer["matchzy.rs.forcereadiedby", player.PlayerName]);
            }
        }

        teamReadyOverride[(CsTeam)player.TeamNum] = true;
        CheckLiveRequired();
    }
}
