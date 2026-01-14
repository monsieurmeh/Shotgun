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
        public static Main? Instance { get; private set; }
        public static bool FPSItemInitialized { get; set; } = false;

        public override void OnEarlyInitializeMelon()
        {
            LoggerInstance.Msg(EarlyInitialize() ? "EarlyInitialized Successfully!" : "EarlyInitialization Errors!");
        }

        [HarmonyPatch(typeof(Panel_Crafting), nameof(Panel_Crafting.Initialize))]
        internal static class LateInitializePatch
        {
            internal static void Postfix() => Instance.LoggerInstance.Msg(Instance.LateInitialize() ? "LateInitialized Successfully!" : "LateInitialization Errors!");
        }

        [HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.RegisterWeaponItem), new Type[] { typeof(FPSItem) })]
        internal static class SuperLateInitializePatch
        {
            internal static void Postfix()
            {
                try
                {
                    if (Instance == null) return;
                    if (FPSItemInitialized) return;
                    vp_FPSCamera fpsCamera = GameManager.GetVpFPSCamera();
                    if (fpsCamera == null) return;
                    FPSItemInitialized = Instance.ConfigureVPFPS(fpsCamera);
                    Instance.LoggerInstance.Msg(FPSItemInitialized ? "ConfigureVPFPS Successful!" : "ConfigureVPFPS Errors!");
                } catch (Exception e)
                {
                    MelonLogger.Error(e);
                }
            }
        }


        public override void OnDeinitializeMelon()
        {
            LoggerInstance.Msg(Shutdown() ? "Shutdown Successfully!" : "Shutdown Errors!");
        }

        internal bool EarlyInitialize()
        {
            try
            {
                Instance = this; // lazy accessor for lateinitialize patch
                string modsPath = MelonEnvironment.ModsDirectory;
                string basePath = Path.Combine(modsPath, DataFolderPath);
                Directory.CreateDirectory(basePath);

                // check version if available to check if force regen needed
                bool shouldForceRefreshEmbeddedData = Versioning.IsVersionCorrect(basePath);
                
                // seems the .modcomponent needs to live at root like the .dll
                EmbeddedResourceExtractor.RefreshEmbeddedData(modsPath, "Shotgun.modcomponent", shouldForceRefreshEmbeddedData);

                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                return false;
            }
        }

        internal bool LateInitialize()
        {
            try
            {
                GameObject shotgun = Addressables.LoadAssetAsync<GameObject>("GEAR_Shotgun").WaitForCompletion();
                if (shotgun == null) return false;
                if (!ConfigureGearItem(shotgun)) return false;
                if (!ConfigureGunItem(shotgun)) return false;
                if (!ConfigureFirstPersonItem(shotgun)) return false;
                //if (!ConfigureVPFPS(shotgun)) return false; //vp_FPSCammera doesnt appear to be avaialble at the same time as the rest of this stuff?

                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                return false;
            }
        }


        internal bool Shutdown()
        {
            return true;
        }


        internal bool ConfigureVPFPS(vp_FPSCamera fpsCamera)
        {
            GameObject shotgunFPS = new GameObject("Shotgun");

            vp_FPSWeapon shotgunFPSWeapon = shotgunFPS.AddComponent<vp_FPSWeapon>();
            if (shotgunFPSWeapon == null)
            {
                MelonLogger.Error("Null vp_FPSWeapon");
                return false;
            }

            //vp_FPSShooter shotgunFPSShooter = shotgunFPS.AddComponent<vp_FPSShooter>();
            //if (shotgunFPSShooter == null)
            //{
            //    MelonLogger.Error("Null vp_FPSShooter");
            //    return false;
            //}

            FPSItem shotgunFPSItem = ScriptableObject.CreateInstance<FPSItem>();
            if (shotgunFPSItem == null)
            {
                MelonLogger.Error("Null FPSItem");
                return false;
            }
            shotgunFPSItem.name = "Shotgun FPS Item";

            shotgunFPSWeapon.MaybeBuildRuntimeHierarchy();
            shotgunFPSWeapon.MaybeSpawnFirstPersonWeapon();

            if (fpsCamera.m_Weapons == null)
            {
                fpsCamera.m_Weapons = new Il2CppSystem.Collections.Generic.Dictionary<FPSItem, vp_FPSWeapon>();
            }

            if (!fpsCamera.m_Weapons.TryAdd(shotgunFPSItem, shotgunFPSWeapon))
            {
                MelonLogger.Error("Failed to add shotgun to vpFPSCamera.m_weapons dictionary");
                return false;
            }

            return true;
        }

        internal bool ConfigureGearItem(GameObject shotgun)
        {
            GearItem gearItem = shotgun.GetComponent<GearItem>();
            if (gearItem == null)
            {
                MelonLogger.Error("Null GearItem");
                return false;
            }

            if (!ConfigureStruggleBonus(shotgun, gearItem)) return false;

            return true;
        }


        internal bool ConfigureStruggleBonus(GameObject shotgun, GearItem gearItem)
        {
            StruggleBonus struggleBonus = shotgun.GetOrCreateComponent<StruggleBonus>();
            if (struggleBonus == null)
            {
                MelonLogger.Error("Null StruggleBonus");
                return false;
            }

            // Future state: use custom enum
            // struggleBonus.m_StruggleWeaponType = (StruggleBonus.StruggleWeaponType)((int)(StruggleBonus.StruggleWeaponType.SurvivalKnife) + 1);

            // For testing: use rifle
            struggleBonus.m_StruggleWeaponType = StruggleBonus.StruggleWeaponType.Rifle;

            gearItem.m_StruggleBonus = struggleBonus;

            return true;
        }


        internal bool ConfigureGunItem(GameObject shotgun)
        {
            GunItem gunItem = shotgun.GetOrCreateComponent<GunItem>();
            if (gunItem == null)
            {
                MelonLogger.Error("Null GunItem");
                return false;
            }

            // Future state: use custom enum
            // gunItemScript.m_GunType  = (GunType)((int)(GunType.Camera) + 1);

            // for testing: use rifle
            gunItem.m_GunType = GunType.Rifle;

            return true;
        }


        internal bool ConfigureFirstPersonItem(GameObject shotgun)
        {
            FirstPersonItem firstPersonItem = shotgun.GetOrCreateComponent<FirstPersonItem>();
            if (firstPersonItem == null)
            {
                MelonLogger.Error("Null FirstPersonItem");
                return false;
            }

            //configure firstPersonItemScript stuff here

            firstPersonItem.m_FirstPersonObjectName = "Shotgun";
            return true;
        }
    }
}