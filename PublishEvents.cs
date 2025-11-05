using System.Text;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;


namespace MatchZy
{
    public partial class MatchZy
    {
        public async Task SendEventAsync(MatchZyEvent @event)
        {
            try
            {
                if (string.IsNullOrEmpty(matchConfig.RemoteLogURL)) return;

                Log($"[SendEventAsync] Sending Event: {@event.EventName} for matchId: {liveMatchId} mapNumber: {matchConfig.CurrentMapNumber} on {matchConfig.RemoteLogURL}");
                
                // Print to server console AND chat for visibility
                Server.NextFrame(() => {
                    Server.PrintToConsole($"[MatchZy Events] Sending '{@event.EventName}' to {matchConfig.RemoteLogURL}");
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Grey}Event:{ChatColors.Default} {ChatColors.Lime}{@event.EventName}{ChatColors.Default} → {ChatColors.Grey}{GetShortUrl(matchConfig.RemoteLogURL)}");
                });

                using var httpClient = new HttpClient();
                using var jsonContent = new StringContent(JsonSerializer.Serialize(@event, @event.GetType()), Encoding.UTF8, "application/json");

                string jsonString = await jsonContent.ReadAsStringAsync();

                Log($"[SendEventAsync] SENDING DATA: {jsonString}");

                if (!string.IsNullOrEmpty(matchConfig.RemoteLogHeaderKey) && !string.IsNullOrEmpty(matchConfig.RemoteLogHeaderValue))
                {
                    httpClient.DefaultRequestHeaders.Add(matchConfig.RemoteLogHeaderKey, matchConfig.RemoteLogHeaderValue);
                }

                var httpResponseMessage = await httpClient.PostAsync(matchConfig.RemoteLogURL, jsonContent);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    Log($"[SendEventAsync] Sending {@event.EventName} for matchId: {liveMatchId} mapNumber: {matchConfig.CurrentMapNumber} successful with status code: {httpResponseMessage.StatusCode}");
                    
                    // Print success to console and chat
                    Server.NextFrame(() => {
                        Server.PrintToConsole($"[MatchZy Events] ✓ '{@event.EventName}' sent successfully ({httpResponseMessage.StatusCode})");
                        Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}✓{ChatColors.Default} {ChatColors.Lime}{@event.EventName}{ChatColors.Default} sent");
                    });
                }
                else
                {
                    string errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    Log($"[SendEventAsync] Sending {@event.EventName} for matchId: {liveMatchId} mapNumber: {matchConfig.CurrentMapNumber} failed with status code: {httpResponseMessage.StatusCode}, ResponseContent: {errorContent}");
                    
                    // Print error to console and chat
                    Server.NextFrame(() => {
                        Server.PrintToConsole($"[MatchZy Events] ✗ FAILED to send '{@event.EventName}' (HTTP {httpResponseMessage.StatusCode})");
                        Server.PrintToConsole($"[MatchZy Events] Error: {errorContent}");
                        Server.PrintToChatAll($"{chatPrefix} {ChatColors.Red}✗{ChatColors.Default} {ChatColors.Lime}{@event.EventName}{ChatColors.Default} {ChatColors.Red}FAILED{ChatColors.Default} ({httpResponseMessage.StatusCode})");
                    });
                }
            }
            catch (Exception e)
            {
                Log($"[SendEventAsync FATAL] An error occurred: {e.Message}");
                
                // Print exception to console and chat
                Server.NextFrame(() => {
                    Server.PrintToConsole($"[MatchZy Events] ✗ EXCEPTION sending '{@event.EventName}': {e.Message}");
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Red}✗{ChatColors.Default} {ChatColors.Lime}{@event.EventName}{ChatColors.Default} {ChatColors.Red}ERROR:{ChatColors.Default} {e.Message}");
                });
            }
        }

        private string GetShortUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host + uri.PathAndQuery;
            }
            catch
            {
                return url.Length > 30 ? url.Substring(0, 27) + "..." : url;
            }
        }
    }
}
