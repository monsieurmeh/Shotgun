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
    internal class vp_FPSShooter_Patches
    {
        [HarmonyPatch(typeof(vp_FPSShooter), nameof(vp_FPSShooter.Awake))]
        internal static class PatchAwake
        {
            internal static void Prefix(vp_FPSShooter __instance)
            {
     
            }

            internal static void Postfix(vp_FPSShooter __instance)
            {

            }
        }
    }
}
