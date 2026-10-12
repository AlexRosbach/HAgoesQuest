using System;

namespace HAgoesQuest.Core
{
    /// <summary>Recognizes an armed fist, rotation and release for one caller-supplied target.</summary>
    /// <remarks>The caller holds the target and roll reference while closed. Tracking loss never commits.</remarks>
    public sealed class GazeGestureGate
    {
        string target;
        double last = -1, openSince = -1, closedSince = -1, releaseSince = -1, cooldown;
        double previousRoll, rotation;
        bool adjusting, closed, ready;
        float initial;
        public bool Ready { get { return ready; } }
        public bool Closed { get { return closed; } }
        public bool Adjusting { get { return adjusting; } }
        public float Value { get; private set; }
        /// <summary>One-step event indicating that the adjusted value should be applied.</summary>
        public bool Commit { get; private set; }
        /// <summary>One-step event for an opened, unadjusted fist in magic mode.</summary>
        public bool Cast { get; private set; }
        public void Reset()
        {
            target=null;last=-1;openSince=closedSince=releaseSince=-1;
            adjusting=closed=ready=Commit=Cast=false;rotation=0;
        }
        /// <summary>Consumes a pose sample; invalid samples reset the gesture without an action.</summary>
        /// <param name="id">Stable entity ID; changing it resets readiness and any closed grip.</param>
        /// <param name="tracked">Whether tracking and the caller's interaction policy allow the sample.</param>
        /// <param name="fist">Whether the tracked fingers form a fist.</param>
        /// <param name="open">Whether a deliberate open palm is recognized.</param>
        /// <param name="magic">Whether an unadjusted release may cast.</param>
        /// <param name="adjustable">Whether rotation may change a level.</param>
        /// <param name="level">Initial level in the normalized range 0 to 1.</param>
        /// <param name="roll">Hand roll in degrees relative to the captured orientation.</param>
        /// <param name="time">Monotonic unscaled time in seconds.</param>
        public void Step(string id,bool tracked,bool fist,bool open,bool magic,bool adjustable,float level,double roll,double time)
        {
            Commit=Cast=false;
            if(!tracked||string.IsNullOrEmpty(id)||double.IsNaN(roll)||double.IsInfinity(roll)||float.IsNaN(level)||float.IsInfinity(level)||
                (last>=0&&(time<last||time-last>.25))) { Reset();return; }
            if(target!=id){Reset();target=id;}
            last=time;
            if(time<cooldown)return;
            if(!closed)
            {
                if(open){if(openSince<0)openSince=time;ready=time-openSince>=.18;}
                else if(!ready)openSince=-1;
                if(ready&&fist)
                {
                    closed=true;closedSince=time;initial=Value=Math.Max(0,Math.Min(1,level));
                    previousRoll=roll;rotation=0;adjusting=false;
                }
                return;
            }
            if(fist)
            {
                releaseSince=-1;
                double delta=(roll-previousRoll+540)%360-180;
                previousRoll=roll;
                if(Math.Abs(delta)>65){Reset();return;}
                rotation=Math.Max(-150,Math.Min(150,rotation+delta));
                if(adjustable&&Math.Abs(rotation)>12&&time-closedSince>=.12)adjusting=true;
                if(adjusting)Value=(float)Math.Max(0,Math.Min(1,initial+(rotation-Math.Sign(rotation)*12)/120));
                return;
            }
            if(!open){releaseSince=-1;return;}
            if(releaseSince<0)releaseSince=time;
            if(time-releaseSince<.08)return;
            Commit=adjusting;Cast=!adjusting&&magic&&time-closedSince>=.12;
            closed=ready=adjusting=false;openSince=closedSince=releaseSince=-1;cooldown=time+.3;
        }
    }
    /// <summary>Emits one pinch after a short intent window, distinguishing it from a closing fist.</summary>
    public sealed class GazePinchGate
    {
        bool released,pending,spent;
        string target;
        double started,last=-1;
        public void Reset(){released=pending=spent=false;target=null;last=-1;}
        public bool Step(string id,bool down,bool allowed,bool intent,double time)
        {
            if(!allowed||(last>=0&&(time<last||time-last>.25))){Reset();return false;}
            last=time;
            if(!down){released=true;pending=spent=false;target=null;return false;}
            if(!released||spent)return false;
            if(!pending){pending=true;target=id;started=time;}
            if(!intent||string.IsNullOrEmpty(id)||id!=target){spent=true;return false;}
            // Give the other fingers time to distinguish a pinch from a closing fist.
            if(time-started<.12)return false;
            spent=true;return true;
        }
    }
}
