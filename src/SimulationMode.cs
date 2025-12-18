using System;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json.Linq;

namespace MatchZy;

// Internal identity for a simulated player: the "real" identity from the match JSON
// that a given in-game bot is representing.
internal record SimulationPlayerIdentity(string ConfigSteamId, string ConfigName, string TeamSlot);

public partial class MatchZy
{
    // Mapping from CS2 userId -> configured player identity for simulation mode.
    private readonly Dictionary<int, SimulationPlayerIdentity> simulationPlayersByUserId = new();

    // Flat pool of configured players across both teams.
    private readonly List<SimulationPlayerIdentity> simulationIdentityPool = new();

    // Tracks which configured SteamIDs have already been assigned to a bot.
    private readonly HashSet<string> assignedSimulationSteamIds = new();

    // Ensure we only start the simulated ready flow once.
    private bool simulationReadyFlowScheduled = false;

    private void ClearSimulationState()
    {
        simulationPlayersByUserId.Clear();
        simulationIdentityPool.Clear();
        assignedSimulationSteamIds.Clear();
        simulationReadyFlowScheduled = false;
        isSimulationMode = false;
    }

    /// <summary>
    /// Build the list of configured players from the loaded JSON config.
    /// This should be called once per match before any bots are mapped.
    /// </summary>
    private void BuildSimulationConfigPlayers()
    {
        simulationIdentityPool.Clear();
        assignedSimulationSteamIds.Clear();
        simulationPlayersByUserId.Clear();

        void AddFromTeam(JToken? teamPlayers, string teamSlot)
        {
            if (teamPlayers is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    string steamId = prop.Name;
                    string name = prop.Value?.ToString() ?? "";
                    simulationIdentityPool.Add(new SimulationPlayerIdentity(steamId, name, teamSlot));
                }
            }
        }

        AddFromTeam(matchzyTeam1.teamPlayers, "team1");
        AddFromTeam(matchzyTeam2.teamPlayers, "team2");

        Log($"[SimulationMode] Built simulation config players - total: {simulationIdentityPool.Count}");
    }

    /// <summary>
    /// Assigns a configured simulation identity to the given bot, if available.
    /// Called from the player connect handler when a bot joins in simulation mode.
    /// </summary>
    private SimulationPlayerIdentity? AssignSimulationIdentityForBot(CCSPlayerController player)
    {
        if (!isSimulationMode || !player.IsBot || !player.UserId.HasValue)
        {
            return null;
        }

        int userId = player.UserId.Value;
        if (simulationPlayersByUserId.TryGetValue(userId, out var existing))
        {
            return existing;
        }

        foreach (var identity in simulationIdentityPool)
        {
            if (!assignedSimulationSteamIds.Contains(identity.ConfigSteamId))
            {
                assignedSimulationSteamIds.Add(identity.ConfigSteamId);
                simulationPlayersByUserId[userId] = identity;
                Log($"[SimulationMode] Assigned bot {player.PlayerName} (UserId {userId}) to simulated player {identity.ConfigName} ({identity.ConfigSteamId}) on {identity.TeamSlot}");
                return identity;
            }
        }

        Log($"[SimulationMode] No available simulated player identity for bot {player.PlayerName} (UserId {userId})");
        return null;
    }

    /// <summary>
    /// Helper to build MatchZyPlayerInfo, respecting simulation mappings when enabled.
    /// </summary>
    private MatchZyPlayerInfo BuildPlayerInfo(CCSPlayerController player, string teamLabelFallback)
    {
        if (isSimulationMode && player.UserId.HasValue &&
            simulationPlayersByUserId.TryGetValue(player.UserId.Value, out var identity))
        {
            return new MatchZyPlayerInfo(identity.ConfigSteamId, identity.ConfigName, identity.TeamSlot);
        }

        return new MatchZyPlayerInfo(player.SteamID.ToString(), player.PlayerName, teamLabelFallback);
    }

    /// <summary>
    /// Spawns one bot per configured player and assigns them to the appropriate CS team.
    /// </summary>
    private void SpawnSimulationBots()
    {
        if (!isSimulationMode || simulationIdentityPool.Count == 0)
        {
            return;
        }

        Log("[SimulationMode] Spawning simulation bots.");

        // Clean up any existing bots and ensure server cvars won't interfere.
        Server.ExecuteCommand("bot_kick");

        // For simulation we want exactly one bot per configured player, and we don't want
        // the engine to immediately kick them again due to a zero bot_quota. Set a sane
        // quota and disable auto-balance / auto-kick.
        int desiredBotCount = simulationIdentityPool.Count;
        if (desiredBotCount < 0) desiredBotCount = 0;

        Server.ExecuteCommand($"mp_autoteambalance 0; mp_limitteams 0; mp_autokick 0; bot_quota_mode normal; bot_quota {desiredBotCount}");

        foreach (var identity in simulationIdentityPool)
        {
            // Decide desired side based on team slot and current teamSides mapping.
            string desiredSide = "T";
            if (identity.TeamSlot == "team1")
            {
                desiredSide = teamSides.TryGetValue(matchzyTeam1, out var side) ? side : "CT";
            }
            else if (identity.TeamSlot == "team2")
            {
                desiredSide = teamSides.TryGetValue(matchzyTeam2, out var side) ? side : "TERRORIST";
            }

            if (desiredSide == "CT")
            {
                Server.ExecuteCommand("bot_join_team CT");
                Server.ExecuteCommand("bot_add_ct");
            }
            else
            {
                Server.ExecuteCommand("bot_join_team T");
                Server.ExecuteCommand("bot_add_t");
            }
        }
    }

    /// <summary>
    /// Starts the simulated ready flow: bots gradually "ready up" and then the match goes live.
    /// </summary>
    private void StartSimulationReadyFlow()
    {
        if (!isSimulationMode || simulationReadyFlowScheduled)
        {
            return;
        }

        simulationReadyFlowScheduled = true;

        var userIds = new List<int>(simulationPlayersByUserId.Keys);
        if (userIds.Count == 0)
        {
            // If there are no configured simulation identities at all, this indicates a
            // genuinely broken simulation configuration (no team/player info). Treat that
            // as a hard error for visibility.
            if (simulationIdentityPool.Count == 0)
            {
                Log("[SimulationMode] ERROR: Simulation mode enabled but no configured players were found in team1/team2.");
                UpdateTournamentStatus("error");
                isSimulationMode = false;
                return;
            }

            // Otherwise, we have valid configured players but no bot mappings yet. This can
            // happen transiently if bots are still connecting. Fall back to a simple
            // team-level auto-ready so the match can proceed end-to-end without human input,
            // but do not treat this as a terminal error.
            Log("[SimulationMode] No mapped simulation players found for ready flow; falling back to team-level auto-ready based on configured players.");

            teamReadyOverride[CsTeam.CounterTerrorist] = true;
            teamReadyOverride[CsTeam.Terrorist] = true;
            CheckAndSendTeamReadyEvent();
            return;
        }

        if (userIds.Count < simulationIdentityPool.Count)
        {
            Log($"[SimulationMode] Warning: Only {userIds.Count} of {simulationIdentityPool.Count} simulated players were mapped to bots.");
        }

        userIds.Sort();

        Log($"[SimulationMode] Starting simulated ready flow for {userIds.Count} players.");

        float delayStep = 0.5f;
        for (int i = 0; i < userIds.Count; i++)
        {
            int userId = userIds[i];
            float delay = i * delayStep;

            AddTimer(delay, () =>
            {
                if (!playerData.TryGetValue(userId, out var player) || !IsPlayerValid(player))
                {
                    return;
                }

                // Mimic the !ready command, which will:
                // - Mark the player as ready
                // - Send player_ready to the API
                // - Potentially trigger CheckLiveRequired and clan tag updates
                OnPlayerReady(player, null);
            });
        }

        float totalDelay = userIds.Count * delayStep + 0.25f;
        AddTimer(totalDelay, () =>
        {
            // Ensure team-level ready state is satisfied and events are emitted.
            teamReadyOverride[CsTeam.CounterTerrorist] = true;
            teamReadyOverride[CsTeam.Terrorist] = true;

            CheckAndSendTeamReadyEvent();
        });
    }

    // Entry point for any simulation-only orchestration after a match is loaded.
    // This drives bot spawning and the simulated ready flow.
    private void MaybeStartSimulationFlow()
    {
        if (!isSimulationMode)
        {
            return;
        }

        Log("[SimulationMode] Simulation mode enabled for this match. Initializing simulation state.");

        // Prepare the configured identities that bots will represent.
        BuildSimulationConfigPlayers();

        // Spawn one bot per configured player and, after they connect,
        // start the simulated ready flow.
        SpawnSimulationBots();

        // Give bots a short time window to connect and be mapped, then drive ready flow.
        // A slightly larger delay here makes it more robust when servers are under load.
        AddTimer(4.0f, StartSimulationReadyFlow);
    }

    /// <summary>
    /// After a simulated series concludes, gracefully disconnect bots.
    /// Waits ~30 seconds, then kicks bots one by one at random intervals
    /// so that they appear to leave the server gradually.
    /// </summary>
    private void ScheduleSimulationBotDisconnects()
    {
        if (!isSimulationMode)
        {
            return;
        }

        const float initialDelaySeconds = 30.0f;

        Log("[SimulationMode] Scheduling simulated bot disconnects after series end.");

        // First wait a short fixed period after series end.
        AddTimer(initialDelaySeconds, () =>
        {
            if (!isSimulationMode)
            {
                return;
            }

            var bots = new List<CCSPlayerController>();

            foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
            {
                if (player == null) continue;
                if (!player.IsValid || !player.IsBot || player.IsHLTV) continue;
                if (player.Connected != PlayerConnectedState.PlayerConnected) continue;

                bots.Add(player);
            }

            if (bots.Count == 0)
            {
                Log("[SimulationMode] No bots found to disconnect after series end.");
                return;
            }

            Log($"[SimulationMode] Disconnecting {bots.Count} simulation bots over a random interval.");

            // Kick bots one by one with a small random delay between each
            var random = new Random();
            float accumulatedDelay = 0.0f;

            foreach (var bot in bots)
            {
                // Random interval between 1 and 5 seconds for each bot
                float interval = 1.0f + (float)(random.NextDouble() * 4.0);
                accumulatedDelay += interval;

                var botRef = bot;
                AddTimer(accumulatedDelay, () =>
                {
                    if (!isSimulationMode)
                    {
                        return;
                    }

                    if (!IsPlayerValid(botRef) || !botRef.IsBot)
                    {
                        return;
                    }

                    Log($"[SimulationMode] Disconnecting simulation bot {botRef.PlayerName} (UserId {botRef.UserId}).");
                    KickPlayer(botRef);
                });
            }
        });
    }
}


