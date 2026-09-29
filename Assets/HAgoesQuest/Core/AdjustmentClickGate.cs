using System;

namespace HAgoesQuest.Core
{
    public sealed class AdjustmentClickGate
    {
        string target;
        float lastValue;
        double next,lastTime=-1;
        public void Reset(){target=null;lastTime=-1;next=0;}
        public bool Step(string id,bool active,float value,double time)
        {
            if(!active||string.IsNullOrEmpty(id)||float.IsNaN(value)||float.IsInfinity(value)||double.IsNaN(time)||double.IsInfinity(time)){Reset();return false;}
            if(target!=id||lastTime<0||time<lastTime||time-lastTime>.25)
            {target=id;lastValue=value;lastTime=time;next=time;return false;}
            lastTime=time;
            if(Math.Abs(value-lastValue)<.035f||time<next)return false;
            lastValue=value;next=time+.055;return true;
        }
    }
}
