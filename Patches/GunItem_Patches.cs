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
    internal class GunItem_Patches
    {
        [HarmonyPatch(typeof(GunItem), nameof(GunItem.Awake))]
        internal static class PatchAwake
        {
            internal static void Prefix(GunItem __instance)
            {

            }
        }
    }
}
