using System;

namespace HAgoesQuest.Core
{
    public struct PushVector
    {
        public double X, Y, Z;
        public PushVector(double x, double y, double z) { X = x; Y = y; Z = z; }
        public static PushVector operator -(PushVector a, PushVector b) { return new PushVector(a.X-b.X, a.Y-b.Y, a.Z-b.Z); }
        public double Dot(PushVector b) { return X*b.X + Y*b.Y + Z*b.Z; }
        public double Length { get { return Math.Sqrt(Dot(this)); } }
    }

    // Coordinates are palm minus head position: walking alone cannot cast a spell.
    public sealed class MagicPushGate
    {
        string target;
        PushVector previous, origin, direction, firedAt;
        double lastTime = -1, settledAt = -1, armedAt, nextAllowed;
        bool latched;
        public bool Armed { get; private set; }
        public void Reset() { target = null; lastTime = settledAt = -1; Armed = false; }

        public bool Step(string id, bool allowed, bool open, PushVector position, PushVector forward, double now)
        {
            if (!allowed) { Reset(); return false; }
            if (!open) { latched = false; Reset(); return false; }
            if (latched)
            {
                if ((position-firedAt).Dot(direction) < -0.10) latched = false;
                Reset(); return false;
            }
            if (string.IsNullOrEmpty(id) || now < nextAllowed) { Reset(); return false; }
            double dt = now-lastTime;
            if (target != id || lastTime < 0 || dt <= 0 || dt > 0.12)
            {
                Reset(); target = id; previous = position; lastTime = now; return false;
            }
            var delta = position-previous;
            double speed = delta.Length/dt;
            previous = position; lastTime = now;
            if (speed > 3.5) { Reset(); return false; } // tracking jump
            if (!Armed)
            {
                if (speed > 0.30) { settledAt = -1; return false; }
                if (settledAt < 0) settledAt = now;
                if (now-settledAt >= 0.15)
                {
                    Armed = true; armedAt = now; origin = position; direction = forward;
                }
                return false;
            }
            var travel = position-origin;
            double along = travel.Dot(direction);
            double lateral = Math.Sqrt(Math.Max(0, travel.Dot(travel)-along*along));
            if (now-armedAt > 2.0 || lateral > 0.12 || along < -0.06) { Reset(); return false; }
            if (along >= 0.12 && delta.Dot(direction)/dt >= 0.45)
            {
                firedAt = position; latched = true; nextAllowed = now+0.35; Reset(); return true;
            }
            return false;
        }
    }
}
