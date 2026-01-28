using HarmonyLib;
using ModComponent.Utils;
using Il2Cpp;
using UnityEngine;
using MelonLoader;

namespace Shotgun
{
    internal class Patches
    {
        [HarmonyPatch(typeof(EquipItemPopup), nameof(EquipItemPopup.AllowedToHideAmmoPopup))]
        [HarmonyPriority(-999999)]
        internal static class EquipItemPopup_AllowedToHideAmmoPopup_ModComponentCompatibilityFix
        {
            internal static void Postfix(ref bool __result)
            {
                __result = __result && (GameManager.GetPlayerManagerComponent().m_ItemInHands == null || GameManager.GetPlayerManagerComponent().m_ItemInHands.m_GunItem == null);
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.PressSwitchAmmo))]
        internal static class GunItem_PreventSwitchAmmoUnlessIntended
        {
            internal static bool Prefix(GunItem __instance)
            {
                return __instance.m_GunType == GunType.Camera || (InputManager.HasContext(InputManager.m_CurrentContext) && Input.GetKey(KeyCode.T));
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.PressReloadAmmo))]
        internal static class GunItem_PreventReloadUnlessIntended
        {
            internal static bool Prefix(GunItem __instance)
            {
                return __instance.m_GunType == GunType.Camera || (InputManager.HasContext(InputManager.m_CurrentContext) && (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.T)));
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Start))]
        internal static class GunItem_UpdateOnStart
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.name.Contains("Shotgun"))
                {
                    UpdateShotgunParams(__instance, __instance.ValidAmmo[__instance.m_SelectedAmmoIndex].DisplayName);
                    CustomAmmoHUD.Current.Refresh();
                }
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Update))]
        internal static class GunItem_SwitchAmmoOnT
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.m_GunType != GunType.Camera && InputManager.HasContext(InputManager.m_CurrentContext) && Input.GetKeyDown(KeyCode.T))
                {
                    while (__instance.m_Clip.Count > 0)
                    {
                        GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(__instance.ValidAmmo[__instance.m_SelectedAmmoIndex].m_PrefabReference);
                        __instance.RemoveNextFromClip();
                    }
                    __instance.PressSwitchAmmo();
                    UpdateShotgunParams(__instance, __instance.ValidAmmo[__instance.m_SelectedAmmoIndex].DisplayName);
                    CustomAmmoHUD.Current.Refresh();
                }
            }
        }




        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Update))]
        internal static class GunItem_ReloadAmmoOnR
        {
            internal static void Postfix(GunItem __instance)
            {
                if (__instance.m_GunType != GunType.Camera && InputManager.HasContext(InputManager.m_CurrentContext) && Input.GetKeyDown(KeyCode.R))
                {
                    while (__instance.m_Clip.Count > 0)
                    {
                        GameManager.GetPlayerManagerComponent().InstantiateItemInPlayerInventory(__instance.ValidAmmo[__instance.m_SelectedAmmoIndex].m_PrefabReference);
                        __instance.RemoveNextFromClip();
                    }
                    __instance.PressReloadAmmo();
                    CustomAmmoHUD.Current.Refresh();
                }
            }
        }


        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Fired))]
        internal static class GunItem_RefreshCustomHUDOnFired
        {
            internal static void Postfix(GunItem __instance)
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
                        __result = false;
                        return;
                    case "GEAR_ShotgunAmmoBoxBuckshot":
                        __result = Utils.RollChance(__instance.GetChanceKill(weapon) * 0.05f);
                        return;
                }
            }
        }

        /*

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


        */


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
