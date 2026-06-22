using HarmonyLib;
using ModComponent.Utils;
using Il2Cpp;
using UnityEngine;
using MelonLoader;
using UnityEngine.Playables;
using UnityEngine.AddressableAssets;
using Shotgun.Utility;

namespace Shotgun
{
    // Note for self: A lot of vanilla firearm functionality hinges on checks for GunType == 1 through 3, none of which we proc with our "custom" gun type.
    // I end up having to recreate a fair bit of vanilla firearm logic here in order to continue streamlined use of vanilla methods as triggers and so forth
    // so if it looks like we're reinventing half the wheel... we are. Wheeeeee!

    internal class Patches
    {
        [HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.LoadAllFPSWeapons))]
        internal static class LateInitializationPatch
        {
            internal static void Postfix() => Main.Instance?.LateInitialize();
        }

        [HarmonyPatch(typeof(EquipItemPopup), nameof(EquipItemPopup.AllowedToHideAmmoPopup))]
        [HarmonyPriority(-999999)]
        internal static class EquipItemPopup_AllowedToHideAmmoPopup_ModComponentCompatibilityFix
        {
            internal static void Postfix(ref bool __result)
            {
                if (!__result) return;
                PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
                if (playerManager == null) return;
                if (playerManager.m_ItemInHands == null) return;
                if (playerManager.m_ItemInHands.m_GunItem == null) return;
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
                if (!Input.GetKey(Settings.Instance.AmmoSwapKeyCode)) return false;
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
                if (!InputManager.GetReloadPressed(InputManager.m_CurrentContext) && !Input.GetKey(Settings.Instance.AmmoSwapKeyCode)) return false;
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
                if (!Input.GetKeyDown(Settings.Instance.AmmoSwapKeyCode)) return;
                if (!__instance.IsEquipped()) return;
                if (__instance.GetNextValidAmmoIndex() == __instance.m_SelectedAmmoIndex) return; //no other ammo available

                __instance.m_PreviousAmmoIndex = __instance.m_SelectedAmmoIndex; //indicate is ammo swap during reload
                __instance.PressReloadAmmo(); 
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Update))]
        internal static class GunItem_ReloadAmmoOnR
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return;
                if (!InputManager.HasContext(InputManager.m_CurrentContext)) return;
                if (!InputManager.GetReloadPressed(InputManager.m_CurrentContext)) return;
                if (__instance.m_Clip.Count == __instance.m_ClipSize && __instance.m_SpentCasingsInClip == 0) return;


                __instance.m_PreviousAmmoIndex = -1; //indicate is NOT ammo swap during reload
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

                GunItem gunItem = __instance.m_Weapon.m_GunItem;

                int slotsToFill;
                if (gunItem.m_PreviousAmmoIndex >= 0)
                {
                    slotsToFill = gunItem.m_ClipSize;
                }
                else
                {
                    slotsToFill = gunItem.m_ClipSize - gunItem.m_Clip.Count;
                }

                int ammoAvailable = 0;
                GearItem weaponInHands = GameManager.GetPlayerManagerComponent()?.m_ItemInHands;
                Inventory inventory = GameManager.GetInventoryComponent();
                if (weaponInHands != null && inventory != null)
                    ammoAvailable = inventory.GetAmmoAvailableForWeapon(weaponInHands);

                ammoCount = Math.Min(slotsToFill, ammoAvailable);
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Fired))]
        internal static class GunItem_RefreshCustomHUDOnFired
        {
            internal static void Postfix(GunItem __instance)
            {
                CustomAmmoHUD.Current.Refresh();

                if (__instance.m_GunType != GunTypeCustom) return;
                Actions.SyncAmmoParams(__instance);
            }
        }


        [HarmonyPatch(typeof(vp_FPSShooter), nameof(vp_FPSShooter.OnBulletLoaded))]
        internal static class vp_FPSShooter_RefreshHUDOnBulletLoaded
        {
            internal static void Postfix()
            {
                CustomAmmoHUD.Current.Refresh();
            }
        }


        [HarmonyPatch(typeof(LocalizedDamage), nameof(LocalizedDamage.GetBleedOutMinutes), new Type[] { typeof(WeaponSource) })]
        internal static class LocalizedDamage_PreventUnfairBleeding
        {
            internal static void Postfix(LocalizedDamage __instance, WeaponSource weapon, ref float __result)
            {
                CustomAmmoHUD futureFirearmManager = CustomAmmoHUD.Current;
                if (futureFirearmManager.LoadedAmmo == null) return;
                ShotgunSkill.MaybeIncreaseSkill();
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
                ShotgunSkill.MaybeIncreaseSkill();
                switch (CustomAmmoHUD.Current.LoadedAmmo.name)
                {
                    case "GEAR_ShotgunAmmoBoxSlug":
                        float shotgunTierCritMultiplier = ShotgunSkill.GetCurrentTier() * 0.25f; //We take opposite approach of rifle: no crits until you start to get high level, at which point you get level 1 rifle boosts
                        float adjustedChance = __instance.GetChanceKill(WeaponSource.Rifle) * shotgunTierCritMultiplier * 1.25f; //add a flat 25% boost to rifle rate
                        MelonLogger.Msg($"Crit Chance: {adjustedChance}");
                        __result = Utils.RollChance(adjustedChance); 
                        return;
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


        [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.Update))]
        internal static class PlayerMovement_TransmitLocomotion
        {
            internal static void Postfix(PlayerMovement __instance)
            {
                Actions.TransmitLocomotion(__instance);
            }
        }


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Equip))]
        internal static class PlayerAnimation_ShotgunEquip
        {
            internal static void Postfix(PlayerAnimation __instance)
            {
                if (!MaybeLatchShotgunAnimator(__instance, "Equip")) return;

                if (__instance.m_EquippedFirstPersonWeaponShoulder != null)
                    __instance.m_EquippedFirstPersonWeaponShoulder.gameObject.name = ShotgunFPHPrefabName;
                Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
                moddedAnimator.Rebind();

                GunItem equippedGun = GameManager.GetPlayerManagerComponent()?.m_ItemInHands?.m_GunItem;
                if (equippedGun != null) 
                    Actions.SyncAmmoParams(equippedGun);

                moddedAnimator.SetTrigger("Trigger_Generic_Equip");
            }
        }


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Unequip))]
        internal static class PlayerAnimation_ShotgunUnequip
        {
            internal static void Prefix(PlayerAnimation __instance)
            {
                if (__instance.m_EquippedFirstPersonWeaponShoulder == null) return;
                if (!__instance.m_EquippedFirstPersonWeaponShoulder.name.Contains("Shotgun")) return;

                Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
                if (moddedAnimator == null || !moddedAnimator.enabled)
                {
                    Animator vanillaAnimator = Main.Instance.Configuration.Refs.VanillaAnimator;
                    if (vanillaAnimator != null) vanillaAnimator.enabled = true;
                    return;
                }

                moddedAnimator.SetTrigger("Trigger_Generic_Unequip");
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


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.Trigger_Generic_Reload))]
        internal static class PlayerAnimation_ShotgunReload
        {
            internal static void Postfix(PlayerAnimation __instance, int bulletsToReload, int roundsLoaded, bool willJam)
            {
                if (!MaybeLatchShotgunAnimator(__instance, "Reload")) return;

                GearItem weaponInHands = GameManager.GetPlayerManagerComponent()?.m_ItemInHands;
                GunItem gunItem = weaponInHands.m_GunItem;
                int spentCasings = gunItem != null ? gunItem.m_SpentCasingsInClip : 0;
                bool isAmmoSwitch = gunItem != null && gunItem.m_PreviousAmmoIndex >= 0;

                Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
                moddedAnimator.SetInteger("Reload_Count", bulletsToReload);
                moddedAnimator.SetInteger("Rounds_Loaded", roundsLoaded);
                moddedAnimator.SetInteger("SpentCasingsInClip", spentCasings);
                moddedAnimator.SetBool("Is_Loaded", roundsLoaded > 0);
                moddedAnimator.SetBool("IsAmmoSwitch", isAmmoSwitch);
                moddedAnimator.SetBool("Is_Jammed", willJam);
                moddedAnimator.SetBool("IsAimHeld", __instance.m_State == PlayerAnimation.State.Aiming);
                ShotgunSkill.ApplyAnimatorParams(moddedAnimator);
                moddedAnimator.SetTrigger("Trigger_Generic_Reload_Single");
            }
        }


        [HarmonyPatch(typeof(PlayerAnimation), nameof(PlayerAnimation.IsAllowedToFire))]
        internal static class PlayerAnimation_IsAllowedToFire_ShotgunOverride
        {
            internal static bool Prefix(PlayerAnimation __instance, bool allowHipFire, ref bool __result)
            {
                if (__instance.m_EquippedFirstPersonWeaponShoulder == null) return true;
                if (!__instance.m_EquippedFirstPersonWeaponShoulder.name.Contains("Shotgun")) return true;

                if (allowHipFire)
                {
                    int state = (int)__instance.m_State;
                    int blocked = (1 << (int)PlayerAnimation.State.Equipping) | (1 << (int)PlayerAnimation.State.Dequipping);
                    __result = ((1 << state) & blocked) == 0;
                }
                else
                {
                    __result = __instance.m_State == PlayerAnimation.State.Aiming;
                }
                return false;
            }
        }


        [HarmonyPatch(typeof(vp_FPSWeapon), nameof(vp_FPSWeapon.PlayFireAnimation))]
        internal static class vp_FPSWeapon_PlayFireAnimation
        {
            internal static bool Prefix(vp_FPSWeapon __instance)
            {
                if (__instance.m_GunItem == null) return true;
                if (__instance.m_GunItem.m_GunType != GunTypeCustom) return true;
                ApplyShotgunRecoil();
                return true;
            }
        }

        [HarmonyPatch(typeof(ResearchItem), nameof(ResearchItem.OnResearchComplete))]
        internal static class Patch_ResearchItem_OnResearchComplete
        {
            private static void Postfix(ResearchItem __instance)
            {
                if (__instance == null || __instance.gameObject == null)
                    return;

                string goName = __instance.gameObject.name;
                if (string.IsNullOrEmpty(goName) || !goName.StartsWith("GEAR_ShotgunBook"))
                    return;

                ShotgunSkill.IncreaseSkillPoints.Invoke("Shotgun", __instance.m_SkillPoints);
            }
        }


        [HarmonyPatch(typeof(Panel_Inventory_Examine), nameof(Panel_Inventory_Examine.OnRead))]
        internal static class Panel_Inventory_Examine_PatchOnRead
        {
            private static bool Prefix(Panel_Inventory_Examine __instance)
            {
                GearItem gi = __instance.m_GearItem;
                if (gi == null)
                    return false;

                ResearchItem ri = gi.m_ResearchItem;
                if (ri == null)
                    return false;

                if (!gi.name.StartsWith("GEAR_ShotgunBook"))
                    return false;

                if (ri.m_ElapsedHours >= ri.GetTimeRequirementHoursPrecise())
                {
                    GameAudioManager.PlayGUIError();
                    return true;
                }

                int currentTier = ShotgunSkill.GetCurrentTier();
                if (currentTier >= ri.m_NoBenefitAtSkillLevel - 1)
                {
                    GameAudioManager.PlayGUIError();
                    ri.m_ElapsedHours = ri.m_TimeRequirementHours;
                    return true;
                }

                if (__instance.MaybeAbortReadingWithHUDMessage())
                {
                    GameAudioManager.PlayGUIError();
                    return true;
                }

                GameAudioManager.PlayGuiConfirm();
                __instance.SelectWindow(__instance.m_ActionInProgressWindow);
                __instance.SetReadInProgress(true);

                UISlider slider = __instance.m_Slider_ActionProgress;
                if (slider == null)
                    return true;

                slider.value = 0f;
                __instance.m_ElapsedProgressBarSeconds = 0f;
                __instance.m_ProgressBarTimeSeconds = __instance.m_ReadTimeSeconds;

                int minutesRemaining = 0;
                if (ri.m_ElapsedHours < ri.GetTimeRequirementHoursPrecise())
                    minutesRemaining = Mathf.RoundToInt((ri.GetTimeRequirementHours() - ri.m_ElapsedHours) * 60f);

                int minutesToAdvance = Math.Min(__instance.m_HoursToRead * 60, minutesRemaining);
                __instance.AccelerateTimeOfDay(minutesToAdvance, false);

                GameObject emitter = InterfaceManager.GetSoundEmitter();
                __instance.m_ProgressBarAudio = GameAudioManager.PlaySound(ri.m_ReadAudio, emitter);
                __instance.SetInventoryAlpha(0f);

                return true;
            }
        }


        [HarmonyPatch(typeof(Panel_Inventory_Examine), nameof(Panel_Inventory_Examine.RefreshReadPanel))]
        internal static class Panel_Inventory_Examine_PatchRefreshReadPanel
        {
            private static bool Prefix(Panel_Inventory_Examine __instance)
            {
                GearItem gi = __instance.m_GearItem;
                if (gi == null)
                    return false;

                ResearchItem ri = gi.m_ResearchItem;
                if (ri == null)
                    return false;

                if (!gi.name.StartsWith("GEAR_ShotgunBook"))
                    return false;

                if (!ShotgunSkill.IsAvailable)
                    return false;

                (int points, int tier) s = ShotgunSkill.GetPointsAndTiers(ShotgunSkill.SkillName);

                if (__instance.m_SkillProgressBarLabel != null)
                    __instance.m_SkillProgressBarLabel.text = Localization.Get("GAMEPLAY_Shotgun_SkillName");

                if (__instance.m_SkillLevelLabel != null)
                    __instance.m_SkillLevelLabel.text = (s.tier + 1).ToString();

                Skill skill = ShotgunSkill.GetSkillObject();
                if (__instance.m_SkillProgressBar != null)
                    __instance.m_SkillProgressBar.value = skill != null ? skill.GetProgressToNextLevelAsNormalizedValue(0) : 0f;

                if (__instance.m_SkillProgressBarSprite != null)
                    __instance.m_SkillProgressBarSprite.fillAmount = skill != null ? Mathf.Clamp01(skill.GetProgressToNextLevelAsNormalizedValue(ri.m_SkillPoints)) : 0f;

                if (__instance.m_ResearchIcon != null)
                    __instance.m_ResearchIcon.spriteName = "ico_Shotgun_Skill";

                return true;
            }
        }

        [HarmonyPatch(typeof(ConsoleManager), nameof(ConsoleManager.Initialize))]
        internal class ConsoleManagerPatches_Initialize
        {
            private static void Postfix()
            {
                uConsole.RegisterCommand("shotgun_levelup", new Action(() =>
                {
                    ShotgunSkill.IncreaseSkillPoints.Invoke("Shotgun", 25);
                }));
            }
        }
    }
}
