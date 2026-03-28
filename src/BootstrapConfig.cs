using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace MatchZy
{
    public partial class MatchZy
    {
        private sealed class BootstrapPayload
        {
            public bool success { get; set; }
            public string? serverId { get; set; }
            public string[]? commands { get; set; }
        }

        [ConsoleCommand("matchzy_bootstrap_url", "HTTP URL to fetch bootstrap config payload (server-only)")]
        public void MatchZyBootstrapUrl(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            string url = command.ArgByIndex(1);

            if (string.IsNullOrWhiteSpace(url))
            {
                Log("[MatchZyBootstrapUrl] Usage: matchzy_bootstrap_url <url>");
                return;
            }

            bootstrapUrl = url.Trim();
            database.SaveConfigValue("matchzy_bootstrap_url", bootstrapUrl);
            Log("[MatchZyBootstrapUrl] Bootstrap URL set and persisted to database");

            TryBootstrapFetch("console");
        }

        [ConsoleCommand("matchzy_bootstrap_token", "Authentication token for bootstrap endpoint (sent as X-MatchZy-Token)")]
        public void MatchZyBootstrapToken(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null) return;
            string token = command.ArgByIndex(1);

            if (string.IsNullOrWhiteSpace(token))
            {
                Log("[MatchZyBootstrapToken] Usage: matchzy_bootstrap_token <token>");
                return;
            }

            bootstrapToken = token.Trim();
            database.SaveConfigValue("matchzy_bootstrap_token", bootstrapToken);
            Log("[MatchZyBootstrapToken] Bootstrap token set and persisted to database");

            TryBootstrapFetch("console");
        }

        private void TryBootstrapFetch(string reason)
        {
            try
            {
                if (bootstrapFetchInProgress) return;
                if (string.IsNullOrWhiteSpace(bootstrapUrl)) return;
                if (string.IsNullOrWhiteSpace(bootstrapToken)) return;

                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (lastBootstrapAttemptAt > 0 && now - lastBootstrapAttemptAt < 5)
                {
                    return; // anti-spam
                }
                lastBootstrapAttemptAt = now;
                bootstrapFetchInProgress = true;

                Task.Run(async () =>
                {
                    try
                    {
                        using var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("X-MatchZy-Token", bootstrapToken);

                        var response = await httpClient.GetAsync(bootstrapUrl);
                        var body = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            Log($"[Bootstrap] Fetch failed ({(int)response.StatusCode}): {body}");
                            return;
                        }

                        BootstrapPayload? payload = null;
                        try
                        {
                            payload = JsonSerializer.Deserialize<BootstrapPayload>(body);
                        }
                        catch (Exception ex)
                        {
                            Log($"[Bootstrap] Failed to parse payload JSON: {ex.Message}");
                            return;
                        }

                        if (payload?.commands == null || payload.commands.Length == 0)
                        {
                            Log("[Bootstrap] Payload contained no commands; skipping");
                            return;
                        }

                        // Apply on main thread.
                        Server.NextFrame(() =>
                        {
                            Log($"[Bootstrap] Applying {payload.commands.Length} bootstrap commands ({reason})");
                            foreach (var cmd in payload.commands)
                            {
                                if (string.IsNullOrWhiteSpace(cmd)) continue;
                                Server.ExecuteCommand(cmd);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Log($"[Bootstrap] Fetch/apply error: {ex.Message}");
                    }
                    finally
                    {
                        bootstrapFetchInProgress = false;
                    }
                });
            }
            catch
            {
                // best-effort only
                bootstrapFetchInProgress = false;
            }
        }
    }
}

