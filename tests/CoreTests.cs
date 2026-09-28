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
            var tracking = new TrackingGate();
            Check(!tracking.Step(false, false, 0), "Untracked startup anchor stays hidden");
            Check(!tracking.Step(true, false, 1), "First tracked pose is not displayed immediately");
            Check(tracking.Step(true, false, 1.8), "Stable tracked pose becomes visible");
            Check(!tracking.Step(false, false, 2), "Tracking loss hides an already localized anchor");
            Check(!tracking.Step(true, false, 3), "Recovery must settle again");
            Check(!tracking.Step(true, true, 3.6), "Relocalization jump restarts stability interval");
            Check(!tracking.Step(true, false, 4), "Jump cannot use previous tracked interval");
            Check(tracking.Step(true, false, 4.4), "Recovered anchor is visible after settling");
            tracking.Reset();
            Check(!tracking.Step(true, false, 10), "Recenter invalidates a previously stable pose");
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
            double magicTime;
            var menuToggle = new FocusPinchGate(0, .25);
            menuToggle.Step("menu", false, true, 0);
            Check(menuToggle.Step("menu", true, true, .3), "Left pinch opens menu");
            Check(!menuToggle.Step("menu", true, true, 2), "Held left pinch cannot toggle twice");
            menuToggle.Step("menu", false, true, 2.1);
            Check(menuToggle.Step("menu", true, true, 2.2), "Next released left pinch closes menu");
            menuToggle.Reset();
            Check(!menuToggle.Step("menu", true, true, 3), "Held pinch after calibration cannot toggle until release");
            menuToggle.Step("menu", false, true, 3.1);
            Check(menuToggle.Step("menu", true, true, 3.2), "Release restores menu control after calibration");
            var magic = ArmedMagic(out magicTime);
            Check(magic.Armed, "Open stationary palm arms after dwell");
            Check(!magic.Step("light.test", true, true, new PushVector(0,0,.05), new PushVector(0,0,1), magicTime+=.05), "Short push does not fire");
            Check(!magic.Step("light.test", true, true, new PushVector(0,0,.10), new PushVector(0,0,1), magicTime+=.05), "Push must reach twelve centimeters");
            Check(magic.Step("light.test", true, true, new PushVector(0,0,.15), new PushVector(0,0,1), magicTime+=.05), "Directed open-palm thrust casts once");
            bool repeated = false;
            for (int i=0;i<60;i++) repeated |= magic.Step("light.test", true, true, new PushVector(0,0,.15), new PushVector(0,0,1), magicTime+=.05);
            Check(!repeated, "Holding extended hand cannot repeatedly toggle");
            magic.Step("light.test", false, false, new PushVector(), new PushVector(0,0,1), magicTime+=.05);
            for (int i=0;i<20;i++) repeated |= magic.Step("light.test", true, true, new PushVector(0,0,.15), new PushVector(0,0,1), magicTime+=.05);
            Check(!repeated && !magic.Armed, "Tracking loss does not release consumed gesture");
            magic.Step("light.test", true, false, new PushVector(), new PushVector(0,0,1), magicTime+=.05);
            for (int i=0;i<10;i++) magic.Step("light.test", true, true, new PushVector(), new PushVector(0,0,1), magicTime+=.05);
            Check(magic.Armed, "Closing and reopening after cooldown permits another cast");
            magic = ArmedMagic(out magicTime);
            Check(!magic.Step("light.test", true, true, new PushVector(.15,0,0), new PushVector(0,0,1), magicTime+=.05) && !magic.Armed, "Sideways swipe rejected");
            magic = ArmedMagic(out magicTime);
            Check(!magic.Step("light.test", true, true, new PushVector(0,0,-.15), new PushVector(0,0,1), magicTime+=.05), "Backward motion rejected");
            magic = ArmedMagic(out magicTime);
            Check(!magic.Step("other", true, true, new PushVector(0,0,.15), new PushVector(0,0,1), magicTime+=.05), "Target change cannot cast");
            magic = ArmedMagic(out magicTime);
            Check(!magic.Step("light.test", true, true, new PushVector(0,0,1), new PushVector(0,0,1), magicTime+=.05), "Tracking position jump rejected");
            magic = ArmedMagic(out magicTime);
            Check(!magic.Step("light.test", true, true, new PushVector(0,0,.15), new PushVector(0,0,1), magicTime+=1), "Long sample gap resets recognition");
            magic = ArmedMagic(out magicTime);
            Check(!magic.Step("light.test", false, true, new PushVector(0,0,.15), new PushVector(0,0,1), magicTime+=.05), "Disabled/menu/offline input cannot cast");
            magic = ArmedMagic(out magicTime); repeated = false;
            for (int i=1;i<=20;i++) repeated |= magic.Step("light.test", true, true, new PushVector(0,0,i*.01), new PushVector(0,0,1), magicTime+=.05);
            Check(!repeated, "Slow reach cannot cast");
            magic = new MagicPushGate(); magicTime = 0;
            int spells = 0;
            for (int shot=0;shot<3;shot++)
            {
                magic.Step(null, true, true, new PushVector(), new PushVector(0,0,1), magicTime+=.05);
                for (int frame=0;frame<10;frame++) magic.Step("light.test", true, true, new PushVector(), new PushVector(0,0,1), magicTime+=.05);
                for (int frame=1;frame<=3;frame++) if (magic.Step("light.test", true, true, new PushVector(0,0,frame*.05), new PushVector(0,0,1), magicTime+=.05)) spells++;
            }
            Check(spells == 3, "Three separate forward thrusts cast consecutively without closing hand");
            Console.WriteLine("PASS: " + assertions + " assertions (interaction, magic gestures, service policy, connection address).");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL: " + ex.Message); return 1; }
    }
    static MagicPushGate ArmedMagic(out double time)
    {
        var gate = new MagicPushGate(); time=0;
        for (int i=0;i<9;i++) gate.Step("light.test", true, true, new PushVector(), new PushVector(0,0,1), time=i*.05);
        return gate;
    }
}
