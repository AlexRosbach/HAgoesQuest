namespace HAgoesQuest.Core
{
    public sealed class InteractionOwnership
    {
        public int Owner { get; private set; }
        public InteractionOwnership(){Owner=-1;}
        double neutralSince = -1;
        public void Step(bool left, bool right, bool busy, bool preferLeft, double time)
        {
            if (Owner < 0 && (left || right)) Owner = left && (!right || preferLeft) ? 0 : 1;
            if (left || right || busy) neutralSince = -1;
            else if (neutralSince < 0) neutralSince = time;
            else if (time - neutralSince >= .18) Owner = -1;
        }
    }
    public sealed class NavigationReleaseGate
    {
        double blockedUntil, neutralSince = -1;
        bool blocked;
        public void Block(double time) { blocked = true; blockedUntil = time + .2; neutralSince = -1; }
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
