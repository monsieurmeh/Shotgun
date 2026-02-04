using MelonLoader;
using UnityEngine;
using System.Reflection;
using Il2Cpp;
using MelonLoader.Utils;
using Shotgun.Utility;
using UnityEngine.AddressableAssets;
using Il2CppVLB;
using HarmonyLib;
using Il2CppTLD.Gameplay;
using static Il2Cpp.Utils;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets.ResourceLocators;
using Il2CppAtlas.Unity;


namespace Shotgun
{
    public class Main : MelonMod
    {
        internal static Main? Instance { get; private set; }

        internal Configuration Configuration { get; private set; } = new();
        internal Settings Settings { get; private set; } = new();
        internal bool LateInitialized { get; private set; } = false;

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
                bool shouldForceRefreshEmbeddedData = Versioning.IsVersionCorrect(basePath, "0.9.5");
                EmbeddedResourceExtractor.RefreshEmbeddedData(modsPath, "Shotgun.modcomponent", shouldForceRefreshEmbeddedData);
                MelonLogger.Msg("EarlyInitialize Complete");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"EarlyInitialize Failed: {e}");
            }

        }

        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ModdedResourceLocator>(new RegisterTypeOptions()
            {
                Interfaces = new Il2CppInterfaceCollection(new[] { typeof(IResourceLocator) })
            });
        }

        public bool Register = false;

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (!Register)
            {
                Addressables.AddResourceLocator(new ModdedResourceLocator().Cast<IResourceLocator>());
                Register = true;
            }
        }


        [HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.LoadAllFPSWeapons))]
        internal static class LateInitializationPatch
        {
            internal static void Postfix() => Main.Instance?.LateInitialize();
        }


        internal void LateInitialize()
        {
            try
            {
                string sceneName = GameManager.m_ActiveScene;
                if (sceneName.Contains("MainMenu") || sceneName.Contains("Boot"))
                {
                    MelonLogger.Msg("Delaying late initialize until non-boot/main scene load");
                    LateInitialized = false;
                    return;
                }
                if (LateInitialized)
                {
                    MelonLogger.Msg("LateInitialization already performed, skipping until next boot/mainmenu scene load");
                    return;
                }
                MelonLogger.Msg("LateInitializing");
                LateInitialized = Configuration.PrepareShotgun();
                MelonLogger.Msg(LateInitialized ? "LateInitialize Complete" : "LateInitialize Failed!");
                MelonLogger.Msg("");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"LateInitialize Failed: {e}");
            }
        }
    }
}