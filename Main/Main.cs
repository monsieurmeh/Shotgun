using MelonLoader;
using UnityEngine;
using System.Reflection;
using Il2Cpp;
using MelonLoader.Utils;
using Shotgun.Util;
using UnityEngine.AddressableAssets;


namespace Shotgun
{
	public class Main : MelonMod
    {
        public override void OnEarlyInitializeMelon()
        {
            LoggerInstance.Msg(EarlyInitialize() ? "EarlyInitialized Successfully!" : "EarlyInitialization Errors!");
        }


        public override void OnLateInitializeMelon()
        {
            LoggerInstance.Msg(LateInitialize() ? "LateInitialized Successfully!" : "LateInitialization Errors!"); 
        }


        public override void OnDeinitializeMelon()
        {
            LoggerInstance.Msg(Shutdown() ? "Shutdown Successfully!" : "Shutdown Errors!");
        }

        protected bool EarlyInitialize()
        {
            try
            {
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

        protected bool LateInitialize()
        {
            try
            {
                GameObject shotgunGO = Addressables.LoadAssetAsync<GameObject>("GEAR_Shotgun").WaitForCompletion();
                if (shotgunGO == null) return false;

                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                return false;
            }
        }


        protected bool Shutdown()
        {
            return true;
        }
    }
}