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
            ConfigurationReferences refs = Main.Instance.Configuration.Refs;
            if (refs.ClipManager == null || refs.AudioShot == null) return;
            AudioMgr.Clip clip = refs.ClipManager.GetClip(sound);
            if (clip == null) return;
            refs.AudioShot._audioSource.PlayOneShot(clip.audioClip);
        }
    }
}
