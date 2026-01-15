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
                if (__instance.name.Contains("Shotgun"))
                {
                    __instance.m_FirstPersonWeaponShoulderPrefab = Main.Instance.Configuration.ConfigRefs.Shotgun.FPH.Root;
                }
            }

            internal static void Postfix(vp_FPSWeapon __instance)
            {
                if (__instance.name.Contains("Shotgun"))
                {
                    __instance.ReportNullProperties();
                }
            }
        }
    }
}
