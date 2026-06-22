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
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Aim_Complete();
        }

        internal void OnAimCancelComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Aim_Cancel_Complete();
        }

        internal void OnFireFX()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Fire_FX();
        }

        internal void OnFireComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Fire_Complete();
        }

        internal void OnEquipShowItem()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Equip_ShowItem();
        }

        internal void OnEquipComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Equip_Complete();
        }

        internal void OnUnequipHidden()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Hidden();
        }

        internal void OnUnequipComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_HiddenComplete();
            Animator vanillaAnimator = Main.Instance?.Configuration?.Refs?.VanillaAnimator;
            Animator moddedAnimator = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
            if (vanillaAnimator != null) vanillaAnimator.enabled = true;
            if (moddedAnimator != null) moddedAnimator.enabled = false;
        }

        internal void OnRoundLoaded()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Round_Loaded();
            GunItem gunItem = GameManager.GetPlayerManagerComponent()?.m_ItemInHands?.m_GunItem;
            Actions.SyncAmmoParams(gunItem);
        }

        internal void OnRoundsUnloaded()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            playerAnimation.OnAnimationEvent_Generic_Rounds_Unloaded();
        }

        internal void OnReloadComplete()
        {
            PlayerAnimation playerAnimation = GameManager.GetPlayerAnimationComponent();
            if (playerAnimation == null) 
                return;

            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            int controlModeBefore = playerManager != null ? (int)playerManager.m_ControlMode : -1;
            int stateBefore = (int)playerAnimation.m_State;

            playerAnimation.OnAnimationEvent_Generic_Reload_Complete();

            int controlModeAfter = playerManager != null ? (int)playerManager.m_ControlMode : -1;
            int stateAfter = (int)playerAnimation.m_State;

            GunItem gunItem = playerManager?.m_ItemInHands?.m_GunItem;
            Actions.SyncAmmoParams(gunItem);
        }

        internal void SyncAmmoVisibility(GunItem gunItem)
        {
            Actions.SyncAmmoParams(gunItem);
        }

        internal void OnSyncAmmoVisibility()
        {
            GunItem gunItem = GameManager.GetPlayerManagerComponent()?.m_ItemInHands?.m_GunItem;
            if (gunItem == null) 
                return;

            SyncAmmoVisibility(gunItem);
        }

        internal void OnInsertStarted(int count)
        {
            if (count <= 0) count = 1;
            GunItem gunItem = GameManager.GetPlayerManagerComponent()?.m_ItemInHands?.m_GunItem;
            Animator moddedAnimator = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
            if (gunItem == null || gunItem.m_Clip == null || moddedAnimator == null) 
                return;

            moddedAnimator.SetInteger("Rounds_Loaded", gunItem.m_Clip.Count + count);
            moddedAnimator.SetInteger("SpentCasingsInClip", gunItem.m_SpentCasingsInClip);
        }

        internal void OnReturnRound(int count)
        {
            if (count <= 0) count = 1;
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            GunItem gunItem = playerManager?.m_ItemInHands?.m_GunItem;
            if (gunItem == null || gunItem.m_Clip == null) 
                return;

            if (gunItem.m_PreviousAmmoIndex < 0) 
                return;

            for (int i = 0; i < count && gunItem.m_Clip.Count > 0; i++)
            {
                int condition = gunItem.m_Clip[0];
                AssetReferenceGearItem assetRef = gunItem.ValidAmmo[gunItem.m_PreviousAmmoIndex].m_PrefabReference;
                playerManager.InstantiateItemInPlayerInventory(assetRef, 1, condition, 0);
                gunItem.m_Clip.RemoveAt(0);
            }

            if (gunItem.m_Clip.Count == 0) gunItem.m_PreviousAmmoIndex = -1;
        }

        internal void OnEjectCasing(int count)
        {
            if (count <= 0) count = 1;
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            GunItem gunItem = playerManager?.m_ItemInHands?.m_GunItem;
            if (gunItem == null) 
                return;

            for (int i = 0; i < count && gunItem.m_SpentCasingsInClip > 0; i++)
            {
                if (gunItem.m_CasingData != null)
                {
                    playerManager.InstantiateItemAtPlayersFeet(gunItem.m_CasingData.m_PrefabReference, 1);
                    gunItem.PlayCasingAudio(1);
                }
                gunItem.m_SpentCasingsInClip--;
            }
            Actions.SyncAmmoParams(gunItem);
        }

        internal void OnPlaySound(string sound)
        {
            ConfigurationReferences refs = Main.Instance.Configuration.Refs;
            if (refs.ClipManager == null || refs.AudioShot == null) 
                return;

            AudioMgr.Clip clip = refs.ClipManager.GetClip(sound);
            if (clip == null)
                return;

            refs.AudioShot._audioSource.PlayOneShot(clip.audioClip);
        }


        internal void OnAmmoTypeChanged()
        {
            // do stuff on ammo type changed
        }
    }
}
