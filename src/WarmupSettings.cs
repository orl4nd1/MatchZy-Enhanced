using System;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;

namespace MatchZy
{
    public partial class MatchZy
    {
        private bool matchzyWarmupEnabled = false;
        private string matchzyWarmupMessageHtml = "";
        private bool matchzyWarmupRespawn = false;
        private bool matchzyWarmupIgnoreWinConditions = false;
        private float matchzyWarmupRoundtimeMinutes = 10.0f;
        private int matchzyWarmupStartmoney = 16000;
        private int matchzyWarmupMaxmoney = 16000;
        private bool matchzyWarmupBuyAnywhere = false;
        private bool matchzyWarmupInfiniteAmmo = false;

        private CounterStrikeSharp.API.Modules.Timers.Timer? matchzyWarmupMessageTimer = null;

        private static string JoinArgs(CommandInfo command, int startIndex)
        {
            if (command.ArgCount <= startIndex) return "";
            return string.Join(" ", Enumerable.Range(startIndex, command.ArgCount - startIndex).Select(command.ArgByIndex));
        }

        private bool CanApplyMatchzyWarmupSettings()
        {
            // Guardrail: these server-level warmup controls should only apply when the server is idle
            // and no match is currently loaded, so match configs remain authoritative.
            if (isMatchSetup) return false;
            var status = (tournamentStatus?.Value ?? "").Trim();
            if (!string.IsNullOrEmpty(status) && !string.Equals(status, "idle", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        private void ApplyMatchzyWarmupSettings(string reason)
        {
            if (!CanApplyMatchzyWarmupSettings())
            {
                Log($"[matchzy_warmup] Skipping apply ({reason}) - server not idle or match loaded.");
                return;
            }

            if (!matchzyWarmupEnabled)
            {
                matchzyWarmupMessageTimer?.Kill();
                matchzyWarmupMessageTimer = null;

                // Best-effort revert to sane defaults.
                Server.ExecuteCommand(
                    "mp_ignore_round_win_conditions 0;" +
                    "mp_respawn_on_death_ct 0;" +
                    "mp_respawn_on_death_t 0;" +
                    "mp_buy_anywhere 0;" +
                    "sv_infinite_ammo 0;" +
                    "mp_startmoney 800;" +
                    "mp_maxmoney 16000;" +
                    "mp_roundtime 1.92;" +
                    "mp_roundtime_defuse 1.92;" +
                    "mp_roundtime_hostage 1.92;"
                );

                Log($"[matchzy_warmup] Disabled ({reason})");
                return;
            }

            var ignoreWin = matchzyWarmupIgnoreWinConditions ? 1 : 0;
            var respawn = matchzyWarmupRespawn ? 1 : 0;
            var buy = matchzyWarmupBuyAnywhere ? 1 : 0;
            var infAmmo = matchzyWarmupInfiniteAmmo ? 2 : 0;
            var roundtime = Math.Clamp(matchzyWarmupRoundtimeMinutes, 1.0f, 120.0f);
            var startmoney = Math.Clamp(matchzyWarmupStartmoney, 0, 60000);
            var maxmoney = Math.Clamp(matchzyWarmupMaxmoney, 0, 60000);

            Server.ExecuteCommand(
                "mp_warmup_start;" +
                "mp_warmup_pausetimer 1;" +
                "mp_warmuptime 9999;" +
                $"mp_ignore_round_win_conditions {ignoreWin};" +
                $"mp_respawn_on_death_ct {respawn};" +
                $"mp_respawn_on_death_t {respawn};" +
                $"mp_roundtime {roundtime};" +
                $"mp_roundtime_defuse {roundtime};" +
                $"mp_roundtime_hostage {roundtime};" +
                $"mp_startmoney {startmoney};" +
                $"mp_maxmoney {maxmoney};" +
                $"mp_buy_anywhere {buy};" +
                $"sv_infinite_ammo {infAmmo};"
            );

            matchzyWarmupMessageTimer?.Kill();
            matchzyWarmupMessageTimer = null;
            if (!string.IsNullOrWhiteSpace(matchzyWarmupMessageHtml))
            {
                // Show immediately then repeat while enabled.
                PrintToCenterHtmlAll(matchzyWarmupMessageHtml);
                matchzyWarmupMessageTimer = AddTimer(10.0f, () =>
                {
                    if (!matchzyWarmupEnabled) return;
                    if (!string.IsNullOrWhiteSpace(matchzyWarmupMessageHtml))
                    {
                        PrintToCenterHtmlAll(matchzyWarmupMessageHtml);
                    }
                }, TimerFlags.REPEAT);
            }

            Log($"[matchzy_warmup] Applied settings ({reason})");
        }

        [ConsoleCommand("matchzy_warmup_enable", "Enable/disable server-level warmup settings (idle servers only). Usage: matchzy_warmup_enable 0|1")]
        public void MatchzyWarmupEnable(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                Log("[matchzy_warmup_enable] Usage: matchzy_warmup_enable 0|1");
                return;
            }

            var enable = raw != "0";
            matchzyWarmupEnabled = enable;
            database.SaveConfigValue("matchzy_warmup_enable", enable ? "1" : "0");
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_message_html", "Set center HTML message shown during warmup (idle servers only). Usage: matchzy_warmup_message_html <html|clear>")]
        public void MatchzyWarmupMessageHtml(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var value = JoinArgs(command, 1).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                Log("[matchzy_warmup_message_html] Usage: matchzy_warmup_message_html <html|clear>");
                return;
            }
            if (string.Equals(value, "clear", StringComparison.OrdinalIgnoreCase))
            {
                value = "";
            }

            matchzyWarmupMessageHtml = value;
            database.SaveConfigValue("matchzy_warmup_message_html", value);
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_respawn", "Enable/disable respawn on death during warmup (idle servers only). Usage: matchzy_warmup_respawn 0|1")]
        public void MatchzyWarmupRespawn(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                Log("[matchzy_warmup_respawn] Usage: matchzy_warmup_respawn 0|1");
                return;
            }
            matchzyWarmupRespawn = raw != "0";
            database.SaveConfigValue("matchzy_warmup_respawn", matchzyWarmupRespawn ? "1" : "0");
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_ignore_win_conditions", "Enable/disable ignore win conditions during warmup (idle servers only). Usage: matchzy_warmup_ignore_win_conditions 0|1")]
        public void MatchzyWarmupIgnoreWinConditions(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                Log("[matchzy_warmup_ignore_win_conditions] Usage: matchzy_warmup_ignore_win_conditions 0|1");
                return;
            }
            matchzyWarmupIgnoreWinConditions = raw != "0";
            database.SaveConfigValue("matchzy_warmup_ignore_win_conditions", matchzyWarmupIgnoreWinConditions ? "1" : "0");
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_roundtime_minutes", "Set warmup roundtime in minutes (idle servers only). Usage: matchzy_warmup_roundtime_minutes <1..120>")]
        public void MatchzyWarmupRoundtimeMinutes(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (!float.TryParse(raw, out var minutes))
            {
                Log("[matchzy_warmup_roundtime_minutes] Usage: matchzy_warmup_roundtime_minutes <1..120>");
                return;
            }
            matchzyWarmupRoundtimeMinutes = Math.Clamp(minutes, 1.0f, 120.0f);
            database.SaveConfigValue("matchzy_warmup_roundtime_minutes", matchzyWarmupRoundtimeMinutes.ToString());
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_startmoney", "Set warmup startmoney (idle servers only). Usage: matchzy_warmup_startmoney <0..60000>")]
        public void MatchzyWarmupStartmoney(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (!int.TryParse(raw, out var v))
            {
                Log("[matchzy_warmup_startmoney] Usage: matchzy_warmup_startmoney <0..60000>");
                return;
            }
            matchzyWarmupStartmoney = Math.Clamp(v, 0, 60000);
            database.SaveConfigValue("matchzy_warmup_startmoney", matchzyWarmupStartmoney.ToString());
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_maxmoney", "Set warmup maxmoney (idle servers only). Usage: matchzy_warmup_maxmoney <0..60000>")]
        public void MatchzyWarmupMaxmoney(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (!int.TryParse(raw, out var v))
            {
                Log("[matchzy_warmup_maxmoney] Usage: matchzy_warmup_maxmoney <0..60000>");
                return;
            }
            matchzyWarmupMaxmoney = Math.Clamp(v, 0, 60000);
            database.SaveConfigValue("matchzy_warmup_maxmoney", matchzyWarmupMaxmoney.ToString());
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_buy_anywhere", "Enable/disable buy anywhere during warmup (idle servers only). Usage: matchzy_warmup_buy_anywhere 0|1")]
        public void MatchzyWarmupBuyAnywhere(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                Log("[matchzy_warmup_buy_anywhere] Usage: matchzy_warmup_buy_anywhere 0|1");
                return;
            }
            matchzyWarmupBuyAnywhere = raw != "0";
            database.SaveConfigValue("matchzy_warmup_buy_anywhere", matchzyWarmupBuyAnywhere ? "1" : "0");
            ApplyMatchzyWarmupSettings("console");
        }

        [ConsoleCommand("matchzy_warmup_infinite_ammo", "Enable/disable infinite ammo during warmup (idle servers only). Usage: matchzy_warmup_infinite_ammo 0|1")]
        public void MatchzyWarmupInfiniteAmmo(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            var raw = command.ArgByIndex(1)?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                Log("[matchzy_warmup_infinite_ammo] Usage: matchzy_warmup_infinite_ammo 0|1");
                return;
            }
            matchzyWarmupInfiniteAmmo = raw != "0";
            database.SaveConfigValue("matchzy_warmup_infinite_ammo", matchzyWarmupInfiniteAmmo ? "1" : "0");
            ApplyMatchzyWarmupSettings("console");
        }
    }
}
