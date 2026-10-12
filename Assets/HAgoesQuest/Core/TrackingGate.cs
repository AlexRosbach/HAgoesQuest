namespace HAgoesQuest.Core
{
    /// <summary>Requires 0.75 seconds of stable tracked pose before allowing visibility.</summary>
    /// <remarks>A localized anchor alone does not prove that its current pose is tracked.</remarks>
    public sealed class TrackingGate
    {
        double since = -1;
        public void Reset() { since = -1; }
        /// <summary>Returns false until tracking has settled; movement restarts the interval.</summary>
        /// <param name="tracked">Whether the current pose is valid and tracked.</param>
        /// <param name="poseChanged">Whether movement exceeded the caller's stability threshold.</param>
        /// <param name="now">Unscaled time in seconds; a backwards clock restarts settling.</param>
        public bool Step(bool tracked, bool poseChanged, double now)
        {
            if (!tracked) { Reset(); return false; }
            if (since < 0 || poseChanged || now < since) since = now;
            return now - since >= 0.75;
        }
    }
}
