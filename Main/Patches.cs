using HarmonyLib;
using ModComponent.Utils;
using Il2Cpp;
using UnityEngine;
using MelonLoader;
using UnityEngine.Playables;
using UnityEngine.AddressableAssets;

namespace Shotgun
{
    // Rare set of comments for me, since a lot of these patches do seemingly illogical things - always a good reason for it though!
    internal class Patches
    {
        [HarmonyPatch(typeof(EquipItemPopup), nameof(EquipItemPopup.AllowedToHideAmmoPopup))]
        [HarmonyPriority(-999999)]
        internal static class EquipItemPopup_AllowedToHideAmmoPopup_ModComponentCompatibilityFix
        {
            internal static void Postfix(ref bool __result)
            {
                if (!__result) return;
                if (GameManager.GetPlayerManagerComponent().m_ItemInHands == null) return;
                if (GameManager.GetPlayerManagerComponent().m_ItemInHands.m_GunItem == null) return;
                __result = false;
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.PressSwitchAmmo))]
        internal static class GunItem_PreventSwitchAmmoUnlessIntended
        {
            internal static bool Prefix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return true;
                if (!InputManager.HasContext(InputManager.m_CurrentContext)) return false;
                if (!Input.GetKey(KeyCode.T)) return false;
                return true;
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.PressReloadAmmo))]
        internal static class GunItem_PreventReloadAmmoUnlessIntended
        {
            internal static bool Prefix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return true;
                if (!InputManager.HasContext(InputManager.m_CurrentContext)) return false;
                if (!Input.GetKey(KeyCode.T) && !Input.GetKey(KeyCode.R)) return false;
                return true;
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Start))]
        internal static class GunItem_UpdateOnStart
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return;
                ShotgunSkill.ApplyToGunItem(__instance);
                UpdateShotgunParams(__instance, __instance.ValidAmmo[__instance.m_SelectedAmmoIndex].DisplayName);
                CustomAmmoHUD.Current.Refresh();
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Update))]
        internal static class GunItem_SwitchAmmoOnT
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return;
                if (!InputManager.HasContext(InputManager.m_CurrentContext)) return;
                if (!Input.GetKeyDown(KeyCode.T)) return;

                GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(__instance.m_CasingData.m_PrefabReference, __instance.m_SpentCasingsInClip);
                __instance.m_SpentCasingsInClip = 0;

                while (__instance.m_Clip.Count > 0)
                {
                    GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(__instance.ValidAmmo[__instance.m_SelectedAmmoIndex].m_PrefabReference);
                    __instance.m_Clip.RemoveAt(0);
                }

                __instance.PressSwitchAmmo();

                UpdateShotgunParams(__instance, __instance.ValidAmmo[__instance.m_SelectedAmmoIndex].DisplayName);
                CustomAmmoHUD.Current.Refresh();
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Update))]
        internal static class GunItem_ReloadAmmoOnR
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return;
                if (!InputManager.HasContext(InputManager.m_CurrentContext)) return;
                if (!Input.GetKeyDown(KeyCode.R)) return;
                if (__instance.m_Clip.Count == __instance.m_ClipSize) return;
                
                GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(__instance.m_CasingData.m_PrefabReference, __instance.m_SpentCasingsInClip);
                __instance.m_SpentCasingsInClip = 0;
                
                __instance.PressReloadAmmo();
                CustomAmmoHUD.Current.Refresh();
            }
        }

        [HarmonyPatch(typeof(GunItem), nameof(GunItem.RemoveNextFromClip))]
        internal static class GunItem_AddSpentShellCasingOnRemoveNextFromClip
        {
            internal static void Prefix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return;
                if (__instance.m_Clip.Count == 0) return;
                __instance.m_SpentCasingsInClip++;
            }
        }

        [HarmonyPatch(typeof(vp_FPSShooter), nameof(vp_FPSShooter.Reload), new Type[] { typeof(int) })]
        internal static class vp_FPSShooter_EnsureCorrectReloadCount
        {
            internal static void Prefix(ref int ammoCount, vp_FPSShooter __instance)
            {
                if (__instance == null) return;
                if (__instance.m_Weapon == null) return;
                if (__instance.m_Weapon.m_GunItem == null) return;
                if (__instance.m_Weapon.m_GunItem.m_GunType != GunTypeCustom) return;

                ammoCount = __instance.m_Weapon.m_GunItem.m_ClipSize - __instance.m_Weapon.m_GunItem.m_Clip.Count;
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Fired))]
        internal static class GunItem_RefreshCustomHUDOnFired
        {
            internal static void Postfix(GunItem __instance)
            {
                CustomAmmoHUD.Current.Refresh();

                if (__instance.m_GunType != GunTypeCustom) return;
                ShotgunSkill.MaybeIncreaseSkillOnFire();
            }
        }


        [HarmonyPatch(typeof(LocalizedDamage), nameof(LocalizedDamage.GetBleedOutMinutes), new Type[] { typeof(WeaponSource) })]
        internal static class LocalizedDamage_PreventUnfairBleeding
        {
            internal static void Postfix(LocalizedDamage __instance, WeaponSource weapon, ref float __result)
            {
                CustomAmmoHUD futureFirearmManager = CustomAmmoHUD.Current;
                if (futureFirearmManager.LoadedAmmo == null) return;
                switch (CustomAmmoHUD.Current.LoadedAmmo.name)
                {
                    case "GEAR_ShotgunAmmoBoxBuckshot":
                    case "GEAR_ShotgunAmmoBoxBirdshot":
                        __result = 0f; //Bleeding is currently too hard to figure out, really damage and bleeding should be much more nuanced for shotgun but for now I just want to allow it on slugs.
                        return;
                }
            }
        }


        [HarmonyPatch(typeof(LocalizedDamage), nameof(LocalizedDamage.RollChanceToKill), new Type[] { typeof(WeaponSource) })]
        internal static class LocalizedDamage_PreventUnfairCriticals
        {
            internal static void Postfix(LocalizedDamage __instance, WeaponSource weapon, ref bool __result)
            {
                CustomAmmoHUD futureFirearmManager = CustomAmmoHUD.Current;
                if (futureFirearmManager.LoadedAmmo == null) return;
                switch (CustomAmmoHUD.Current.LoadedAmmo.name)
                {
                    case "GEAR_ShotgunAmmoBoxBirdshot":
                    case "GEAR_ShotgunAmmoBoxBuckshot":
                        __result = false; //We don't allow crits on multi-projectile shot
                        return;
                }
            }
        }       


        [HarmonyPatch(typeof(BaseAi), nameof(BaseAi.ApplyDamage), new Type[] { typeof(float), typeof(float), typeof(DamageSource), typeof(string) })]
        internal static class BaseAi_LogDamage
        {
            internal static void Postfix(float damage, float bleedOutMintues, DamageSource damageSource, string collider)
            {
                MelonLogger.Msg($"Damage Taken: {damage} | Bleedout Minutes inflicted: {bleedOutMintues} | DamageSource: {damageSource}");
            }
        }


        [HarmonyPatch(typeof(BodyDamage), nameof(BodyDamage.GetDamageScale), new Type[] { typeof(BodyPart), typeof(WeaponSource) })]
        internal static class BodyDamage_GetDamageScalePatch
        {
            internal static void Postfix(BodyPart bodyPart, WeaponSource w, ref float __result)
            {
                MelonLogger.Msg($"Damage Scale: {__result} | BodyPart: {bodyPart} | WeaponSource: {w}");
            }
        }


        [HarmonyPatch(typeof(vp_Bullet), nameof(vp_Bullet.Start))]
        internal static class bullet_start_info
        {
            internal static void Postfix(vp_Bullet __instance)
            {
                MelonLogger.Msg($"Damage: {__instance.Damage} | DamageFalloffPerMeterBeyondEffectiveRange: {__instance.DamageFalloffPerMeterBeyondEffectiveRange} | MinDamageFalloffBeyondEffectiveRange: {__instance.MinimumDamageFalloffBeyondEffectiveRange} | Accuracy: {__instance.Accuracy}");
            }
        }


        internal static bool MaybeLatchShotgunAnimator(PlayerAnimation instance, string callerName = "")
        {
            Animator vanillaAnimator = Main.Instance.Configuration.Refs.VanillaAnimator;
            if (vanillaAnimator == null)
            {
                MelonLogger.Msg($"[{callerName}] ABORT LATCH: Null vanilla animator");
                return false;
            }

            Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
            if (moddedAnimator == null)
            {
                MelonLogger.Msg($"[{callerName}] ABORT LATCH: Null modded animator");
                vanillaAnimator.enabled = true;
                return false;
            }

            if (instance.m_EquippedFirstPersonWeaponShoulder == null)
            {
                MelonLogger.Msg($"[{callerName}] ABORT LATCH: Null FPS Shoulder");
                moddedAnimator.enabled = false;
                vanillaAnimator.enabled = true;
                return false;
            }
            Animator shotgunAnimator = instance.m_EquippedFirstPersonWeaponShoulder.m_Animator;

            if (shotgunAnimator == null)
            {
                MelonLogger.Msg($"[{callerName}] ABORT LATCH: Null Shotgun Animator");
                moddedAnimator.enabled = false;
                vanillaAnimator.enabled = true;
                return false;
            }

            bool isShotgun = instance.m_EquippedFirstPersonWeaponShoulder.name.Contains("Shotgun");

            moddedAnimator.enabled = isShotgun;
            vanillaAnimator.enabled = !isShotgun;
            shotgunAnimator.enabled = !isShotgun;

            MelonLogger.Msg($"[{callerName}] LATCH: {isShotgun}");

            return isShotgun;
        }



        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Equip))]
        internal static class PlayerAnimation_ShotgunEquip
        {
            internal static void Postfix(PlayerAnimation __instance)
            {
                if (!MaybeLatchShotgunAnimator(__instance, "Equip")) return;

                // Rename the clone so animation clip paths match (clips target "FPH_Shotgun", not "FPH_Shotgun (Clone)")
                if (__instance.m_EquippedFirstPersonWeaponShoulder != null)
                    __instance.m_EquippedFirstPersonWeaponShoulder.gameObject.name = ShotgunFPHPrefabName;

                // With the vanilla animator disabled, animation events that advance the state machine
                // will never fire. Manually simulate them so m_State reaches Idle (5) and subsequent
                // Trigger_Generic_Aim / Fire calls aren't blocked by MaybeSetState.
                MelonLogger.Msg("[Equip] Manually firing ShowItem + EquipComplete to advance state machine");
                __instance.OnAnimationEvent_Generic_Equip_ShowItem();
                __instance.OnAnimationEvent_Generic_Equip_Complete();
                MelonLogger.Msg($"[Equip] State after manual advance: {__instance.m_State}");
            }
        }


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Aim))]
        internal static class PlayerAnimation_ShotgunAim
        {
            internal static void Postfix(PlayerAnimation __instance)
            {
                if (!MaybeLatchShotgunAnimator(__instance, "Aim")) return;
                Main.Instance.Configuration.Refs.ModdedAnimator.SetTrigger("Trigger_Generic_Aim");
            }
        }


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Aim_Cancel))]
        internal static class PlayerAnimation_ShotgunAimCancel
        {
            internal static void Postfix(PlayerAnimation __instance)
            {
                if (!MaybeLatchShotgunAnimator(__instance, "AimCancel")) return;
                Main.Instance.Configuration.Refs.ModdedAnimator.SetTrigger("Trigger_Generic_Aim_Cancel");
            }
        }


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Fire))]
        internal static class PlayerAnimation_ShotgunFire
        {
            internal static void Postfix(PlayerAnimation __instance)
            {
                if (!MaybeLatchShotgunAnimator(__instance, "Fire")) return;
                Main.Instance.Configuration.Refs.ModdedAnimator.SetTrigger("Trigger_Generic_Fire");
            }
        }


        [HarmonyPatch(typeof(vp_FPSWeapon), nameof(vp_FPSWeapon.PlayFireAnimation))]
        internal static class vp_FPSWeapon_PlayFireAnimation
        {
            internal static bool Prefix(vp_FPSWeapon __instance)
            {
                if (__instance.m_GunItem == null) return true;
                if (__instance.m_GunItem.m_GunType != GunTypeCustom) return true;
                return true;
            }
        }


        // this should turn into an expansion of the ModAmmoComponent in ModComponent - these should all be parameters exposed and then applied just like this when that ammo is chosen.
        internal static void UpdateShotgunParams(GunItem gun, string ammoName)
        {
            if (gun == null) return;
            if (!gun.name.Contains("Shotgun")) return;
            vp_Bullet currentBullet = Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectilePrefab.GetComponent<vp_Bullet>();
            if (ammoName.Contains("Slug"))
            {
                gun.m_DamageHP = 120;
                gun.m_AccuracyRange = 50;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileCount = 1;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileSpread = 0f;
                currentBullet.MinimumDamageFalloffBeyondEffectiveRange = 25f;
                currentBullet.DamageFalloffPerMeterBeyondEffectiveRange = 5f;
            }
            else if (ammoName.Contains("Buckshot"))
            {
                gun.m_DamageHP = 15;
                gun.m_AccuracyRange = 40f;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileCount = 9;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileSpread = 1f;
                currentBullet.MinimumDamageFalloffBeyondEffectiveRange = 5f;
                currentBullet.DamageFalloffPerMeterBeyondEffectiveRange = 0.5f;
            }
            else if (ammoName.Contains("Birdshot"))
            {
                gun.m_DamageHP = 3f;
                gun.m_AccuracyRange = 30f;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileCount = 30;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileSpread = 2f;
                currentBullet.MinimumDamageFalloffBeyondEffectiveRange = 1f;
                currentBullet.DamageFalloffPerMeterBeyondEffectiveRange = 0.2f;
            }
        }
    }
}
