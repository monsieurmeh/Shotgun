using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace Shotgun.Patches
{
    internal class vp_FPSPlayer_Patches
    {
        [HarmonyPatch(typeof(vp_FPSPlayer), nameof(vp_FPSPlayer.Start))]
        internal static class SuperLateInitializePatch
        {
            internal static void Postfix() => Main.Instance?.LateInitialize();
        }
    }
}
