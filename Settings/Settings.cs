using ModSettings;
using Il2Cpp;
using UnityEngine;


namespace Shotgun
{
    internal class Settings : JsonModSettings
    {
        internal static Settings Instance;

        [Section("General Settings")]
        [Name("Custom Ammo HUD X Offset")]
        [Slider(10, 1920, 192)]
        [Description("X offset of custom ammo HUD, measured in pixels from the right of the screen relative to a screen size of 1920x1080. Scales with your actual resolution.")]
        public int CustomAmmoHUDXOffset = 60;

        [Name("Custom Ammo HUD Y Offset")]
        [Slider(10, 1080, 108)]
        [Description("Y offset of custom ammo HUD, measured in pixels from the bottom of the screen relative to a screen size of 1920x1080. Scales with your actual resolution.")]
        public int CustomAmmoHUDYOffset = 80;

        [Name("Custom Ammo HUD Icon Height")]
        [Slider(30, 120, 19)]
        [Description("Y offset of custom ammo HUD, measured in pixels from the bottom of the screen relative to a screen size of 1920x1080. Scales with your actual resolution.")]
        public int CustomAmmoHUDIconHeight = 50;

        [Name("Custom Ammo HUD Spacing Ratio")]
        [Slider(0.25f, 3.0f, 12)]
        [Description("Spacing-to-Width ratio for Custom Ammo HUD icons. Higher values will result in larger spacing between ammo icons.")]
        public float CustomAmmoHUDIconSpacingRatio = 1.0f;

        [Name("Ammo Switch Hotkey")]
        [Description("Hotkey for swapping ammo types. Vanilla does not handle this, instead using single ammo types for guns and using the reload key for cameras. Default: T")]
        public KeyCode AmmoSwapKeyCode = KeyCode.T;


        [Section("Slug Settings")]
        [Name("Slug Base Damage")]
        [Description("Base damage dealt by slugs within accuracy range. Default: 120")]
        [Slider(50, 150)]
        public int SlugBaseDamage = 120;

        [Name("Slug Accuracy Range")]
        [Description("Distance before damage begins to drop per meter. Default: 30m")]
        [Slider(10, 50)]
        public int SlugAccuracyRange = 30;

        [Name("Slug Damage Falloff Rate")]
        [Description("Damage reduction per meter beyond accuracy range. Default: 5")]
        [Slider(1f, 10f)]
        public float SlugDamageFalloff = 5f;

        [Name("Slug Minimum Damage")]
        [Description("Minimum damage dealt as long as a hit is registered. Default: 20")]
        [Slider(0f, 50f)]
        public float SlugMinimumDamage = 20f;


        [Section("Buckshot Settings")]
        [Name("Buckshot Base Damage")]
        [Description("Base damage dealt by buckshot within accuracy range. Default: 15")]
        [Slider(5, 25)]
        public int BuckshotBaseDamage = 15;

        [Name("Buckshot Accuracy Range")]
        [Description("Distance before damage begins to drop per meter. Default: 25m")]
        [Slider(10, 40)]
        public int BuckshotAccuracyRange = 25;

        [Name("Buckshot Damage Falloff Rate")]
        [Description("Damage reduction per meter beyond accuracy range. Default: 1")]
        [Slider(0.1f, 10f)]
        public float BuckshotDamageFalloff = 1f;

        [Name("Buckshot Minimum Damage")]
        [Description("Minimum damage dealt as long as a hit is registered. Default: 5")]
        [Slider(0f, 15f)]
        public float BuckshotMinimumDamage = 5f;

        [Name("Buckshot Pellet Spread")]
        [Description("Conical spread (total included angle) of pellets. Default: 2 degrees")]
        [Slider(0.2f, 5f)]
        public float BuckshotPelletSpread = 2f;

        [Name("Buckshot Pellet Count")]
        [Description("Pellet Count. Default: 9")]
        [Slider(6f, 12f)]
        public int BuckshotPelletCount = 9;


        [Section("Birdshot Settings")]
        [Name("Birdshot Base Damage")]
        [Description("Base damage dealt by Birdshot within accuracy range. Default: 3")]
        [Slider(1f, 5f)]
        public float BirdshotBaseDamage = 3f;

        [Name("Birdshot Accuracy Range")]
        [Description("Distance before damage begins to drop per meter. Default: 20m")]
        [Slider(10, 30)]
        public int BirdshotAccuracyRange = 20;

        [Name("Birdshot Damage Falloff Rate")]
        [Description("Damage reduction per meter beyond accuracy range. Default: 0.2")]
        [Slider(0.01f, 1f)]
        public float BirdshotDamageFalloff = 0.2f;

        [Name("Birdshot Minimum Damage")]
        [Description("Minimum damage dealt as long as a hit is registered. Default: 1")]
        [Slider(0f, 3f)]
        public float BirdshotMinimumDamage = 1f;

        [Name("Birdshot Pellet Spread")]
        [Description("Conical spread (total included angle) of pellets. Default: 4 degrees")]
        [Slider(0.2f, 8f)]
        public float BirdshotPelletSpread = 4f;

        [Name("Birdshot Pellet Count")]
        [Description("Pellet Count. Default: 30")]
        [Slider(20, 40f)]
        public int BirdshotPelletCount = 30;


        [Section("Recoil Settings")]
        [Name("Recoil Pitch Kick")]
        [Description("Strength of the upward camera jerk when the shotgun fires, at Shotgun skill level 1. Scales down linearly to zero at level 5. Set to 0 to disable. Default: 6")]
        [Slider(0f, 20f)]
        public float RecoilMaxPitchKick = 6f;

        [Name("Recoil Yaw Kick")]
        [Description("Strength of the random sideways camera jerk when the shotgun fires, at Shotgun skill level 1. Scales down to zero at level 5. Set to 0 to disable. Default: 1.5")]
        [Slider(0f, 8f)]
        public float RecoilMaxYawKick = 1.5f;

        [Name("Recoil Rise Time")]
        [Description("Seconds over which the upward kick is applied. Higher = a slower, smoother rise. Default: 0.12")]
        [Slider(0.02f, 0.5f)]
        public float RecoilRiseTime = 0.12f;

        internal Settings() : base(Path.Combine("Shotgun", "Shotgun"))
        {
            Initialize();
        }

        protected void Initialize()
        {
            Instance = this;
            AddToModSettings("Shotgun");
            RefreshGUI();
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            CustomAmmoHUD.Current.UpdateIconPanelPosition();
        }
    }
}