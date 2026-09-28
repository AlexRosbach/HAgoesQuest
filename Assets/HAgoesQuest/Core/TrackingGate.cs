namespace HAgoesQuest.Core
{
    // A localization flag alone does not mean that a current pose is tracked.
    public sealed class TrackingGate
    {
        double since = -1;
        public void Reset() { since = -1; }
        public bool Step(bool tracked, bool poseChanged, double now)
        {
            if (!tracked) { Reset(); return false; }
            if (since < 0 || poseChanged || now < since) since = now;
            return now - since >= 0.75;
        }
    }
}
