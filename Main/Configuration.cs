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

namespace Shotgun
{ 
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

            if (!GetRifleVPFPSRoot(ConfigRefs)) return;
            if (!GetRifleVPFPSWeapon(ConfigRefs)) return;
            if (!GetRifleVPFPShooter(ConfigRefs)) return;
            if (!GetRifleFPH(ConfigRefs)) return;
            if (!GetRifleFirstPersonWeapon(ConfigRefs)) return;
            if (!GetRifleAnimator(ConfigRefs)) return;

            if (!InstantiateShotgunFPH(ConfigRefs)) return;
            if (!InstantiateShotgunFirstPersonWeapon(ConfigRefs)) return;
            if (!InstantiateShotgunAnimator(ConfigRefs)) return;

            if (!InstantiateShotgunWeaponGroup(ConfigRefs)) return;
            if (!InstantiateShotgunVPFPSWeapon(ConfigRefs)) return;
            if (!InstantiateShotgunVPFPSShooter(ConfigRefs)) return;

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

                refs.Shotgun.FPH.FirstPersonWeapon.m_Renderable = refs.Shotgun.FPH.Root.transform.GetChild(0).gameObject;
                //refs.Shotgun.FPH.FirstPersonWeapon.m_BulletEmissionPoint = ???; gameobject ref
                refs.Shotgun.FPH.FirstPersonWeapon.m_Animator = refs.Shotgun.FPH.Animator;
                //refs.Shotgun.FPH.FirstPersonWeapon.m_FrontSight = ???; transform ref
                //refs.Shotgun.FPH.FirstPersonWeapon.m_RearSight = ???; transform ref

                refs.Shotgun.FPH.Animator.avatar = refs.Rifle.FPH.Animator.avatar;
                refs.Shotgun.FPH.Animator.runtimeAnimatorController = refs.Rifle.FPH.Animator.runtimeAnimatorController;
                refs.Shotgun.FPH.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                refs.Shotgun.WeaponView.Root.name = ShotgunFPSPrefabName;

                refs.Shotgun.WeaponView.FPSShooter.enabled = true;
                refs.Shotgun.WeaponView.FPSShooter.m_Weapon = refs.Shotgun.WeaponView.FPSWeapon;

                // CRITICAL NOTE: vp_FPSWeapon appears to suffer a null reference exception unless there is at least one FirstPerson prefab attached... 
                // Following line was removed from ConfigureDetails and moved to a patch
                // refs.Shotgun.WeaponView.FPSWeapon.m_FirstPersonWeaponShoulderPrefab = refs.Shotgun.FPH.Root;

                refs.Shotgun.WeaponView.FPSWeapon.m_GearItem = refs.Shotgun.Item.Prefab.GetComponent<GearItem>();
                refs.Shotgun.WeaponView.FPSWeapon.m_GunItem = refs.Shotgun.Item.Prefab.GetComponent<GunItem>();

                refs.Shotgun.WeaponView.FPSWeapon.MaybeBuildRuntimeHierarchy();
                refs.Shotgun.WeaponView.FPSWeapon.MaybeSpawnFirstPersonWeapon();

                refs.Shotgun.WeaponView.Root.transform.parent.gameObject.name = $"FPSWeapon_{ShotgunFPSPrefabName}Transform";

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
