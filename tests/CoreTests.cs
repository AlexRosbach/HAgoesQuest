using System;
using HAgoesQuest.Core;

public static class CoreTests
{
    static int assertions;
    static void DetentTests()
    {
        var tick=new AdjustmentClickGate();
        Check(!tick.Step("light.a",true,.5f,0),"No click on grip start");
        Check(!tick.Step("light.a",true,.51f,.1),"Small tracking jitter is silent");
        Check(tick.Step("light.a",true,.55f,.15),"Turning across a value step clicks");
        Check(!tick.Step("light.a",true,.60f,.16),"Fast samples cannot stack click bursts");
        Check(tick.Step("light.a",true,.60f,.22),"Continued turning produces another detent");
        Check(!tick.Step("light.a",true,.60f,.30),"Holding a value is silent");
        Check(tick.Step("light.a",true,.55f,.40),"Reverse rotation also clicks");
        Check(!tick.Step("light.b",true,.9f,.45),"Changing target resets audio baseline");
        Check(!tick.Step("light.b",false,.4f,.5),"Closing adjustment cancels sound");
        Check(!tick.Step("light.b",true,1,.55),"Resuming has no spurious click");
        Check(!tick.Step("light.b",true,1,.65),"Turning against a clamped endpoint is silent");
        Check(!tick.Step("light.b",true,.5f,1),"Tracking sample gap resets audio");
        Check(!tick.Step("light.b",true,float.NaN,1.1),"Invalid value never produces sound");
    }
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
            GazeTests();
            DetentTests();
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
    static GazeGestureGate ReadyGrip(out double t)
    {
        var g=new GazeGestureGate();t=0;
        for(int i=0;i<6;i++){g.Step("light.a",true,false,true,true,true,.5f,0,t);t+=.05;}
        return g;
    }
    static void GazeTests()
    {
        double t;var g=ReadyGrip(out t);Check(g.Ready,"Open hand on looked-at target becomes ready");
        g.Step("light.a",true,true,false,true,true,.5f,0,t+=.05);Check(g.Closed&&!g.Cast,"Fist arms without switching");
        g.Step("light.a",true,true,false,true,true,.5f,0,t+=.15);
        g.Step("light.a",true,false,true,true,true,.5f,0,t+=.05);Check(!g.Cast,"Opening is debounced");
        g.Step("light.a",true,false,true,true,true,.5f,0,t+=.10);Check(g.Cast&&!g.Commit,"Opening armed stationary fist casts once");
        g.Step("light.a",true,false,true,true,true,.5f,0,t+=.05);Check(!g.Cast,"Held open hand cannot repeat fireball");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,true,true,.5f,0,t+=.05);
        g.Step("light.a",true,true,false,true,true,.5f,42,t+=.15);Check(g.Adjusting&&Math.Abs(g.Value-.75f)<.001,"Twist previews brightness after dead zone");
        g.Step("light.a",true,false,true,true,true,.5f,42,t+=.05);g.Step("light.a",true,false,true,true,true,.5f,42,t+=.1);
        Check(g.Commit&&!g.Cast,"Twist release commits instead of throwing");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,true,true,.5f,0,t+=.05);
        g.Step("light.b",true,false,true,true,true,.5f,0,t+=.15);Check(!g.Cast&&!g.Commit&&!g.Closed,"Looking at another entity cancels grip");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,true,true,.5f,0,t+=.05);
        g.Step("light.a",false,false,true,true,true,.5f,0,t+=.1);Check(!g.Cast&&!g.Commit&&!g.Closed,"Lost tracking never counts as opening fist");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,true,true,.5f,0,t+=.05);
        g.Step("light.a",true,true,false,true,true,.5f,100,t+=.1);Check(!g.Closed,"Orientation discontinuity cancels adjustment");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,true,true,.5f,179,t+=.05);
        g.Step("light.a",true,true,false,true,true,.5f,-179,t+=.15);Check(g.Closed&&!g.Adjusting,"Angle wrap is two degrees, not a full rotation");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,false,true,.1f,0,t+=.05);
        g.Step("light.a",true,true,false,false,true,.1f,-60,t+=.15);Check(g.Value==0,"Volume cannot go below zero");
        g.Step("light.a",true,false,true,false,true,.1f,-60,t+=.05);g.Step("light.a",true,false,true,false,true,.1f,-60,t+=.1);Check(g.Commit&&!g.Cast,"Normal mode supports level adjustment without magic");
        g=ReadyGrip(out t);g.Step("light.a",true,true,false,true,true,.5f,0,t+=.05);
        g.Step("light.a",true,false,true,true,true,.5f,0,t+=1);Check(!g.Cast&&!g.Closed,"Long sample gap cancels gesture");
        g=new GazeGestureGate();for(int i=0;i<8;i++)g.Step("light.a",true,true,false,true,true,.5f,0,i*.05);Check(!g.Closed&&!g.Cast,"Startup with held fist cannot arm");
        int casts=0;t=0;g=new GazeGestureGate();
        for(int cycle=0;cycle<3;cycle++)
        {
            for(int i=0;i<12;i++)g.Step("light.a",true,false,true,true,false,0,0,t+=.05);
            for(int i=0;i<4;i++)g.Step("light.a",true,true,false,true,false,0,0,t+=.05);
            for(int i=0;i<4;i++){g.Step("light.a",true,false,true,true,false,0,0,t+=.05);if(g.Cast)casts++;}
        }
        Check(casts==3,"Three close-open gestures produce exactly three casts");
        var p=new GazePinchGate();p.Step("a",false,true,true,0);
        Check(!p.Step("a",true,true,true,.05),"Pinch waits for intent classification");
        Check(p.Step("a",true,true,true,.18),"Deliberate gaze pinch selects");
        Check(!p.Step("a",true,true,true,.2),"Held gaze pinch cannot repeat");
        p.Reset();p.Step("a",false,true,true,0);p.Step("a",true,true,true,.05);
        Check(!p.Step("a",true,true,false,.1)&&!p.Step("a",true,true,true,.2),"Closing fist cancels pending pinch until release");
        p.Reset();p.Step("a",false,true,true,0);p.Step("a",true,true,true,.05);
        Check(!p.Step("b",true,true,true,.2),"Gaze target change cancels pending pinch");
        p.Reset();p.Step("a",true,false,true,0);Check(!p.Step("a",true,true,true,.1),"Tracking recovery while pinched cannot select");
    }
}
