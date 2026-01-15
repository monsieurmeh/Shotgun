using MelonLoader;
using UnityEngine;
using System.Reflection;
using Il2Cpp;
using MelonLoader.Utils;
using Shotgun.Util;
using UnityEngine.AddressableAssets;
using Il2CppVLB;
using HarmonyLib;
using Il2CppTLD.Gameplay;


namespace Shotgun
{
    public class Main : MelonMod
    {
        internal static Main? Instance { get; private set; }

        internal Configuration Configuration { get; private set; } = new();

        public override void OnEarlyInitializeMelon()
        {
            EarlyInitialize();
        }

        internal void EarlyInitialize()
        {
            try
            {
                MelonLogger.Msg("EarlyInitialize");
                Instance = this;
                string modsPath = MelonEnvironment.ModsDirectory;
                string basePath = Path.Combine(modsPath, DataFolderPath);
                Directory.CreateDirectory(basePath);
                bool shouldForceRefreshEmbeddedData = Versioning.IsVersionCorrect(basePath);
                EmbeddedResourceExtractor.RefreshEmbeddedData(modsPath, "Shotgun.modcomponent", shouldForceRefreshEmbeddedData);
                MelonLogger.Msg("EarlyInitialize Complete");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"EarlyInitialize Failed: {e}");
            }

        }

        internal void LateInitialize()
        {
            try
            {
                MelonLogger.Msg("LateInitialize");
                Configuration.ConfigureShotgun();
                MelonLogger.Msg("LateInitialize Complete");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"LateInitialize Failed: {e}");
            }
        }
    }


    [HarmonyPatch(typeof(vp_FPSPlayer), nameof(vp_FPSPlayer.Start))]
    internal static class SuperLateInitializePatch
    {
        internal static void Postfix() => Main.Instance?.LateInitialize();
    }


    internal class ConfigurationReferences
    {
        internal vp_FPSPlayer? FPSPlayer { get; set; }
        internal vp_FPSCamera? FPSCamera { get; set; }
        internal GunRefs? Shotgun { get; set; } = new();
        internal GunRefs? Rifle { get; set; } = new();
    }


    internal class GunRefs
    {
        internal ItemRefs Item { get; set; } = new();
        internal WeaponViewRefs WeaponView { get; set; } = new();
        internal FPHRefs FPH { get; set; } = new();
        internal FloatingRefs Floating { get; set; } = new();
    }


    internal class ItemRefs
    {
        internal GameObject? Prefab { get; set; }
        internal GearItem? GearItem { get; set; }
        internal GunItem? GunItem { get; set; }
        internal FirstPersonItem? FirstPersonItem { get; set; }
        internal StruggleBonus? StruggleBonus { get; set; }
    }


    internal class FPHRefs
    {
        internal GameObject? Root { get; set; }
        internal FirstPersonWeapon? FirstPersonWeapon { get; set; }
        internal Animator? Animator { get; set; }
    }


    internal class WeaponViewRefs
    {
        internal GameObject? Root { get; set; }
        internal GameObject? Child { get; set; }
        internal vp_FPSWeapon? FPSWeapon { get; set; }
        internal vp_FPSShooter? FPSShooter { get; set; }
    }


    internal class FloatingRefs
    {
        internal FPSItem? FPSItem { get; set; }
    }


    internal class Configuration
    {
        internal ConfigurationReferences ConfigRefs { get; set; } = new();

        internal void ConfigureShotgun()
        {
            if (!GetFPSPlayer(ConfigRefs)) return;
            if (!GetFPSCamera(ConfigRefs)) return;
            if (!ConfigureRadial(ConfigRefs)) return;

            if (!GetRifleAsset(ConfigRefs)) return;
            if (!GetRifleFirstPersonItem(ConfigRefs)) return;
            if (!GetRifleFPSItem(ConfigRefs)) return;

            if (!InstantiateShotgunWeaponGroup(ConfigRefs)) return;
            if (!InstantiateShotgunVPFPSWeapon(ConfigRefs)) return;
            if (!InstantiateShotgunVPFPSShooter(ConfigRefs)) return;

            if (!InstantiateShotgunFPH(ConfigRefs)) return;
            if (!InstantiateShotgunFirstPersonWeapon(ConfigRefs)) return;
            if (!InstantiateShotgunAnimator(ConfigRefs)) return;

            if (!InstantiateShotgunPrefab(ConfigRefs)) return;
            if (!GetShotgunGearItem(ConfigRefs)) return;
            if (!InstantiateShotgunStruggleBonus(ConfigRefs)) return;
            if (!InstantiateShotgunGunItem(ConfigRefs)) return;
            if (!InstantiateShotgunFirstPersonItem(ConfigRefs)) return;
            if (!InstantiateShotgunFPSItem(ConfigRefs)) return;

            if (!ConfigureDetails(ConfigRefs)) return;
        }

        internal bool ConfigureDetails(ConfigurationReferences refs)
        {
            try
            {
                refs.Shotgun.Item.GearItem.m_StruggleBonus = refs.Shotgun.Item.StruggleBonus;
                refs.Shotgun.Item.GunItem.m_GunType = GunType.Rifle;
                refs.Shotgun.Item.FirstPersonItem.m_FirstPersonObjectName = "Shotgun";
                refs.Shotgun.Item.FirstPersonItem.m_ItemData = refs.Shotgun.Floating.FPSItem;
                refs.Shotgun.Item.StruggleBonus.m_StruggleWeaponType = StruggleBonus.StruggleWeaponType.Rifle;

                refs.Shotgun.Floating.FPSItem.name = "FPSItem_Shotgun";
                refs.Shotgun.Floating.FPSItem.m_FPSWeaponPrefab = refs.Rifle.Floating.FPSItem.m_FPSWeaponPrefab;

                refs.Shotgun.FPH.FirstPersonWeapon.m_Renderable = refs.Shotgun.FPH.Root;

                refs.Shotgun.WeaponView.Root.transform.SetParent(refs.FPSCamera.m_WeaponParent, false);
                refs.Shotgun.WeaponView.FPSShooter.enabled = true;
                refs.Shotgun.WeaponView.FPSWeapon.m_WeaponGroup = refs.Shotgun.WeaponView.Root;
                refs.Shotgun.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulderPrefab = refs.Shotgun.FPH.Root;
                refs.Shotgun.WeaponView.FPSWeapon.m_GearItem = refs.Shotgun.Item.Prefab.GetComponent<GearItem>();
                refs.Shotgun.WeaponView.FPSWeapon.m_GunItem = refs.Shotgun.Item.Prefab.GetComponent<GunItem>();
                refs.Shotgun.WeaponView.FPSWeapon.MaybeBuildRuntimeHierarchy();
                refs.Shotgun.WeaponView.FPSWeapon.MaybeSpawnFirstPersonWeapon();
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error during detailed configuration: {e}");
                return false;
            }
        }


        internal bool GetFPSPlayer(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetFPSPlayer");
            refs.FPSPlayer = GameManager.GetVpFPSPlayer();
            if (refs.FPSPlayer == null) MelonLogger.Error("Could not summon vp_FPSPlayer");
            return refs.FPSPlayer != null;
        }


        internal bool GetFPSCamera(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetFPSCamera");
            refs.FPSCamera = refs.FPSPlayer?.FPSCamera ?? null;
            if (refs.FPSCamera == null) MelonLogger.Error("Could not get vp_FPSCamera from vp_FPSPlayer instance");
            return refs.FPSCamera != null;
        }


        internal bool InstantiateShotgunPrefab(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunPrefab");
            refs.Shotgun.Item.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Shotgun").WaitForCompletion();
            if (refs.Shotgun.Item.Prefab == null) MelonLogger.Error("Could not summon GEAR_Shotgun");
            return refs.Shotgun.Item.Prefab != null;
        }


        internal bool GetShotgunGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureGearItem");
            refs.Shotgun.Item.GearItem = refs.Shotgun.Item.Prefab?.GetComponent<GearItem>();
            if (refs.Shotgun.Item.GearItem == null) MelonLogger.Error("Null Shotgun GearItem");
            return refs.Shotgun.Item.GearItem != null;
        }


        internal bool InstantiateShotgunStruggleBonus(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureStruggleBonus");
            refs.Shotgun.Item.StruggleBonus = refs.Shotgun.Item.Prefab?.GetOrCreateComponent<StruggleBonus>() ?? null;
            if (refs.Shotgun.Item.StruggleBonus == null)
            {
                MelonLogger.Error("Null StruggleBonus");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunGunItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureGunItem");
            refs.Shotgun.Item.GunItem = refs.Shotgun.Item.Prefab?.GetOrCreateComponent<GunItem>() ?? null;
            if (refs.Shotgun.Item.GunItem == null)
            {
                MelonLogger.Error("Null GunItem");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunFirstPersonItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureFirstPersonItem");
            refs.Shotgun.Item.FirstPersonItem = refs.Shotgun.Item.Prefab?.GetOrCreateComponent<FirstPersonItem>() ?? null;
            if (refs.Shotgun.Item.FirstPersonItem == null)
            {
                MelonLogger.Error("Null FirstPersonItem");
                return false;
            }
            return true;
        }


        internal bool ConfigureRadial(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureRadial");
            Panel_ActionsRadial radialActionsPanel = InterfaceManager.GetPanel<Panel_ActionsRadial>();
            if (radialActionsPanel == null)
            {
                MelonLogger.Error("Null Panel_ActionsRadial");
                return false;
            }
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray newWeaponFilterList = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray(radialActionsPanel.m_WeaponRadialOrder.Length + 1);
            for (int i = 0, iMax = radialActionsPanel.m_WeaponRadialOrder.Length; i < iMax; i++)
            {
                newWeaponFilterList[i] = radialActionsPanel.m_WeaponRadialOrder[i];
            }
            newWeaponFilterList[^1] = "GEAR_Shotgun";
            radialActionsPanel.m_WeaponRadialOrder = newWeaponFilterList;
            return true;
        }


        internal bool InstantiateShotgunWeaponGroup(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunWeaponGroup");
            refs.Shotgun.WeaponView.Root = new GameObject("FPSWeapon_ShotgunTransform");
            refs.Shotgun.WeaponView.Child = new GameObject("Shotgun");
            refs.Shotgun.WeaponView.Child.transform.SetParent(refs.Shotgun.WeaponView.Root.transform, false);
            return true;
        }


        internal bool InstantiateShotgunVPFPSWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunVPFPSWeapon");
            refs.Shotgun.WeaponView.FPSWeapon = refs.Shotgun.WeaponView.Child.GetOrCreateComponent<vp_FPSWeapon>();
            if (refs.Shotgun.WeaponView.FPSWeapon == null)
            {
                MelonLogger.Error("Null vp_FPSWeapon");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunVPFPSShooter(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunVPFPSShooter");
            refs.Shotgun.WeaponView.FPSShooter = refs.Shotgun.WeaponView.Child.GetOrCreateComponent<vp_FPSShooter>();
            if (refs.Shotgun.WeaponView.FPSShooter == null)
            {
                MelonLogger.Error("Null vp_FPSShooter");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunFPSItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSItem");
            refs.Shotgun.Floating.FPSItem = ScriptableObject.CreateInstance<FPSItem>();
            if (refs.Shotgun.Floating.FPSItem == null)
            {
                MelonLogger.Error("Null FPSItem");
                return false;
            }
            return true;
        }


        internal bool GetRifleAsset(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleAsset");
            refs.Rifle.Item.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Rifle").WaitForCompletion();
            if (refs.Rifle.Item.Prefab == null)
            {
                MelonLogger.Error("Could not summon rifle prefab");
                return false;
            }
            return true;
        }


        internal bool GetRifleFirstPersonItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleFirstPersonItem");
            refs.Rifle.Item.FirstPersonItem = refs.Rifle.Item.Prefab.GetComponent<FirstPersonItem>();
            if (refs.Rifle.Item.FirstPersonItem == null)
            {
                MelonLogger.Error("Could not find rifle first person item");
                return false;
            }
            return true;
        }


        internal bool GetRifleFPSItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleFPSItem");
            refs.Rifle.Floating.FPSItem = refs.Rifle.Item.FirstPersonItem.m_ItemData;
            if (refs.Rifle.Item.FirstPersonItem == null)
            {
                MelonLogger.Error("Could not find rifle first person item");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunFPH(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunFPH");
            refs.Shotgun.FPH.Root = Addressables.LoadAssetAsync<GameObject>("FPH_Shotgun").WaitForCompletion();
            if (refs.Shotgun.FPH.Root == null)
            {
                MelonLogger.Error("Could not summon FPH_Shotgun");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunFirstPersonWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunFirstPersonWeapon");
            refs.Shotgun.FPH.FirstPersonWeapon = refs.Shotgun.FPH.Root.GetOrCreateComponent<FirstPersonWeapon>();
            if (refs.Shotgun.FPH.FirstPersonWeapon == null)
            {
                MelonLogger.Error("Could not attach FirstPersonWeapon to FPH_Shotgun");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunAnimator(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunAnimator");
            refs.Shotgun.FPH.Animator = refs.Shotgun.FPH.Root.GetOrCreateComponent<Animator>();
            if (refs.Shotgun.FPH.FirstPersonWeapon == null)
            {
                MelonLogger.Error("Could not attach Animator to FPH_Shotgun");
                return false;
            }
            return true;
        }


        internal bool RegisterShotgunFPSItemAndFPSWeapon(ConfigurationReferences refs)
        {
            if (!refs.FPSCamera.m_Weapons.TryAdd(refs.Shotgun.Floating.FPSItem, refs.Shotgun.WeaponView.FPSWeapon))
            {
                MelonLogger.Error("Failed to add shotgun to vpFPSCamera.m_weapons dictionary");
                return false;
            }
            return true;
        }


    }
}