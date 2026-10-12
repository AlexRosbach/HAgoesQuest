namespace HAgoesQuest.Core
{
    /// <summary>Assigns one hand to an action until both hands and the action are idle.</summary>
    public sealed class InteractionOwnership
    {
        /// <summary>Active hand: -1 for none, 0 for left, 1 for right.</summary>
        public int Owner { get; private set; }
        public InteractionOwnership(){Owner=-1;}
        double neutralSince = -1;
        /// <summary>Updates ownership without allowing a held second gesture to take over.</summary>
        /// <param name="left">Whether the left hand currently expresses an input intent.</param>
        /// <param name="right">Whether the right hand currently expresses an input intent.</param>
        /// <param name="busy">Whether an action still needs its current hand, including release confirmation.</param>
        /// <param name="preferLeft">Tie-breaker when both hands start together.</param>
        /// <param name="time">Monotonic unscaled time in seconds.</param>
        public void Step(bool left, bool right, bool busy, bool preferLeft, double time)
        {
            if (Owner < 0 && (left || right)) Owner = left && (!right || preferLeft) ? 0 : 1;
            if (left || right || busy) neutralSince = -1;
            else if (neutralSince < 0) neutralSince = time;
            else if (time - neutralSince >= .18) Owner = -1;
        }
    }
    /// <summary>Prevents the gesture that changed a page from activating the next page.</summary>
    public sealed class NavigationReleaseGate
    {
        double blockedUntil, neutralSince = -1;
        bool blocked;
        /// <summary>Starts a new settling interval and requires a fresh neutral input.</summary>
        public void Block(double time) { blocked = true; blockedUntil = time + .2; neutralSince = -1; }
        /// <summary>Returns true once navigation has settled and the previous input is released.</summary>
        /// <param name="active">Whether a pinch, trigger or near-menu touch is still active.</param>
        /// <param name="time">Monotonic unscaled time in seconds.</param>
        public bool Step(bool active, double time)
        {
            if (!blocked) return true;
            if (active) neutralSince = -1;
            else if (neutralSince < 0) neutralSince = time;
            if (time >= blockedUntil && neutralSince >= 0 && time - neutralSince >= .12) blocked = false;
            return !blocked;
        }
    }
}
