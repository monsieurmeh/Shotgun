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

        }
    }
}
