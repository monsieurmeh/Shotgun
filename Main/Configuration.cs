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

namespace Shotgun
{
    internal class Configuration
    {
        internal void PrepareShotgun()
        {
            MelonLogger.Msg("Preparing shotgun...");
            ConfigurationReferences configRefs = new();
            if (!Get(configRefs)) return;
            if (!Instantiate(configRefs)) return;
            if (!Configure(configRefs)) return;
            if (!RegisterShotgun(configRefs)) return;
            ReportDifferences(configRefs);
            MelonLogger.Msg("Ready to roll!");
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
            ScriptComparer.ReportDifferences(refs.Shotgun.Item.GunItem, refs.Rifle.Item.GunItem);
            ScriptComparer.ReportDifferences(refs.Shotgun.Item.GearItem, refs.Rifle.Item.GearItem);
            ScriptComparer.ReportDifferences(refs.Shotgun.Item.FirstPersonItem, refs.Rifle.Item.FirstPersonItem);
            ScriptComparer.ReportDifferences(refs.Shotgun.WeaponView.FPSShooter, refs.Rifle.WeaponView.FPSShooter);
            ScriptComparer.ReportDifferences(refs.Shotgun.WeaponView.FPSWeapon, refs.Rifle.WeaponView.FPSWeapon);
            ScriptComparer.ReportDifferences(refs.Shotgun.FPH.FirstPersonWeapon, refs.Rifle.FPH.FirstPersonWeapon);
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
            if (!GetFPSCamera(refs)) return false;
            if (!GetFPSPlayer(refs)) return false;
            return true;
        }


        internal bool GetShotgun(ConfigurationReferences refs)
        {
            if (!GetShotgunPrefab(refs)) return false;
            if (!GetShotgunGearItem(refs)) return false;
            if (!GetShotgunModComponent(refs)) return false;
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
            return true;
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


        internal bool GetRifleGunItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleGunItem");
            refs.Rifle.Item.GunItem = refs.Rifle.Item.Prefab.GetComponent<GunItem>();
            if (refs.Rifle.Item.GearItem == null)
            {
                MelonLogger.Error("Could not find rifle GunItem from rifle prefab");
                return false;
            }
            return true;
        }

        internal bool GetRifleGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetRifleGearItrm");
            refs.Rifle.Item.GearItem = refs.Rifle.Item.Prefab.GetComponent<GearItem>();
            if (refs.Rifle.Item.GearItem == null)
            {
                MelonLogger.Error("Could not find rifle gearitem from rifle prefab");
                return false;
            }
            return true;
        }


        internal bool GetShotgunPrefab(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetShotgunPrefab");
            refs.Shotgun.Item.Prefab = Addressables.LoadAssetAsync<GameObject>("GEAR_Shotgun").WaitForCompletion();
            if (refs.Shotgun.Item.Prefab == null) MelonLogger.Error("Could not summon GEAR_Shotgun");
            return refs.Shotgun.Item.Prefab != null;
        }


        internal bool GetShotgunGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetShotgunGearItem");
            refs.Shotgun.Item.GearItem = refs.Shotgun.Item.Prefab?.GetComponent<GearItem>();
            if (refs.Shotgun.Item.GearItem == null) MelonLogger.Error("Null Shotgun GearItem");
            return refs.Shotgun.Item.GearItem != null;
        }


        internal bool GetShotgunModComponent(ConfigurationReferences refs)
        {
            MelonLogger.Msg("GetShotgunModComponent");
            refs.Shotgun.Item.ModComponent = refs.Shotgun.Item.Prefab?.GetComponent<ModGenericComponent>();
            if (refs.Shotgun.Item.GearItem == null) MelonLogger.Error("Null Shotgun GearItem");
            return refs.Shotgun.Item.GearItem != null;
        }


        #endregion


        #region Instantiate

        internal bool Instantiate(ConfigurationReferences refs)
        {
            if (!InstantiateTopLevel(refs)) return false;
            if (!InstantiateShotgun(refs)) return false;
            return false;
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
                ConfigureShotgunGearItem(refs);
                ConfigureShotgunGunItem(refs);
                ConfigureShotgunFirstPersonItem(refs);
                ConfigureShotgunStruggleBonus(refs);
                ConfigureShotgunFPSItem(refs);
                ConfigureShotgunFirstPersonWeapon(refs);
                ConfigureShotgunAnimator(refs);
                ConfigureShotgunFPHRoot(refs);
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


        internal void ConfigureShotgunGearItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunGearItem");
            refs.Shotgun.Item.GearItem.m_StruggleBonus = refs.Shotgun.Item.StruggleBonus;
        }


        internal void ConfigureShotgunGunItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunGunItem");
            refs.Shotgun.Item.GunItem.m_GunType = GunType.Rifle;
            refs.Shotgun.Item.GunItem.m_AmmoReferences = refs.Rifle.Item.GunItem.m_AmmoReferences;
            refs.Shotgun.Item.GunItem.m_AmmoSpriteName = refs.Rifle.Item.GunItem.m_AmmoSpriteName;
            refs.Shotgun.Item.GunItem.m_ClipSize = 6;
            refs.Shotgun.Item.GunItem.m_GearItem = refs.Shotgun.Item.GearItem;
        }


        internal void ConfigureShotgunFirstPersonItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFirstPersonItem");
            refs.Shotgun.Item.FirstPersonItem.m_FirstPersonObjectName = "Shotgun";
            refs.Shotgun.Item.FirstPersonItem.m_PlayerStateTransitions = new PlayerStateTransitions();
            refs.Shotgun.Item.FirstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<PlayerAnimation.StateFlags>(refs.Rifle.Item.FirstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions.Length);
            refs.Shotgun.Item.FirstPersonItem.m_PlayerStateTransitions.m_ValidTransitions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<PlayerAnimation.StateFlags>(refs.Rifle.Item.FirstPersonItem.m_PlayerStateTransitions.m_ValidTransitions.Length);
            int i = 0;
            foreach (var transition in refs.Rifle.Item.FirstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions)
            {
                refs.Shotgun.Item.FirstPersonItem.m_PlayerStateTransitions.m_InvalidTransitions[i] = transition;
                i++;
            }
            i = 0;
            foreach (var transition in refs.Rifle.Item.FirstPersonItem.m_PlayerStateTransitions.m_ValidTransitions)
            {
                refs.Shotgun.Item.FirstPersonItem.m_PlayerStateTransitions.m_ValidTransitions[i] = transition;
                i++;
            }
            refs.Shotgun.Item.FirstPersonItem.m_ItemData = refs.Shotgun.Floating.FPSItem;
        }

        internal void ConfigureShotgunStruggleBonus(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunStruggleBonus");
            refs.Shotgun.Item.StruggleBonus.m_StruggleWeaponType = StruggleBonus.StruggleWeaponType.Rifle;
        }

        internal void ConfigureShotgunFPSItem(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSItem");
            refs.Shotgun.Floating.FPSItem.name = "FPSItem_Shotgun";
            refs.Shotgun.Floating.FPSItem.m_FPSWeaponPrefab = refs.Rifle.Floating.FPSItem.m_FPSWeaponPrefab;
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
        }

        internal void ConfigureShotgunAnimator(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunAnimator");
            refs.Shotgun.FPH.Animator.avatar = refs.Rifle.FPH.Animator.avatar;
            refs.Shotgun.FPH.Animator.runtimeAnimatorController = refs.Rifle.FPH.Animator.runtimeAnimatorController;
            refs.Shotgun.FPH.Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        internal void ConfigureShotgunFPHRoot(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPHRoot");
            refs.Shotgun.WeaponView.Root.name = ShotgunFPSPrefabName;
        }

        internal void ConfigureShotgunFPSShooter(ConfigurationReferences refs)
        {
            MelonLogger.Msg("ConfigureShotgunFPSShooter");
            refs.Shotgun.WeaponView.FPSShooter.m_Weapon = refs.Shotgun.WeaponView.FPSWeapon;
            refs.Shotgun.WeaponView.FPSShooter.MuzzleFlashPrefab = refs.Rifle.WeaponView.FPSShooter.MuzzleFlashPrefab;
            refs.Shotgun.WeaponView.FPSShooter.ProjectilePrefab = refs.Rifle.WeaponView.FPSShooter.ProjectilePrefab;
            refs.Shotgun.WeaponView.FPSShooter.ShellPrefab = refs.Rifle.WeaponView.FPSShooter.ShellPrefab;

            refs.Shotgun.WeaponView.FPSShooter.m_StateManager.m_States.Clear();
            foreach (var state in refs.Rifle.WeaponView.FPSShooter.m_StateManager.m_States)
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
                refs.Shotgun.WeaponView.FPSShooter.m_StateManager.m_States.Add(newState);
                if (state.Name == "Default") refs.Shotgun.WeaponView.FPSShooter.m_DefaultState = newState;
            }

            refs.Shotgun.WeaponView.FPSShooter.enabled = true;
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

            refs.Shotgun.WeaponView.FPSWeapon.m_GearItem = refs.Shotgun.Item.Prefab.GetComponent<GearItem>();
            refs.Shotgun.WeaponView.FPSWeapon.m_GunItem = refs.Shotgun.Item.Prefab.GetComponent<GunItem>();

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
