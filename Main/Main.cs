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


namespace Shotgun
{
    public class Main : MelonMod
    {
        internal static Main? Instance { get; private set; }

        internal Configuration Configuration { get; private set; } = new();

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
                bool shouldForceRefreshEmbeddedData = Versioning.IsVersionCorrect(basePath);
                EmbeddedResourceExtractor.RefreshEmbeddedData(modsPath, "Shotgun.modcomponent", shouldForceRefreshEmbeddedData);
                MelonLogger.Msg("EarlyInitialize Complete");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"EarlyInitialize Failed: {e}");
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
                LateInitialized = true;
                MelonLogger.Msg("LateInitializing");
                Configuration.ConfigureShotgun();
                MelonLogger.Msg("LateInitialize Complete");
            }
            catch (Exception e)
            {
                MelonLogger.Error($"LateInitialize Failed: {e}");
            }
        }
    }
}