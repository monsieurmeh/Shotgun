using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Shotgun
{
    internal class AnimationEventProxy : MonoBehaviour
    {
        internal void OnAimComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) return;
            playerAnimation.OnAnimationEvent_Generic_Aim_Complete();
        }

        internal void OnAimCancelComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) return;
            playerAnimation.OnAnimationEvent_Generic_Aim_Cancel_Complete();
        }

        internal void OnFireFX()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) return;
            playerAnimation.OnAnimationEvent_Generic_Fire_FX();
        }

        internal void OnFireComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) return;
            playerAnimation.OnAnimationEvent_Generic_Fire_Complete();
        }

        internal void OnPlaySound(string sound)
        {
            // TODO: implement sound playback
            MelonLogger.Msg($"AnimationEventProxy.OnPlaySound: {sound}");
        }
    }
}
