using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using Shotgun.Utility;

namespace Shotgun.Patches
{
    internal class vp_FPSWeapon_Patches
    {
        [HarmonyPatch(typeof(vp_FPSWeapon), nameof(vp_FPSWeapon.Awake))]
        internal static class PatchAwake
        {
            internal static void Prefix(vp_FPSWeapon __instance)
            {
                MelonLogger.Msg($"vp_FPSWeapon.Awake prefix on {__instance.name}");
                if (__instance.name.Contains("Shotgun"))
                {
                    MelonLogger.Msg($"vp_FPSWeapon.Awake triggered on {__instance.name}");
                    __instance.ReportNullProperties();
                    __instance.m_FirstPersonWeaponShoulderPrefab = Main.Instance.Configuration.ConfigRefs.Shotgun.FPH.Root;
                    __instance.ReportNullProperties();
                }
            }
        }
    }
}
