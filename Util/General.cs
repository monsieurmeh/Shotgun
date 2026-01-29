global using static Shotgun.Utility.Utility;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shotgun.Utility
{
    internal static class Utility
    {
        internal const GunType GunTypeCustom = (GunType)((int)(GunType.Camera) + 1);
        internal const string DataFolderPath = "Shotgun";
        internal const string ShotgunFPSPrefabName = "Shotgun";
        internal const string ShotgunItemPrefabName = "GEAR_Shotgun";
        internal const string ShotgunItemPrefabCloneName = "GEAR_Shotgun(Clone)";
    }
}
