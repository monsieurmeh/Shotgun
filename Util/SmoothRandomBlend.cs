using System;
using Il2Cpp;

namespace Shotgun.Utility
{
    // AI DISCLOSURE: AI Generated logic, tested and hand-distilled into something I'm comfortable with. No part of The Long Dark was used in generating this, just my own written code and basic Unity RAG.
    // Utilizes xorshift64 to generate a random curve at runtime cheaply.
    internal class SmoothRandomBlend
    {
        internal float OutputMin { get; set; } = 0f;
        internal float OutputMax { get; set; } = 1f;
        internal float MinSegmentDuration { get; set; } = 0.5f;
        internal float MaxSegmentDuration { get; set; } = 2.0f;
        internal float MinTargetDistance { get; set; } = 0.2f;
        internal float MaxTargetDistance { get; set; } = 0.8f;
        
        internal float TimeScale { get; set; } = 1.0f;
 
        private ulong mRngState;
        private bool mInitialized;
        private bool mHasLastTime;
        private double mLastGameTime;
        private float mSegmentStart;
        private float mSegmentTarget;
        private float mSegmentDuration;
        private float mSegmentElapsed;

        internal float Value { get; private set; }

        internal SmoothRandomBlend(int? seed = null)
        {
            mRngState = (ulong)(seed ?? Environment.TickCount);
            if (mRngState == 0) mRngState = 0x9E3779B97F4A7C15UL;
        }

        internal SmoothRandomBlend Clone()
        {
            SmoothRandomBlend c = new()
            {
                OutputMin = OutputMin,
                OutputMax = OutputMax,
                MinSegmentDuration = MinSegmentDuration,
                MaxSegmentDuration = MaxSegmentDuration,
                MinTargetDistance = MinTargetDistance,
                MaxTargetDistance = MaxTargetDistance,
                TimeScale = TimeScale,
            };
            c.mRngState = mRngState;
            c.mInitialized = mInitialized;
            c.mHasLastTime = mHasLastTime;
            c.mLastGameTime = mLastGameTime;
            c.mSegmentStart = mSegmentStart;
            c.mSegmentTarget = mSegmentTarget;
            c.mSegmentDuration = mSegmentDuration;
            c.mSegmentElapsed = mSegmentElapsed;
            c.Value = Value;
            return c;
        }

        internal void Tick()
        {
            double now = SampleGameTime();
            if (!mHasLastTime)
            {
                mLastGameTime = now;
                mHasLastTime = true;
            }

            double rawDelta = now - mLastGameTime;
            mLastGameTime = now;
            if (rawDelta < 0) rawDelta = 0;

            Advance((float)rawDelta * TimeScale);
        }

        internal float Advance(float scaledDelta)
        {
            Initialize();
            if (scaledDelta < 0f) scaledDelta = 0f;

            mSegmentElapsed += scaledDelta;

            while (mSegmentElapsed >= mSegmentDuration && mSegmentDuration > 0f)
            {
                mSegmentElapsed -= mSegmentDuration;
                mSegmentStart = mSegmentTarget;
                mSegmentTarget = PickTarget(mSegmentStart);
                mSegmentDuration = RandomDuration();
            }

            float t = mSegmentDuration > 0f ? mSegmentElapsed / mSegmentDuration : 1f;
            if (t > 1f) t = 1f;

            float curved = Smootherstep(t);
            float unit = mSegmentStart + (mSegmentTarget - mSegmentStart) * curved;
            Value = Remap(unit);
            return Value;
        }

        private void Initialize()
        {
            if (mInitialized) return;
            mSegmentStart = RandomInUnit();
            mSegmentTarget = PickTarget(mSegmentStart);
            mSegmentDuration = RandomDuration();
            mSegmentElapsed = 0f;
            Value = Remap(mSegmentStart);
            mInitialized = true;
        }

        private double NextUnit()
        {
            ulong x = mRngState;
            x ^= x >> 12;
            x ^= x << 25;
            x ^= x >> 27;
            mRngState = x;
            ulong result = x * 0x2545F4914F6CDD1DUL;
            return (result >> 11) * (1.0 / 9007199254740992.0); 
        }

        private float RandomDuration()
        {
            float lo = MinSegmentDuration;
            float hi = MaxSegmentDuration;
            if (hi < lo) hi = lo;
            return lo + (float)NextUnit() * (hi - lo);
        }

        private float PickTarget(float current)
        {
            float lo = MinTargetDistance;
            float hi = MaxTargetDistance;
            if (hi < lo) hi = lo;
            float dist = lo + (float)NextUnit() * (hi - lo);

            float dir = NextUnit() < 0.5 ? -1f : 1f;
            float t = current + dir * dist;

            if (t < 0f) t = -t;
            if (t > 1f) t = 2f - t;
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;
            return t;
        }

        private float RandomInUnit() => (float)NextUnit();
        private float Remap(float unit) => OutputMin + (OutputMax - OutputMin) * unit;
        private static float Smootherstep(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
    }
}
