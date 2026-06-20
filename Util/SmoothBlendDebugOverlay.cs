using UnityEngine;

namespace Shotgun.Utility
{
    // AI DISCLOSURE: 100% AI generated, used just to help visualize some testing data for Waltz and I. No part of The Long Dark was used in generating this, just my own written code and basic Unity RAG.
    internal class SmoothBlendDebugOverlay
    {
        internal bool Enabled { get; set; } = true;

        private const int HistorySize = 512;
        private const int PreviewSamples = 256;

        private const double MinorTickHours = 1.0 / 60.0; // one in-game minute
        private const double MajorTickHours = 1.0;        // one in-game hour

        private readonly double[] mTimes = new double[HistorySize];
        private readonly float[] mValues = new float[HistorySize];
        private readonly float[] mPreview = new float[PreviewSamples];
        private int mCount;
        private int mHead;          // next write index (ring buffer)

        private bool mHasStart;
        private double mStartTime;  // game-time of the first recorded sample
        private double mLastSample; // game-time of the most recent recorded sample

        private static Texture2D? sPixel;

        // Fixed visible span (game-hours) from settings, and half of it (now sits at the midpoint).
        private static float WindowHours => Mathf.Max(0.01f, Settings.Instance.IdleVariationGraphWindowHours);

        // "On" state inferred purely from incoming data: the modded animator is only enabled while
        // the shotgun is equipped/latched, so reading its flag tells us whether Idle_Variation is
        // actually being driven -- no need to hook the equip/unequip patches.
        private static bool IsActive()
        {
            Animator animator = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
            return animator != null && animator.enabled;
        }

        internal void Tick()
        {
            if (!Enabled || Settings.Instance == null) return;

            // Shotgun not driving the animator -> clear history and stop recording until re-equipped.
            if (!IsActive())
            {
                mCount = 0;
                mHead = 0;
                mHasStart = false;
                return;
            }

            double now = SampleGameTime();
            double interval = WindowHours / HistorySize;
            if (interval <= 0.0) interval = 0.0001;

            // Reset on first run or if game-time jumps backwards (save load / new game).
            if (!mHasStart || now + 1e-9 < mLastSample)
            {
                mCount = 0;
                mHead = 0;
                mStartTime = now;
                mLastSample = now - interval;
                mHasStart = true;
            }

            // Sample on game-time, not frames -> paused game == frozen graph.
            if (now - mLastSample >= interval)
            {
                mTimes[mHead] = now;
                mValues[mHead] = Patches.IdleVariationApplied;
                mHead = (mHead + 1) % HistorySize;
                if (mCount < HistorySize) mCount++;
                mLastSample = now;
            }
        }

        internal void Draw()
        {
            if (!Enabled) return;
            Settings settings = Settings.Instance;
            if (settings == null) return;

            EnsurePixel();

            const float scale = 2f;
            Matrix4x4 prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            const float pad = 8f;
            const float panelW = 720f;   // ~2x longer graph
            const float graphH = 165f;   // 1.5x taller graph
            const float axisW = 30f;

            SmoothRandomBlend gen = Patches.IdleVariation;
            float min = gen.OutputMin;
            float max = gen.OutputMax;

            float panelH = 4f + graphH + 16f + 6f;
            Fill(new Rect(pad, pad, panelW, panelH), new Color(0f, 0f, 0f, 0.65f));

            float x = pad + 6f;
            float y = pad + 4f;
            float innerW = panelW - 12f;

            // combined timeline graph (green history + yellow projection); blank when not equipped
            Rect graph = new(x + axisW, y, innerW - axisW, graphH);
            DrawTimeline(graph, gen, min, max, IsActive());

            GUI.color = Color.white;
            GUI.matrix = prevMatrix;
        }

        private void DrawTimeline(Rect area, SmoothRandomBlend gen, float min, float max, bool active)
        {
            // frame + Y gridlines/labels
            Fill(area, new Color(0.08f, 0.08f, 0.08f, 1f));
            GUI.color = new Color(0.75f, 0.75f, 0.75f, 1f);
            for (int i = 0; i <= 2; i++)
            {
                float t = i * 0.5f;
                float ly = area.yMax - t * area.height;
                Fill(new Rect(area.x, ly, area.width, 1f), new Color(1f, 1f, 1f, 0.15f));
                GUI.Label(new Rect(area.x - 30f, ly - 8f, 28f, 16f), Mathf.Lerp(min, max, t).ToString("0.0"));
            }

            // Not driving the animator -> leave the graph blank (just the empty value axis).
            if (!active)
            {
                GUI.color = Color.white;
                return;
            }

            double now = SampleGameTime();
            float half = WindowHours * 0.5f;
            double windowLeft = System.Math.Max(mStartTime, now - half);
            double windowRight = windowLeft + WindowHours;
            double span = WindowHours;

            DrawTicks(area, windowLeft, windowRight, span);

            // vertical "now" divider
            float nowX = area.x + (float)((now - windowLeft) / span) * area.width;
            Fill(new Rect(nowX, area.y, 1f, area.height), new Color(1f, 1f, 1f, 0.5f));

            // GREEN history: recorded samples falling inside [windowLeft, now]
            Color green = new(0.4f, 1f, 0.5f, 1f);
            for (int i = 0; i < mCount; i++)
            {
                int idx = (mHead - mCount + i + HistorySize) % HistorySize;
                double t = mTimes[idx];
                if (t < windowLeft || t > now) continue;
                PlotPoint(area, (float)((t - windowLeft) / span), Mathf.Clamp01(Mathf.InverseLerp(min, max, mValues[idx])), green);
            }

            // YELLOW projection: generator's actual future from now -> windowRight
            double previewHours = windowRight - now;
            SmoothRandomBlend proj = gen.Clone();
            float previewScaled = (float)(previewHours * Mathf.Max(0.0001f, gen.TimeScale));
            float step = previewScaled / (PreviewSamples - 1);
            mPreview[0] = proj.Advance(0f);
            for (int j = 1; j < PreviewSamples; j++) mPreview[j] = proj.Advance(step);

            Color yellow = new(1f, 0.85f, 0.3f, 1f);
            for (int j = 0; j < PreviewSamples; j++)
            {
                float frac = j / (float)(PreviewSamples - 1);
                PlotPoint(area, (float)((now + frac * previewHours - windowLeft) / span),
                    Mathf.Clamp01(Mathf.InverseLerp(min, max, mPreview[j])), yellow);
            }

            GUI.color = Color.white;
        }

        // Moving tick marks anchored to absolute game-time: minor every minute, major (labelled) every hour.
        private void DrawTicks(Rect area, double windowLeft, double windowRight, double span)
        {
            Color tickColor = new(1f, 1f, 1f, 0.25f);

            // Minor (per-minute) ticks -- only when they aren't packed too tightly to read.
            float minorPx = (float)(area.width * (MinorTickHours / span));
            if (minorPx >= 2.5f)
            {
                long first = (long)System.Math.Ceiling(windowLeft / MinorTickHours);
                long last = (long)System.Math.Floor(windowRight / MinorTickHours);
                for (long k = first; k <= last; k++)
                {
                    if (k % 60 == 0) continue; // hour marks drawn as major below
                    float px = area.x + (float)((k * MinorTickHours - windowLeft) / span) * area.width;
                    Fill(new Rect(px, area.yMax - 4f, 1f, 4f), tickColor);
                }
            }

            // Major (per-hour) ticks + labels.
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            long firstH = (long)System.Math.Ceiling(windowLeft / MajorTickHours);
            long lastH = (long)System.Math.Floor(windowRight / MajorTickHours);
            for (long h = firstH; h <= lastH; h++)
            {
                float px = area.x + (float)((h * MajorTickHours - windowLeft) / span) * area.width;
                Fill(new Rect(px, area.yMax - 8f, 1f, 8f), new Color(1f, 1f, 1f, 0.45f));
                GUI.Label(new Rect(px - 18f, area.yMax + 1f, 40f, 16f), $"{h}h");
            }
            GUI.color = Color.white;
        }

        private static void PlotPoint(Rect area, float tFrac, float vFrac, Color color)
        {
            if (tFrac < 0f || tFrac > 1f) return;
            float px = area.x + tFrac * area.width;
            float py = area.yMax - vFrac * area.height;
            Fill(new Rect(px, py - 1f, 2f, 2f), color);
        }

        private static void EnsurePixel()
        {
            if (sPixel != null) return;
            sPixel = new Texture2D(1, 1);
            sPixel.SetPixel(0, 0, Color.white);
            sPixel.Apply();
        }

        private static void Fill(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, sPixel);
            GUI.color = prev;
        }
    }
}
