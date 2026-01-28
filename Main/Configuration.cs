using Il2Cpp;
using Il2CppTLD.Gameplay;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;
using Shotgun.Utility;
using Harmony;
using ModComponent.API.Components;
using System.Runtime.CompilerServices;
using Il2CppTLD.Gear;
using Il2CppSystem.Collections.Generic;

namespace Shotgun
{
    internal class Configuration
    {
        internal ConfigurationReferences Refs { get; set; }

        internal bool PrepareShotgun()
        {
            try
            {
                MelonLogger.Msg("Preparing shotgun...");
                ConfigurationReferences configRefs = new();
                Refs = configRefs;
                if (!Get(configRefs)) return false;
                if (!Instantiate(configRefs)) return false;
                if (!Configure(configRefs)) return false;
                if (!RegisterShotgun(configRefs)) return false;
                if (!SetupTest.Validate(configRefs)) return false;
                ReportDifferences(configRefs);
                MelonLogger.Msg("Ready to roll!");
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error during PrepareShotgun: {e}");
                return false;
            }
        }

        internal bool RegisterShotgun(ConfigurationReferences refs)
        {
            MelonLogger.Msg("RegisterShotgunFPSItemAndFPSWeapon");
            if (!refs.FPSCamera.m_Weapons.TryAdd(refs.Shotgun.Floating.FPSItem, refs.Shotgun.WeaponView.FPSWeapon))
            {
                MelonLogger.Error("Failed to add shotgun to vpFPSCamera.m_weapons dictionary");
                return false;
            }
            return true;
        }


        internal void ReportDifferences(ConfigurationReferences refs)
        {

        }


        internal static bool Attempt(Action action, Func<bool> test, string errorMessage, [CallerMemberName] string methodName = "")
        {
            MelonLogger.Msg(methodName);
            action.Invoke();
            if (!test.Invoke())
            {
                MelonLogger.Error($"[CONFIG FAIL]: {errorMessage}");
                return false;
            }
            return true;
        }


        #region Get



        internal bool Get(ConfigurationReferences refs)
        {
            if (!GetTopLevel(refs)) return false;
            if (!GetRifle(refs)) return false;
            if (!GetShotgun(refs)) return false;
            return true;
        }


        internal bool GetTopLevel(ConfigurationReferences refs)
        {
            if (!GetFPSPlayer(refs)) return false;
            if (!GetFPSCamera(refs)) return false;
            return true;
        }


        internal static readonly string[] ShotgunAmmoTypes = { "Slug", "Buckshot", "Birdshot" };

        internal bool GetShotgun(ConfigurationReferences refs)
        {
            if (!GetShotgunPrefab(refs)) return false;
            if (!GetShotgunGearItem(refs)) return false;
            if (!GetShotgunModComponent(refs)) return false;
            if (!GetShotgunCasing(refs)) return false;
            foreach (var ammoType in ShotgunAmmoTypes)
            {
                refs.Shotgun.AmmoTypes[ammoType] = new ShotgunAmmoTypeRefs();
                if (!GetShotgunAmmoSingle(refs, ammoType)) return false;
                if (!GetShotgunAmmoBox(refs, ammoType)) return false;
            }
            return true;
        }


        internal bool GetRifle(ConfigurationReferences refs)
        {
            if (!GetRifleAsset(refs)) return false;
            if (!GetRifleFirstPersonItem(refs)) return false;
            if (!GetRifleFPSItem(refs)) return false;
            if (!GetRifleVPFPSRoot(refs)) return false;
            if (!GetRifleVPFPSWeapon(refs)) return false;
            if (!GetRifleVPFPShooter(refs)) return false;
            if (!GetRifleFPH(refs)) return false;
            if (!GetRifleFirstPersonWeapon(refs)) return false;
            if (!GetRifleAnimator(refs)) return false;
            if (!GetRifleGearItem(refs)) return false;
            if (!GetRifleGunItem(refs)) return false;
            if (!GetRifleAmmoSingle(refs)) return false;
            if (!GetRifleAmmoBox(refs)) return false;
            if (!GetRifleCasing(refs)) return false;
            return true;
        }


        internal bool GetRifleAmmoSingle(ConfigurationReferences refs)
        {
            if (!GetRifleAmmoSingleAsset(refs)) return false;
            if (!GetRifleAmmoSingleGearItem(refs)) return false;
            if (!GetRifleAmmoSingleAmmoItem(refs)) return false;
            if (!GetRifleAmmoSingleStackableItem(refs)) return false;
            if (!GetRifleAmmoSingleHarvestable(refs)) return false;
            return true;
        }


        internal bool GetRifleAmmoBox(ConfigurationReferences refs)
        {
            if (!GetRifleAmmoBoxAsset(refs)) return false;
            if (!GetRifleAmmoBoxGearItem(refs)) return false;
            if (!GetRifleAmmoBoxAmmoItem(refs)) return false;
            if (!GetRifleAmmoBoxStackableItem(refs)) return false;
            if (!GetRifleAmmoBoxHarvestable(refs)) return false;
            return true;
        }

        internal bool GetRifleCasing(ConfigurationReferences refs)
        {
            if (!GetRifleCasingAsset(refs)) return false;
            if (!GetRifleCasingGearItem(refs)) return false;
            if (!GetRifleCasingStackableItem(refs)) return false;
            return true;
        }


        internal bool GetShotgunAmmoSingle(ConfigurationReferences refs, string ammoType)
        {
            if (!GetShotgunAmmoSingleAsset(refs, ammoType)) return false;
            if (!GetShotgunAmmoSingleGearItem(refs, ammoType)) return false;
            if (!GetShotgunAmmoSingleAmmoItem(refs, ammoType)) return false;
            if (!GetShotgunAmmoSingleStackableItem(refs, ammoType)) return false;
            if (!GetShotgunAmmoSingleHarvestable(refs, ammoType)) return false;
            return true;
        }


        internal bool GetShotgunAmmoBox(ConfigurationReferences refs, string ammoType)
        {
            if (!GetShotgunAmmoBoxAsset(refs, ammoType)) return false;
            if (!GetShotgunAmmoBoxGearItem(refs, ammoType)) return false;
            if (!GetShotgunAmmoBoxAmmoItem(refs, ammoType)) return false;
            if (!GetShotgunAmmoBoxStackableItem(refs, ammoType)) return false;
            if (!GetShotgunAmmoBoxHarvestable(refs, ammoType)) return false;
            return true;
        }

        internal bool GetShotgunCasing(ConfigurationReferences refs)
        {
            if (!GetShotgunCasingAsset(refs)) return false;
            if (!GetShotgunCasingGearItem(refs)) return false;
            if (!GetShotgunCasingStackableItem(refs)) return false;
            return true;
        }


        internal bool GetRifleAmmoSingleAsset(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoSingle.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_RifleAmmoSingle").WaitForCompletion(), () => refs.Rifle.AmmoSingle.Prefab != null, "Null GEAR_RifleAmmoSingle");
        internal bool GetRifleAmmoSingleGearItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoSingle.GearItem = refs.Rifle.AmmoSingle.Prefab.GetComponent<GearItem>(), () => refs.Rifle.AmmoSingle.GearItem != null, "Null GEAR_RifleAmmoSingle.GearItem");
        internal bool GetRifleAmmoSingleAmmoItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoSingle.AmmoItem = refs.Rifle.AmmoSingle.Prefab.GetComponent<AmmoItem>(), () => refs.Rifle.AmmoSingle.AmmoItem != null, "Null GEAR_RifleAmmoSingle.AmmoItem");
        internal bool GetRifleAmmoSingleStackableItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoSingle.StackableItem = refs.Rifle.AmmoSingle.Prefab.GetComponent<StackableItem>(), () => refs.Rifle.AmmoSingle.StackableItem != null, "Null GEAR_RifleAmmoSingle.StackableItem");
        internal bool GetRifleAmmoSingleHarvestable(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoSingle.Harvestable = refs.Rifle.AmmoSingle.Prefab.GetComponent<Harvestable>(), () => refs.Rifle.AmmoSingle.GearItem != null, "Null GEAR_RifleAmmoSingle.Harvestable");

        internal bool GetRifleAmmoBoxAsset(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoBox.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_RifleAmmoBox").WaitForCompletion(), () => refs.Rifle.AmmoBox.Prefab != null, "Null GEAR_RifleAmmoBox");
        internal bool GetRifleAmmoBoxGearItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoBox.GearItem = refs.Rifle.AmmoBox.Prefab.GetComponent<GearItem>(), () => refs.Rifle.AmmoBox.GearItem != null, "Null GEAR_RifleAmmoBox.GearItem");
        internal bool GetRifleAmmoBoxAmmoItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoBox.AmmoItem = refs.Rifle.AmmoBox.Prefab.GetComponent<AmmoItem>(), () => refs.Rifle.AmmoBox.AmmoItem != null, "Null GEAR_RifleAmmoBox.AmmoItem");
        internal bool GetRifleAmmoBoxStackableItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoBox.StackableItem = refs.Rifle.AmmoBox.Prefab.GetComponent<StackableItem>(), () => refs.Rifle.AmmoBox.StackableItem != null, "Null GEAR_RifleAmmoBox.StackableItem");
        internal bool GetRifleAmmoBoxHarvestable(ConfigurationReferences refs) => Attempt(() => refs.Rifle.AmmoBox.Harvestable = refs.Rifle.AmmoBox.Prefab.GetComponent<Harvestable>(), () => refs.Rifle.AmmoBox.GearItem != null, "Null GEAR_RifleAmmoBox.Harvestable");
        
        internal bool GetRifleCasingAsset(ConfigurationReferences refs) => Attempt(() => refs.Rifle.Casing.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_RifleAmmoCasing").WaitForCompletion(), () => refs.Rifle.Casing.Prefab != null, "Null GEAR_RifleAmmoCasing");
        internal bool GetRifleCasingGearItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.Casing.GearItem = refs.Rifle.Casing.Prefab.GetComponent<GearItem>(), () => refs.Rifle.Casing.GearItem != null, "Null GEAR_RifleAmmoCasing.GearItem");
        internal bool GetRifleCasingStackableItem(ConfigurationReferences refs) => Attempt(() => refs.Rifle.Casing.StackableItem = refs.Rifle.Casing.Prefab.GetComponent<StackableItem>(), () => refs.Rifle.Casing.StackableItem != null, "Null GEAR_RifleAmmoCasing.StackableItem");

        internal bool GetShotgunAmmoSingleAsset(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab = Addressables.LoadAssetAsync<GameObject>($"GEAR_ShotgunAmmoSingle{ammoType}").WaitForCompletion(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab != null, $"Null GEAR_ShotgunAmmoSingle{ammoType}");
        internal bool GetShotgunAmmoSingleGearItem(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.GearItem = refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab.GetComponent<GearItem>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.GearItem != null, $"Null GEAR_ShotgunAmmoSingle{ammoType}.GearItem");
        internal bool GetShotgunAmmoSingleAmmoItem(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.AmmoItem = refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab.GetComponent<AmmoItem>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.AmmoItem != null, $"Null GEAR_ShotgunAmmoSingle{ammoType}.AmmoItem");
        internal bool GetShotgunAmmoSingleStackableItem(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.StackableItem = refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab.GetComponent<StackableItem>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.StackableItem != null, $"Null GEAR_ShotgunAmmoSingle{ammoType}.StackableItem");
        internal bool GetShotgunAmmoSingleHarvestable(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Harvestable = refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.Prefab.GetComponent<Harvestable>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoSingle.GearItem != null, $"Null GEAR_ShotgunAmmoSingle{ammoType}.Harvestable");

        internal bool GetShotgunAmmoBoxAsset(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab = Addressables.LoadAssetAsync<GameObject>($"GEAR_ShotgunAmmoBox{ammoType}").WaitForCompletion(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab != null, $"Null GEAR_ShotgunAmmoBox{ammoType}");
        internal bool GetShotgunAmmoBoxGearItem(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.GearItem = refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab.GetComponent<GearItem>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.GearItem != null, $"Null GEAR_ShotgunAmmoBox{ammoType}.GearItem");
        internal bool GetShotgunAmmoBoxAmmoItem(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.AmmoItem = refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab.GetComponent<AmmoItem>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.AmmoItem != null, $"Null GEAR_ShotgunAmmoBox{ammoType}.AmmoItem");
        internal bool GetShotgunAmmoBoxStackableItem(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.StackableItem = refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab.GetComponent<StackableItem>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.StackableItem != null, $"Null GEAR_ShotgunAmmoBox{ammoType}.StackableItem");
        internal bool GetShotgunAmmoBoxHarvestable(ConfigurationReferences refs, string ammoType) => Attempt(() => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Harvestable = refs.Shotgun.AmmoTypes[ammoType].AmmoBox.Prefab.GetComponent<Harvestable>(), () => refs.Shotgun.AmmoTypes[ammoType].AmmoBox.GearItem != null, $"Null GEAR_ShotgunAmmoBox{ammoType}.Harvestable");

        internal bool GetShotgunCasingAsset(ConfigurationReferences refs) => Attempt(() => refs.Shotgun.Casing.Prefab = Addressables.LoadAssetAsync<GameObject>($"GEAR_ShotgunAmmoCasing").WaitForCompletion(), () => refs.Shotgun.Casing.Prefab != null, $"Null GEAR_ShotgunAmmoCasing");
        internal bool GetShotgunCasingGearItem(ConfigurationReferences refs) => Attempt(() => refs.Shotgun.Casing.GearItem = refs.Shotgun.Casing.Prefab.GetComponent<GearItem>(), () => refs.Shotgun.Casing.GearItem != null, $"Null GEAR_ShotgunAmmoCasing.GearItem");
        internal bool GetShotgunCasingStackableItem(ConfigurationReferences refs) => Attempt(() => refs.Shotgun.Casing.StackableItem = refs.Shotgun.Casing.Prefab.GetComponent<StackableItem>(), () => refs.Shotgun.Casing.StackableItem != null, $"Null GEAR_ShotgunAmmoCasing.StackableItem");

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

        internal bool GetRifleAsset(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleAsset");
            refs.Rifle.Gun.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Rifle").WaitForCompletion();
            if (refs.Rifle.Gun.Prefab == null)
            {
                MelonLogger.Error("Could not summon rifle prefab");
                return false;
            }
            return true;
        }


        internal bool GetRifleFirstPersonItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleFirstPersonItem");
            refs.Rifle.Gun.FirstPersonItem = refs.Rifle.Gun.Prefab.GetComponent<FirstPersonItem>();
            if (refs.Rifle.Gun.FirstPersonItem == null)
            {
                MelonLogger.Error("Could not find rifle first person item");
                return false;
            }
            return true;
        }


        internal bool GetRifleFPSItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleFPSItem");
            refs.Rifle.Floating.FPSItem = refs.Rifle.Gun.FirstPersonItem.m_ItemData;
            if (refs.Rifle.Gun.FirstPersonItem == null)
            {
                MelonLogger.Error("Could not find rifle first person item");
                return false;
            }
            return true;
        }

        internal bool GetRifleVPFPSRoot(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleVPFPSWeapon");
            foreach (var kvp in refs.FPSCamera.m_Weapons)
            {
                if (kvp.Value.name.Contains("Rifle"))
                {
                    refs.Rifle.WeaponView.Root = kvp.Value.gameObject;
                    break;
                }
            }
            if (refs.Rifle.WeaponView.Root == null)
            {
                MelonLogger.Error("Could not find rifle vp_FPSWeapon script amongst all of vp_FPSCamera.m_Weapons");
                return false;
            }
            return true;
        }


        internal bool GetRifleVPFPSWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleVPFPSWeapon");
            refs.Rifle.WeaponView.FPSWeapon = refs.Rifle.WeaponView.Root.GetComponent<vp_FPSWeapon>();
            if (refs.Rifle.WeaponView.FPSWeapon == null)
            {
                MelonLogger.Error("Could not find rifle vp_FPSWeapon");
                return false;
            }
            return true;
        }


        internal bool GetRifleVPFPShooter(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleVPFPShooter");
            refs.Rifle.WeaponView.FPSShooter = refs.Rifle.WeaponView.Root.GetComponent<vp_FPSShooter>();
            if (refs.Rifle.WeaponView.FPSWeapon == null)
            {
                MelonLogger.Error("Could not find rifle vp_FPSShooter");
                return false;
            }
            return true;
        }


        internal bool GetRifleFPH(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleFPH");
            refs.Rifle.FPH.Root = refs.Rifle.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulder.gameObject;
            if (refs.Rifle.FPH.Root == null)
            {
                MelonLogger.Error("Could not find rifle FPH root from rifle vp_FPSWeapon");
                return false;
            }
            return true;
        }


        internal bool GetRifleFirstPersonWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleFirstPersonWeapon");
            refs.Rifle.FPH.FirstPersonWeapon = refs.Rifle.FPH.Root.GetComponent<FirstPersonWeapon>();
            if (refs.Rifle.FPH.FirstPersonWeapon == null)
            {
                MelonLogger.Error("Could not find rifle first person weapon from rifle FPH root");
                return false;
            }
            return true;
        }


        internal bool GetRifleAnimator(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleAnimator");
            refs.Rifle.FPH.Animator = refs.Rifle.FPH.Root.GetComponent<Animator>();
            if (refs.Rifle.FPH.Animator == null)
            {
                MelonLogger.Error("Could not find rifle animator from rifle FPH root");
                return false;
            }
            return true;
        }


        internal bool GetRifleGunItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleGunItem");
            refs.Rifle.Gun.GunItem = refs.Rifle.Gun.Prefab.GetComponent<GunItem>();
            if (refs.Rifle.Gun.GearItem == null)
            {
                MelonLogger.Error("Could not find rifle GunItem from rifle prefab");
                return false;
            }
            return true;
        }

        internal bool GetRifleGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleGearItrm");
            refs.Rifle.Gun.GearItem = refs.Rifle.Gun.Prefab.GetComponent<GearItem>();
            if (refs.Rifle.Gun.GearItem == null)
            {
                MelonLogger.Error("Could not find rifle gearitem from rifle prefab");
                return false;
            }
            return true;
        }


        internal bool GetShotgunPrefab(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetShotgunPrefab");
            refs.Shotgun.Gun.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Shotgun").WaitForCompletion();
            if (refs.Shotgun.Gun.Prefab == null) MelonLogger.Error("Could not summon GEAR_Shotgun");
            return refs.Shotgun.Gun.Prefab != null;
        }


        internal bool GetShotgunGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetShotgunGearItem");
            refs.Shotgun.Gun.GearItem = refs.Shotgun.Gun.Prefab?.GetComponent<GearItem>();
            if (refs.Shotgun.Gun.GearItem == null) MelonLogger.Error("Null Shotgun GearItem");
            return refs.Shotgun.Gun.GearItem != null;
        }


        internal bool GetShotgunModComponent(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetShotgunModComponent");
            refs.Shotgun.Gun.ModComponent = refs.Shotgun.Gun.Prefab?.GetComponent<ModGenericComponent>();
            if (refs.Shotgun.Gun.GearItem == null) MelonLogger.Error("Null Shotgun GearItem");
            return refs.Shotgun.Gun.GearItem != null;
        }


        #endregion


        #region Instantiate

        internal bool Instantiate(ConfigurationReferences refs)
        {
            if (!InstantiateTopLevel(refs)) return false;
            if (!InstantiateShotgun(refs)) return false;
            return true;
        }


        internal bool InstantiateTopLevel(ConfigurationReferences refs)
        {
            //nothing top level to instantiate yet, so far I've been able to do everything with vanilla logic.
            return true;
        }


        internal bool InstantiateShotgun(ConfigurationReferences refs)
        {
            if (!InstantiateShotgunFPH(refs)) return false;
            if (!InstantiateShotgunFirstPersonWeapon(refs)) return false;
            if (!InstantiateShotgunAnimator(refs)) return false;
            if (!InstantiateShotgunWeaponGroup(refs)) return false;
            if (!InstantiateShotgunVPFPSWeapon(refs)) return false;
            if (!InstantiateShotgunVPFPSShooter(refs)) return false;
            if (!InstantiateShotgunStruggleBonus(refs)) return false;
            if (!InstantiateShotgunGunItem(refs)) return false;
            if (!InstantiateShotgunFirstPersonItem(refs)) return false;
            if (!InstantiateShotgunFPSItem(refs)) return false;
            return true;
        }


        internal bool InstantiateShotgunStruggleBonus(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunStruggleBonus");
            refs.Shotgun.Gun.StruggleBonus = refs.Shotgun.Gun.Prefab?.GetOrCreateComponent<StruggleBonus>() ?? null;
            if (refs.Shotgun.Gun.StruggleBonus == null)
            {
                MelonLogger.Error("Null StruggleBonus");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunGunItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunGunItem");
            refs.Shotgun.Gun.GunItem = refs.Shotgun.Gun.Prefab?.GetOrCreateComponent<GunItem>() ?? null;
            if (refs.Shotgun.Gun.GunItem == null)
            {
                MelonLogger.Error("Null GunItem");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunFirstPersonItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunFirstPersonItem");
            refs.Shotgun.Gun.FirstPersonItem = refs.Shotgun.Gun.Prefab?.GetOrCreateComponent<FirstPersonItem>() ?? null;
            if (refs.Shotgun.Gun.FirstPersonItem == null)
            {
                MelonLogger.Error("Null FirstPersonItem");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunWeaponGroup(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunWeaponGroup");
            refs.Shotgun.WeaponView.Root = new GameObject();
            refs.Shotgun.WeaponView.Root.transform.SetParent(refs.FPSCamera.m_WeaponParent.transform, false);
            return true;
        }


        internal bool InstantiateShotgunVPFPSWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunVPFPSWeapon");
            refs.Shotgun.WeaponView.FPSWeapon = refs.Shotgun.WeaponView.Root.GetOrCreateComponent<vp_FPSWeapon>();
            if (refs.Shotgun.WeaponView.FPSWeapon == null)
            {
                MelonLogger.Error("Null vp_FPSWeapon");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunVPFPSShooter(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunVPFPSShooter");
            refs.Shotgun.WeaponView.FPSShooter = refs.Shotgun.WeaponView.Root.GetOrCreateComponent<vp_FPSShooter>();
            if (refs.Shotgun.WeaponView.FPSShooter == null)
            {
                MelonLogger.Error("Null vp_FPSShooter");
                return false;
            }
            return true;
        }


        internal bool InstantiateShotgunFPSItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("InstantiateShotgunFPSItem");
            refs.Shotgun.Floating.FPSItem = ScriptableObject.CreateInstance<FPSItem>();
            if (refs.Shotgun.Floating.FPSItem == null)
            {
                MelonLogger.Error("Null FPSItem");
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

        #endregion


        #region Configure

        private bool Configure(ConfigurationReferences refs)
        {
            if (!ConfigureTopLevel(refs)) return false;
            if (!ConfigureShotgun(refs)) return false;
            return true;
        }


        internal bool ConfigureTopLevel(ConfigurationReferences refs)
        {
            if (!ConfigureRadial(refs)) return false;
            return true;
        }


        internal bool ConfigureShotgun(ConfigurationReferences refs)
        {
            try
            {
                foreach (var ammoType in ShotgunAmmoTypes)
                {
                    ConfigureShotgunAmmoSingle(refs, ammoType);
                    ConfigureShotgunAmmoBox(refs, ammoType);
                    ConfigureShotgunAmmoCasing(refs, ammoType);
                }
                ConfigureShotgunGearItem(refs);
                ConfigureShotgunGunItem(refs);
                ConfigureShotgunFirstPersonItem(refs);
                ConfigureShotgunStruggleBonus(refs);
                ConfigureShotgunFPSItem(refs);
                ConfigureShotgunFPHRoot(refs);
                ConfigureShotgunFirstPersonWeapon(refs);
                ConfigureShotgunAnimator(refs);
                ConfigureShotgunFPSShooter(refs);
                ConfigureShotgunFPSWeapon(refs);
                ConfigureShotgunFPSRoot(refs);
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error during detailed configuration: {e}");
                return false;
            }
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

        internal void ConfigureShotgunAmmoSingle(ConfigurationReferences refs, string ammoType)
        {
            MelonLogger.Msg($"ConfigureShotgunAmmoSingle{ammoType}");
            var ammoRefs = refs.Shotgun.AmmoTypes[ammoType];
            ammoRefs.AmmoSingle.AmmoItem.m_AmmoForGunType = (GunType)((int)(GunType.Camera) + 1);
            ammoRefs.AmmoSingle.AmmoItem.m_LoadedHudIconSpriteName = "";
            ammoRefs.AmmoSingle.AmmoItem.m_EmptyHudIconSpriteName = "";
            ammoRefs.AmmoBox.GearItem.m_GearItemData.name = $"GEAR_ShotgunAmmoSingle{ammoType}";
            CustomAddressablesPatch.customAddressablePaths[$"ModdedGearItemData_ShotgunAmmoSingle{ammoType}"] = ammoRefs.AmmoSingle.GearItem.m_GearItemData;
            ammoRefs.AmmoSingle.GearItem.m_AmmoItem = ammoRefs.AmmoSingle.AmmoItem;
        }


        internal void ConfigureShotgunAmmoBox(ConfigurationReferences refs, string ammoType)
        {
            MelonLogger.Msg($"ConfigureShotgunAmmoBox{ammoType}");
            var ammoRefs = refs.Shotgun.AmmoTypes[ammoType];
            ammoRefs.AmmoBox.AmmoItem.m_AmmoForGunType = (GunType)((int)(GunType.Camera) + 1);
            ammoRefs.AmmoBox.AmmoItem.m_LoadedHudIconSpriteName = "";
            ammoRefs.AmmoBox.AmmoItem.m_EmptyHudIconSpriteName = "";
            ammoRefs.AmmoBox.GearItem.m_GearItemData.name = $"GEAR_ShotgunAmmoBox{ammoType}";
            CustomAddressablesPatch.customAddressablePaths[$"ModdedGearItemData_ShotgunAmmoBox{ammoType}"] = ammoRefs.AmmoBox.GearItem.m_GearItemData;
            ammoRefs.AmmoBox.GearItem.m_AmmoItem = ammoRefs.AmmoBox.AmmoItem;

            CustomAmmoHUD.HUDPreset preset = new CustomAmmoHUD.HUDPreset();
            preset.LoadedSprite = Addressables.LoadAssetAsync<Sprite>($"ico_Shotgun{ammoType}HUD").WaitForCompletion();
            preset.UnloadedSprite = Addressables.LoadAssetAsync<Sprite>($"ico_ShotgunEmptyHUD").WaitForCompletion();
            preset.IconHeight = 30f;
            preset.IconWidth = 10;
            preset.IconSpacing = 10f;
            preset.IconsPerRow = 2;
            CustomAmmoHUD.Current.TryRegisterHUDPreset("GEAR_Shotgun", $"GEAR_ShotgunAmmoBox{ammoType}", preset);
        }


        internal void ConfigureShotgunAmmoCasing(ConfigurationReferences refs, string ammoType)
        {
            MelonLogger.Msg($"ConfigureShotgunAmmoCasing{ammoType}");
            // Casing configuration if needed in future
        }


        internal void ConfigureShotgunGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunGearItem");
            GearItem gearItem = refs.Shotgun.Gun.GearItem;
            gearItem.m_StruggleBonus = refs.Shotgun.Gun.StruggleBonus;
            gearItem.m_FirstPersonItem = refs.Shotgun.Gun.FirstPersonItem;
            gearItem.m_GunItem = refs.Shotgun.Gun.GunItem;

            //move to get?
            gearItem.m_Inspect = refs.Shotgun.Gun.Prefab.GetComponent<Inspect>();

            //need to configure?
            // degrate on use?
            // GearItem.m_BodyHarvestPanel.m_PanelReference 
            // GearItem.m_IceFishingPanel.m_PanelReference
            // GearItem.m_InventoryPanel.m_PanelReference 

            //need to do anything with?
            // m_GearItemData

        }


        internal void ConfigureShotgunGunItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunGunItem");
            GunItem gunItem = refs.Shotgun.Gun.GunItem;
            GunItem rifleGunItem = refs.Rifle.Gun.GunItem;

            gunItem.m_AmmoReferences = new Il2CppSystem.Collections.Generic.List<AssetReferenceT<Il2CppTLD.Gear.GearItemData>>();
            foreach (var ammoType in ShotgunAmmoTypes)
            {
                //gunItem.m_AmmoReferences.Add(new AssetReferenceT<Il2CppTLD.Gear.GearItemData>($"ModdedGearItemData_ShotgunAmmoSingle{ammoType}"));
                gunItem.m_AmmoReferences.Add(new AssetReferenceT<Il2CppTLD.Gear.GearItemData>($"ModdedGearItemData_ShotgunAmmoBox{ammoType}"));
            }
            gunItem.m_GearItem = refs.Shotgun.Gun.GearItem;
            gunItem.m_CasingData = refs.Shotgun.Casing.GearItem.m_GearItemData;

            // copied from rifle - expose in modcomponent!
            gunItem.m_AmmoSpriteName = rifleGunItem.m_AmmoSpriteName;
            gunItem.m_CasingAudio = rifleGunItem.m_CasingAudio;
            gunItem.m_DryFireAudio = rifleGunItem.m_DryFireAudio;
            gunItem.m_FireAudio = rifleGunItem.m_FireAudio;
            gunItem.m_ImpactAudio = rifleGunItem.m_ImpactAudio;
            gunItem.m_UncockAudio = rifleGunItem.m_UncockAudio;
            gunItem.m_MisfireTable = rifleGunItem.m_MisfireTable;
            gunItem.m_MisfirePlayerVO = rifleGunItem.m_MisfirePlayerVO;
            gunItem.m_AimingStaminaExhaustedSound = rifleGunItem.m_AimingStaminaExhaustedSound;
            gunItem.m_GunType = (GunType)((int)(GunType.Camera) + 1);


            // preset here - expose in modcomponent!
            gunItem.m_ClipSize = 2;
            gunItem.m_RoundsToReloadPerClip = 2;
            gunItem.m_DamageHP = 125;
            gunItem.m_AccuracyRange = 35;
            gunItem.m_FiringRateSeconds = 0.35f;
            gunItem.m_ReloadCoolDownSeconds = 1.5f;
            gunItem.m_MinDistanceForAimAssist = 15;
            gunItem.m_MuzzleFlash_FlashDelay = 0.1f;
            gunItem.m_MuzzleFlash_SmokeDelay = 0.15f;
            gunItem.m_SwayValueMaxFatigue = 0.75f;
            gunItem.m_SwayIncreasePerSecond = 0.1f;
            gunItem.m_SwayDecreasePerSecond = 0.15f;
        }


        internal void ConfigureShotgunFirstPersonItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFirstPersonItem");
            FirstPersonItem firstPersonItem = refs.Shotgun.Gun.FirstPersonItem;
            FirstPersonItem rifleFirstPersonItem = refs.Rifle.Gun.FirstPersonItem;

            firstPersonItem.m_FirstPersonObjectName = "Shotgun";
            firstPersonItem.m_PlayerStateTransitions = new PlayerStateTransitions();
            firstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<PlayerAnimation.StateFlags>(rifleFirstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions.Length);
            firstPersonItem.m_PlayerStateTransitions.m_ValidTransitions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<PlayerAnimation.StateFlags>(rifleFirstPersonItem.m_PlayerStateTransitions.m_ValidTransitions.Length);
            int i = 0;
            foreach (var transition in rifleFirstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions)
            {
                firstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions[i] = transition;
                i++;
            }
            i = 0;
            foreach (var transition in rifleFirstPersonItem.m_PlayerStateTransitions.m_ValidTransitions)
            {
                firstPersonItem.m_PlayerStateTransitions.m_ValidTransitions[i] = transition;
                i++;
            }
            firstPersonItem.m_ItemData = refs.Shotgun.Floating.FPSItem;
            firstPersonItem.m_WieldAudioEvent = rifleFirstPersonItem.m_WieldAudioEvent;
            firstPersonItem.m_UnwieldAudioEvent = rifleFirstPersonItem.m_UnwieldAudioEvent;
        }


        internal void ConfigureShotgunStruggleBonus(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunStruggleBonus");
            refs.Shotgun.Gun.StruggleBonus.m_StruggleWeaponType = StruggleBonus.StruggleWeaponType.Rifle;
        }


        internal void ConfigureShotgunFPSItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSItem");
            refs.Shotgun.Floating.FPSItem.name = "FPSItem_Shotgun";
            refs.Shotgun.Floating.FPSItem.m_FPSWeaponPrefab = refs.Rifle.Floating.FPSItem.m_FPSWeaponPrefab;
        }

        internal void ConfigureShotgunFPHRoot(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPHRoot");
            refs.Shotgun.WeaponView.Root.name = ShotgunFPSPrefabName;
        }


        internal void ConfigureShotgunFirstPersonWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFirstPersonWeapon");
            refs.Shotgun.FPH.FirstPersonWeapon.m_Renderable = refs.Shotgun.FPH.Root.transform.GetChild(0).gameObject;
            refs.Shotgun.FPH.FirstPersonWeapon.m_Animator = refs.Shotgun.FPH.Animator;
            foreach (Transform transform in refs.Shotgun.FPH.Root.GetComponentsInChildren<Transform>())
            {
                switch (transform.gameObject.name)
                {
                    case "FrontSight": refs.Shotgun.FPH.FirstPersonWeapon.m_FrontSight = transform; break;
                    case "RearSight": refs.Shotgun.FPH.FirstPersonWeapon.m_RearSight = transform; break;
                    case "BulletEmissionPoint": refs.Shotgun.FPH.FirstPersonWeapon.m_BulletEmissionPoint = transform.gameObject; break;
                }
            }            
            /* DEBUG
             * Tested this, for sure the front and rear sight are 100% used for aiming and its very consistent. Enable this to test ingame for yourself!
            if (refs.Shotgun.FPH.FirstPersonWeapon.m_RearSight != null && refs.Shotgun.FPH.FirstPersonWeapon.m_FrontSight != null)
            {
                Primitives.ShootRay(refs.Shotgun.FPH.FirstPersonWeapon.m_RearSight, refs.Shotgun.FPH.FirstPersonWeapon.m_FrontSight, 0.001f, Color.green, 100);
            }
            */
        }


        internal void ConfigureShotgunAnimator(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunAnimator");
            refs.Shotgun.FPH.Animator.avatar = refs.Rifle.FPH.Animator.avatar;
            refs.Shotgun.FPH.Animator.runtimeAnimatorController = refs.Rifle.FPH.Animator.runtimeAnimatorController;
            refs.Shotgun.FPH.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }





        internal void ConfigureShotgunFPSShooter(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSShooter");
            vp_FPSShooter shooter = refs.Shotgun.WeaponView.FPSShooter;
            vp_FPSShooter rifleShooter = refs.Rifle.WeaponView.FPSShooter;
            shooter.m_Weapon = refs.Shotgun.WeaponView.FPSWeapon;
            shooter.MuzzleFlashPrefab = rifleShooter.MuzzleFlashPrefab;
            shooter.ProjectilePrefab = rifleShooter.ProjectilePrefab;
            shooter.ShellPrefab = rifleShooter.ShellPrefab;
            shooter.BulletEmissionLocator = refs.Shotgun.FPH.FirstPersonWeapon.m_BulletEmissionPoint.transform;

            shooter.m_StateManager.m_States.Clear();
            foreach (var state in rifleShooter.m_StateManager.m_States)
            {
                vp_StateInfo newState = new vp_StateInfo(state.TypeName, state.Name);
                vp_ComponentPreset newPreset = new vp_ComponentPreset();
                newPreset.m_ComponentType = state.Preset.m_ComponentType;
                newPreset.m_Fields = new Il2CppSystem.Collections.Generic.List<vp_ComponentPreset.Field>();
                foreach (vp_ComponentPreset.Field field in state.Preset.m_Fields)
                {
                    newPreset.m_Fields.Add(new vp_ComponentPreset.Field(field.m_FieldInfo, field.Args));
                }
                newState.Preset = newPreset;
                newState.TextAsset = state.TextAsset;
                shooter.m_StateManager.m_States.Add(newState);
                if (state.Name == "Default") shooter.m_DefaultState = newState;
            }

            shooter.ProjectileScale = 1.0f;
            shooter.MotionPositionPause = 0f; //not sure what this is for?
            shooter.ShellScale = 1.0f;
            shooter.ShellEjectSpin = 0f;


            shooter.enabled = true;

            // need to find and attach bullet emission point?
        }

        internal void ConfigureShotgunFPSWeapon(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSWeapon");
            refs.Shotgun.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulderPrefab = refs.Shotgun.FPH.Root;
            refs.Shotgun.WeaponView.FPSWeapon.m_StateManager.m_States.Clear();
            foreach (var state in refs.Rifle.WeaponView.FPSWeapon.m_StateManager.m_States)
            {
                vp_StateInfo newState = new vp_StateInfo(state.TypeName, state.Name);
                vp_ComponentPreset newPreset = new vp_ComponentPreset();
                newPreset.m_ComponentType = state.Preset.m_ComponentType;
                newPreset.m_Fields = new Il2CppSystem.Collections.Generic.List<vp_ComponentPreset.Field>();
                foreach (vp_ComponentPreset.Field field in state.Preset.m_Fields)
                {
                    newPreset.m_Fields.Add(new vp_ComponentPreset.Field(field.m_FieldInfo, field.Args));
                }
                newState.Preset = newPreset;
                newState.TextAsset = state.TextAsset;
                refs.Shotgun.WeaponView.FPSWeapon.m_StateManager.m_States.Add(newState);
                if (state.Name == "Default") refs.Shotgun.WeaponView.FPSWeapon.m_DefaultState = newState;
            }

            refs.Shotgun.WeaponView.FPSWeapon.m_GearItem = refs.Shotgun.Gun.Prefab.GetComponent<GearItem>();
            refs.Shotgun.WeaponView.FPSWeapon.m_GunItem = refs.Shotgun.Gun.Prefab.GetComponent<GunItem>();

            refs.Shotgun.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulderPrefab.layer = 23;
            refs.Shotgun.WeaponView.FPSWeapon.m_UseFirstPersonHands = true;

            refs.Shotgun.WeaponView.FPSWeapon.MaybeBuildRuntimeHierarchy();
            refs.Shotgun.WeaponView.FPSWeapon.MaybeSpawnFirstPersonWeapon();



        }

        internal void ConfigureShotgunFPSRoot(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSRoot");
            refs.Shotgun.WeaponView.Root.transform.parent.gameObject.name = $"FPSWeapon_{ShotgunFPSPrefabName}Transform";
        }

        #endregion
    }
}
