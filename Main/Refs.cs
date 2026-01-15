using Il2Cpp;
using Il2CppTLD.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shotgun
{
    internal class ConfigurationReferences
    {
        internal vp_FPSPlayer? FPSPlayer { get; set; }
        internal vp_FPSCamera? FPSCamera { get; set; }
        internal GunRefs? Shotgun { get; set; } = new();
        internal GunRefs? Rifle { get; set; } = new();
    }


    internal class GunRefs
    {
        internal ItemRefs Item { get; set; } = new();
        internal WeaponViewRefs WeaponView { get; set; } = new();
        internal FPHRefs FPH { get; set; } = new();
        internal FloatingRefs Floating { get; set; } = new();
    }


    internal class ItemRefs
    {
        internal GameObject? Prefab { get; set; }
        internal GearItem? GearItem { get; set; }
        internal GunItem? GunItem { get; set; }
        internal FirstPersonItem? FirstPersonItem { get; set; }
        internal StruggleBonus? StruggleBonus { get; set; }
    }


    internal class FPHRefs
    {
        internal GameObject? Root { get; set; }
        internal FirstPersonWeapon? FirstPersonWeapon { get; set; }
        internal Animator? Animator { get; set; }
    }


    internal class WeaponViewRefs
    {
        internal GameObject? Root { get; set; }
        internal vp_FPSWeapon? FPSWeapon { get; set; }
        internal vp_FPSShooter? FPSShooter { get; set; }
    }


    internal class FloatingRefs
    {
        internal FPSItem? FPSItem { get; set; }
    }
    
}
