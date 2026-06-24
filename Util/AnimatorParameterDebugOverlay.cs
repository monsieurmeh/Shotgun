using System.Text;
using UnityEngine;

namespace Shotgun.Utility
{
    // AI DISCLOSURE: 100% AI generated, used just to help visualize some testing data for Waltz and I. No part of The Long Dark was used in generating this, just my own written code and basic Unity RAG.
    internal class AnimatorParameterDebugOverlay
    {
        internal bool Enabled { get; set; } = false;

        private readonly StringBuilder mBuilder = new();

        /// <summary>Resolve the animator to inspect. Override the source here if it ever moves.</summary>
        private static Animator? ResolveAnimator()
        {
            return Main.Instance?.Configuration?.Refs?.ModdedAnimator;
        }

        internal void Draw()
        {
            if (!Enabled) return;

            Animator? animator = ResolveAnimator();

            // Scale the whole panel (geometry + text) up so it's actually readable. Because the
            // matrix scales GUI space, the usable width is Screen.width / scale -- anchor to that.
            const float scale = 2f;
            Matrix4x4 prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            const float pad = 8f;
            const float panelW = 320f;
            const float lineH = 16f;
            float x = Screen.width / scale - panelW - pad;
            float y = pad;

            string body = BuildBody(animator, out int lineCount);

            float panelH = 24f + lineCount * lineH;
            Fill(new Rect(x, y, panelW, panelH), new Color(0f, 0f, 0f, 0.65f));

            GUI.color = Color.white;
            GUI.Label(new Rect(x + 6f, y + 4f, panelW - 12f, lineH), "Modded Animator Params");
            GUI.Label(new Rect(x + 6f, y + 4f + lineH, panelW - 12f, panelH - lineH), body);

            GUI.matrix = prevMatrix;
        }

        private string BuildBody(Animator? animator, out int lineCount)
        {
            mBuilder.Clear();
            lineCount = 1;

            if (animator == null)
            {
                mBuilder.Append("<animator null>");
                return mBuilder.ToString();
            }

            // Iterate the controller's declared parameter list by index (avoids array interop typing).
            int shown = 0;
            int count = animator.parameterCount;
            for (int i = 0; i < count; i++)
            {
                AnimatorControllerParameter p = animator.GetParameter(i);
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        mBuilder.Append(p.name).Append(" = ").Append(animator.GetBool(p.name)).Append('\n');
                        shown++;
                        break;
                    case AnimatorControllerParameterType.Float:
                        mBuilder.Append(p.name).Append(" = ").Append(animator.GetFloat(p.name).ToString("0.000")).Append('\n');
                        shown++;
                        break;
                    case AnimatorControllerParameterType.Int:
                        mBuilder.Append(p.name).Append(" = ").Append(animator.GetInteger(p.name)).Append('\n');
                        shown++;
                        break;
                    // AnimatorControllerParameterType.Trigger -> intentionally skipped
                    default:
                        break;
                }
            }

            if (shown == 0) mBuilder.Append("<no bool/float/int params>");
            lineCount = shown == 0 ? 1 : shown;
            return mBuilder.ToString();
        }

        private static Texture2D? sPixel;

        private static void Fill(Rect r, Color c)
        {
            if (sPixel == null)
            {
                sPixel = new Texture2D(1, 1);
                sPixel.SetPixel(0, 0, Color.white);
                sPixel.Apply();
            }
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, sPixel);
            GUI.color = prev;
        }
    }
}
