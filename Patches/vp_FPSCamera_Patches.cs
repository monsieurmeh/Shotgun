using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace Shotgun.Patches
{
    internal class vp_FPSCamera_Patches
    {
        [HarmonyPatch(typeof(vp_FPSCamera), nameof(vp_FPSCamera.LoadAllFPSWeapons))]
        internal static class LateInitializationPatch
        {
            internal static void Postfix() => Main.Instance?.LateInitialize();
        }
    }
}
