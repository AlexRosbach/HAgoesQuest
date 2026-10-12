using System;
using System.Text.RegularExpressions;

namespace HAgoesQuest.Core
{
    /// <summary>Accepts a fresh pinch after target dwell and cooldown; a held pinch never repeats.</summary>
    public sealed class FocusPinchGate
    {
        readonly double dwell;
        readonly double cooldown;
        string target;
        double since;
        double nextAllowed;
        bool wasDown;
        bool released;

        public FocusPinchGate(double dwellSeconds = 0.35, double cooldownSeconds = 0.7)
        {
            dwell = dwellSeconds;
            cooldown = cooldownSeconds;
        }

        public bool Step(string current, bool down, bool tracked, double now)
        {
            if (!tracked || String.IsNullOrEmpty(current))
            {
                target = null;
                released = false;
                wasDown = down;
                return false;
            }
            if (target != current)
            {
                target = current;
                since = now;
                released = !down;
            }
            if (!down) released = true;
            bool fire = down && !wasDown && released && now - since >= dwell && now >= nextAllowed;
            wasDown = down;
            if (down) released = false;
            if (fire) nextAllowed = now + cooldown;
            return fire;
        }

        public void Reset()
        {
            target = null;
            released = false;
            wasDown = true;
        }
    }

    /// <summary>Separates displayable HA IDs from the restricted light/switch command policy.</summary>
    public static class EntityPolicy
    {
        static readonly Regex IdPattern = new Regex(@"^[a-z0-9_]+\.[a-z0-9_]+$");
        public static string Domain(string id)
        {
            return id != null && IdPattern.IsMatch(id) ? id.Substring(0, id.IndexOf('.')) : "";
        }
        public static bool Visible(string id)
        {
            string d = Domain(id);
            return d.Length > 0; // All HA domains can be displayed; commands remain restricted below.
        }
        public static bool CanSwitch(string id, string state)
        {
            string d = Domain(id);
            return (d == "light" || d == "switch") && (state == "on" || state == "off");
        }
        /// <summary>Returns the explicit service that reverses a known on/off state.</summary>
        /// <exception cref="InvalidOperationException">The entity or state cannot be switched.</exception>
        public static string Service(string id, string state)
        {
            if (!CanSwitch(id, state)) throw new InvalidOperationException("Entity cannot be switched.");
            // Explicit state avoids a duplicate toggle reversing an earlier request.
            return state == "on" ? "turn_off" : "turn_on";
        }
    }

    /// <summary>Validates a root HA origin before credentials are attached to requests.</summary>
    public static class HaAddress
    {
        /// <summary>Returns an HTTP(S) base URI without paths, credentials, query or fragment.</summary>
        /// <exception cref="ArgumentException">The URL is invalid or HTTP has not been explicitly allowed.</exception>
        public static Uri Parse(string value, bool allowInsecureHttp)
        {
            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri) ||
                (uri.Scheme != "https" && uri.Scheme != "http") ||
                !String.IsNullOrEmpty(uri.UserInfo) || !String.IsNullOrEmpty(uri.Query) ||
                !String.IsNullOrEmpty(uri.Fragment) || uri.AbsolutePath != "/")
                throw new ArgumentException("Use the Home Assistant base URL, without path, credentials or query.");
            if (uri.Scheme == "http" && !allowInsecureHttp)
                throw new ArgumentException("HTTP requires explicit allowInsecureHttp in the connection file.");
            return uri;
        }
    }
}
