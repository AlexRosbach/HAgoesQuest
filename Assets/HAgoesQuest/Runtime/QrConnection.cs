using System;
using HAgoesQuest.Core;
using Newtonsoft.Json.Linq;

namespace HAgoesQuest
{
    public static class QrConnection
    {
        // QR contents are data only. Never open a scanned URL or send a token automatically.
        public static bool TryParse(string payload, out string url, out string token)
        {
            url = token = "";
            if (string.IsNullOrWhiteSpace(payload) || payload.Length > 8192) return false;
            payload = payload.Trim();
            try
            {
                if (payload.StartsWith("{"))
                {
                    var value = JObject.Parse(payload);
                    url = (string)value["baseUrl"] ?? (string)value["url"] ?? "";
                    token = (string)value["accessToken"] ?? (string)value["token"] ?? "";
                    if (url.Length > 0) HaAddress.Parse(url, true);
                }
                else token = payload;
                if (token.Length < 20 || token.Length > 4096 || Array.Exists(token.ToCharArray(), char.IsWhiteSpace))
                    throw new ArgumentException();
                // Reject links masquerading as raw tokens.
                if (token.Contains("://")) throw new ArgumentException();
                return true;
            }
            catch { url = token = ""; return false; }
        }
    }
}
