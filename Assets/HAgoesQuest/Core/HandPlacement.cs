namespace HAgoesQuest.Core
{
    public enum PlacementPhase { AwaitRelease, Ready, Holding, Confirm }

    public sealed class HandPlacement
    {
        public PlacementPhase Phase { get; private set; }
        public HandPlacement() { Reset(); }
        public void Reset() { Phase = PlacementPhase.AwaitRelease; }
        public void Step(bool tracked, bool pinched)
        {
            if (Phase == PlacementPhase.Confirm) return;
            if (!tracked) { Phase = PlacementPhase.AwaitRelease; return; }
            switch (Phase)
            {
                case PlacementPhase.AwaitRelease:
                    if (!pinched) Phase = PlacementPhase.Ready;
                    break;
                case PlacementPhase.Ready:
                    if (pinched) Phase = PlacementPhase.Holding;
                    break;
                case PlacementPhase.Holding:
                    if (!pinched) Phase = PlacementPhase.Confirm;
                    break;
            }
        }
    }
}
