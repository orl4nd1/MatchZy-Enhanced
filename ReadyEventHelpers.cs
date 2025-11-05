using CounterStrikeSharp.API.Core;

namespace MatchZy;

public partial class MatchZy
{
    private void SendPlayerReadyEvent(CCSPlayerController player, bool isReady)
    {
        if (!isMatchSetup || string.IsNullOrEmpty(matchConfig.RemoteLogURL)) return;
        if (!player.UserId.HasValue) return;

        // Get ready counts
        (int team1PlayerCount, int team1ReadyCount) = GetTeamPlayerCount((int)CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist, false);
        (int team2PlayerCount, int team2ReadyCount) = GetTeamPlayerCount((int)CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist, false);

        // Expected total is players per team * 2
        int expectedTotal = matchConfig.PlayersPerTeam * 2;

        // Determine which team this player is on
        string teamName = "none";
        if (reverseTeamSides.ContainsKey("CT") && player.TeamNum == 3)
        {
            teamName = reverseTeamSides["CT"].teamName;
        }
        else if (reverseTeamSides.ContainsKey("TERRORIST") && player.TeamNum == 2)
        {
            teamName = reverseTeamSides["TERRORIST"].teamName;
        }

        var playerInfo = new MatchZyPlayerInfo(
            player.SteamID.ToString(),
            player.PlayerName,
            teamName
        );

        if (isReady)
        {
            var readyEvent = new MatchZyPlayerReadyEvent
            {
                MatchId = liveMatchId,
                Player = playerInfo,
                Team = teamName,
                ReadyCountTeam1 = reverseTeamSides.ContainsKey("CT") && reverseTeamSides["CT"] == matchzyTeam1 ? team1ReadyCount : team2ReadyCount,
                ReadyCountTeam2 = reverseTeamSides.ContainsKey("CT") && reverseTeamSides["CT"] == matchzyTeam2 ? team1ReadyCount : team2ReadyCount,
                TotalReady = team1ReadyCount + team2ReadyCount,
                ExpectedTotal = expectedTotal
            };

            Task.Run(async () => {
                await SendEventAsync(readyEvent);
            });

            // Check if team is now ready
            CheckAndSendTeamReadyEvent();
        }
        else
        {
            var unreadyEvent = new MatchZyPlayerUnreadyEvent
            {
                MatchId = liveMatchId,
                Player = playerInfo,
                Team = teamName,
                ReadyCountTeam1 = reverseTeamSides.ContainsKey("CT") && reverseTeamSides["CT"] == matchzyTeam1 ? team1ReadyCount : team2ReadyCount,
                ReadyCountTeam2 = reverseTeamSides.ContainsKey("CT") && reverseTeamSides["CT"] == matchzyTeam2 ? team1ReadyCount : team2ReadyCount,
                TotalReady = team1ReadyCount + team2ReadyCount,
                ExpectedTotal = expectedTotal
            };

            Task.Run(async () => {
                await SendEventAsync(unreadyEvent);
            });
        }
    }

    private void CheckAndSendTeamReadyEvent()
    {
        if (!isMatchSetup || string.IsNullOrEmpty(matchConfig.RemoteLogURL)) return;

        bool team1Ready = IsTeamReady((int)CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist);
        bool team2Ready = IsTeamReady((int)CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist);

        (int team1PlayerCount, int team1ReadyCount) = GetTeamPlayerCount((int)CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist, false);
        (int team2PlayerCount, int team2ReadyCount) = GetTeamPlayerCount((int)CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist, false);

        int expectedTotal = matchConfig.PlayersPerTeam * 2;
        int totalReady = team1ReadyCount + team2ReadyCount;

        // Send team_ready event for CT team if ready
        if (team1Ready && reverseTeamSides.ContainsKey("CT"))
        {
            var teamReadyEvent = new MatchZyTeamReadyEvent
            {
                MatchId = liveMatchId,
                Team = reverseTeamSides["CT"] == matchzyTeam1 ? "team1" : "team2",
                ReadyCount = team1ReadyCount,
                TotalReady = totalReady,
                ExpectedTotal = expectedTotal
            };

            Task.Run(async () => {
                await SendEventAsync(teamReadyEvent);
            });
        }

        // Send team_ready event for T team if ready
        if (team2Ready && reverseTeamSides.ContainsKey("TERRORIST"))
        {
            var teamReadyEvent = new MatchZyTeamReadyEvent
            {
                MatchId = liveMatchId,
                Team = reverseTeamSides["TERRORIST"] == matchzyTeam1 ? "team1" : "team2",
                ReadyCount = team2ReadyCount,
                TotalReady = totalReady,
                ExpectedTotal = expectedTotal
            };

            Task.Run(async () => {
                await SendEventAsync(teamReadyEvent);
            });
        }

        // Send all_players_ready event if both teams are ready
        if (team1Ready && team2Ready)
        {
            var allPlayersReadyEvent = new MatchZyAllPlayersReadyEvent
            {
                MatchId = liveMatchId,
                ReadyCountTeam1 = team1ReadyCount,
                ReadyCountTeam2 = team2ReadyCount,
                TotalReady = totalReady,
                CountdownStarted = true
            };

            Task.Run(async () => {
                await SendEventAsync(allPlayersReadyEvent);
            });
        }
    }
}

