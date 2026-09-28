using System;
using HAgoesQuest.Core;

public static class CoreTests
{
    static int assertions;
    static void Check(bool condition, string name)
    {
        assertions++;
        if (!condition) throw new Exception(name);
    }
    static void Reject(Action action, string name)
    {
        bool rejected = false;
        try { action(); } catch (ArgumentException) { rejected = true; }
        Check(rejected, name);
    }
    public static int Main()
    {
        try
        {
            var gate = new FocusPinchGate();
            Check(!gate.Step("lamp", false, true, 0), "Looking alone does not activate");
            Check(gate.Step("lamp", true, true, .4), "Fresh pinch after dwell activates");
            Check(!gate.Step("lamp", true, true, 2), "Held pinch never repeats");
            Check(!gate.Step("other", true, true, 3), "Held pinch cannot transfer to another target");
            Check(!gate.Step("other", true, true, 4), "Dwell cannot activate a previously held pinch");
            gate.Step("other", false, true, 4.1);
            Check(gate.Step("other", true, true, 4.2), "Release re-arms target");
            gate.Step("other", false, true, 4.3);
            Check(!gate.Step("other", true, true, 4.4), "Cooldown rejects fast duplicate");
            gate.Step("other", false, true, 5);
            Check(!gate.Step("other", true, false, 5.1), "Tracking loss rejects input");
            Check(!gate.Step("other", true, true, 6), "Tracking recovery while pinched is not an edge");
            gate.Step("other", false, true, 6.1);
            Check(gate.Step("other", true, true, 6.5), "Release after recovery restores operation");
            gate.Reset();
            Check(!gate.Step("other", true, true, 8), "App resume while held does not fire");
            gate.Step("other", false, true, 8.1);
            Check(gate.Step("other", true, true, 8.5), "Resume requires new pinch");
            gate.Step(null, false, true, 9);
            Check(!gate.Step("lamp", true, true, 10), "Pinch on acquisition rejected");
            gate.Step("lamp", false, true, 11);
            Check(gate.Step("lamp", true, true, 11.1), "New edge fires after acquisition dwell");
            gate = new FocusPinchGate();
            gate.Step("lamp", false, true, 0);
            Check(!gate.Step("lamp", true, true, .1), "Early pinch rejected");
            Check(!gate.Step("lamp", true, true, 1), "Early pinch does not auto-fire when dwell expires");
            gate.Step("lamp", false, true, 1.1);
            Check(!gate.Step("other", true, true, 1.2), "Target changes restart dwell");

            Check(EntityPolicy.Service("light.desk", "off") == "turn_on", "Off light uses explicit turn_on");
            Check(EntityPolicy.Service("switch.desk", "on") == "turn_off", "On switch uses explicit turn_off");
            Check(!EntityPolicy.CanSwitch("sensor.temperature", "on"), "Sensors read only");
            Check(!EntityPolicy.CanSwitch("binary_sensor.door", "on"), "Binary sensors read only");
            Check(!EntityPolicy.CanSwitch("light.desk", "unavailable"), "Unavailable devices rejected");
            Check(!EntityPolicy.CanSwitch("light.desk", "unknown"), "Unknown state rejected");
            Check(!EntityPolicy.CanSwitch("lock.front_door", "off"), "Locks out of prototype scope");
            Check(!EntityPolicy.Visible("light.x/../../lock.front_door"), "Path injection rejected");
            Check(!EntityPolicy.Visible(null), "Null entity rejected");
            Check(EntityPolicy.Visible("sensor.temperature_2"), "Sensor supported");
            Check(EntityPolicy.Visible("climate.living_room"), "Other HA domains available for display and filtering");
            Check(EntityPolicy.Visible("lock.front_door") && !EntityPolicy.CanSwitch("lock.front_door", "on"), "Display support does not grant control of locks");

            Check(HaAddress.Parse("https://ha.example.com", false).AbsoluteUri == "https://ha.example.com/", "HTTPS base normalized");
            Check(HaAddress.Parse("http://192.168.1.10:8123", true).Port == 8123, "Explicit LAN HTTP works");
            Reject(() => HaAddress.Parse("http://192.168.1.10:8123", false), "HTTP opt-in required");
            Reject(() => HaAddress.Parse("https://user:secret@ha.example.com", false), "URL credentials rejected");
            Reject(() => HaAddress.Parse("https://ha.example.com/api", false), "API path rejected");
            Reject(() => HaAddress.Parse("https://ha.example.com/?token=x", false), "URL query rejected");
            Reject(() => HaAddress.Parse("https://ha.example.com/#x", false), "Fragment rejected");
            Reject(() => HaAddress.Parse("file:///tmp/file", false), "File URL rejected");
            Reject(() => HaAddress.Parse("ha.example.com", false), "Missing scheme rejected");
            var placement = new HandPlacement();
            Check(placement.Phase == PlacementPhase.AwaitRelease, "Placement waits for menu gesture release");
            placement.Step(true, true);
            Check(placement.Phase == PlacementPhase.AwaitRelease, "Menu pinch cannot start a drag");
            placement.Step(true, false);
            Check(placement.Phase == PlacementPhase.Ready, "Release readies placement");
            placement.Step(true, true);
            Check(placement.Phase == PlacementPhase.Holding, "New pinch grabs the entity");
            placement.Step(true, true);
            Check(placement.Phase == PlacementPhase.Holding, "Held pinch moves without committing");
            placement.Step(false, false);
            Check(placement.Phase == PlacementPhase.AwaitRelease, "Lost tracking cannot drop or save the entity");
            placement.Step(true, true);
            Check(placement.Phase == PlacementPhase.AwaitRelease, "Recovery with held pinch requires release");
            placement.Step(true, false);
            placement.Step(true, true);
            placement.Step(true, false);
            Check(placement.Phase == PlacementPhase.Confirm, "Tracked release requests confirmation");
            placement.Step(false, true);
            Check(placement.Phase == PlacementPhase.Confirm, "Confirmation keeps the dropped point stable");
            placement.Reset();
            Check(placement.Phase == PlacementPhase.AwaitRelease, "Moving again requires new gesture");
            Console.WriteLine("PASS: " + assertions + " assertions (interaction, service policy, connection address).");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL: " + ex.Message); return 1; }
    }
}
