global using static Shotgun.Utility.Utility;
global using MehToolBox;
global using Shotgun.ToolBox;
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
        internal static readonly GunType GunTypeCustom = (GunType)(Enum.GetValues<GunType>().Cast<int>().Max() + 1);
        internal const string DataFolderPath = "DoubleBarrel";
        internal const string ModComponentFileName = "DoubleBarrel.modcomponent";
        internal const string ShotgunFPSPrefabName = "DoubleBarrel";
        internal const string ShotgunFPHPrefabName = "FPH_DoubleBarrel";
        internal const string ShotgunItemPrefabName = "GEAR_DoubleBarrel";
        internal const string ShotgunItemPrefabCloneName = "GEAR_DoubleBarrel(Clone)";

        internal static float SampleGameTime() => GameManager.m_TimeOfDay.m_WeatherSystem.m_ElapsedHoursAccumulator + GameManager.m_TimeOfDay.m_WeatherSystem.m_ElapsedHours;

        internal static GunItem? GunItemInHands() => GameManager.GetPlayerManagerComponent()?.m_ItemInHands?.m_GunItem;

        internal static GunItem? ShotgunInHands() => GunItemInHands() is GunItem gunItem && gunItem.m_GunType == GunTypeCustom ? gunItem : null;
    }
}
