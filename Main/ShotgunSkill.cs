using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Shotgun
{
    internal static class ShotgunSkill
    {
        internal const string SkillName = "Shotgun";
        internal static readonly float[] TierMultipliers = { 1.0f, 1.15f, 1.30f, 1.50f, 1.75f };
        internal const float BaseReloadCooldown = 2.625f;
        internal const float BaseSwayMaxFatigue = 1.3125f;
        internal const float BaseSwayIncrease = 0.175f;
        internal const float BaseSwayDecrease = 0.0857f;

        #region FiligranisSkills Compat

        private static Type mFSkillType;

        internal delegate (int points, int tier) GetPointsAndTiersDelegate(string skillName);
        internal static GetPointsAndTiersDelegate GetPointsAndTiers { get; private set; }

        internal delegate void IncreaseSkillPointsDelegate(string skillName, int points);
        internal static IncreaseSkillPointsDelegate IncreaseSkillPoints { get; private set; }

        internal static bool IsAvailable => mFSkillType != null && GetPointsAndTiers != null;

        internal static void Init()
        {
            Type fskillType = AccessTools.TypeByName("FiligranisSkills.FiligranisSkills, FiligranisSkills");
            if (fskillType == null)
            {
                MelonLogger.Warning("FiligranisSkills not found, Shotgun skill will not be available");
                return;
            }

            mFSkillType = fskillType;

            MethodInfo methodInfo = AccessTools.FirstMethod(mFSkillType, mi => mi.Name == nameof(GetPointsAndTiers));
            if (methodInfo == null) MelonLogger.Error($"GetPointsAndTiers method not found in {mFSkillType.Name}");
            else GetPointsAndTiers = methodInfo.CreateDelegate<GetPointsAndTiersDelegate>();

            methodInfo = AccessTools.FirstMethod(mFSkillType, mi => mi.Name == nameof(IncreaseSkillPoints));
            if (methodInfo == null) MelonLogger.Error($"IncreaseSkillPoints method not found in {mFSkillType.Name}");
            else IncreaseSkillPoints = methodInfo.CreateDelegate<IncreaseSkillPointsDelegate>();

            TryRegisterSkill();
        }

        private static void TryRegisterSkill()
        {
            if (mFSkillType == null) return;

            MethodInfo registerMethod = AccessTools.Method(mFSkillType, "RegisterSkill");
            if (registerMethod == null)
            {
                MelonLogger.Error("RegisterSkill method not found");
                return;
            }

            Type skillDefType = AccessTools.TypeByName("FiligranisSkills.SkillDefinition, FiligranisSkills");
            ConstructorInfo constructor = AccessTools.Constructor(skillDefType, new Type[]
            {
                typeof(string),
                typeof(LocalizedString),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(LocalizedString[]),
                typeof(LocalizedString[]),
                typeof(int[]),
                typeof(UnityEngine.Texture2D)
            });

            object skillDef = constructor.Invoke(new object[]
            {
                SkillName,
                new LocalizedString { m_LocalizationID = "Shotgun" },
                "ico_skill_large_Rifle",
                "ico_skill_large_Rifle",
                "ico_skill_large_Rifle",
                new LocalizedString[]
                {
                    new LocalizedString { m_LocalizationID = "Novice: You've never handled a shotgun before. The recoil catches you off guard, your aim wavers constantly, and reloading is painfully slow." },
                    new LocalizedString { m_LocalizationID = "Beginner: You're getting used to the kick. Your hands are steadier and you fumble less with shells, but there's still a long way to go." },
                    new LocalizedString { m_LocalizationID = "Intermediate: The shotgun feels more natural now. You brace properly against the recoil and can keep your sights on target longer." },
                    new LocalizedString { m_LocalizationID = "Advanced: Confidence with the shotgun is showing. You handle recoil smoothly, reload with practiced efficiency, and hold steady aim under pressure." },
                    new LocalizedString { m_LocalizationID = "Expert: The shotgun is an extension of yourself. Recoil is absorbed instinctively, reloading is fluid, and you can hold aim with remarkable control." }
                },
                new LocalizedString[]
                {
                    new LocalizedString { m_LocalizationID = "Base Shotgun Skill." },
                    new LocalizedString { m_LocalizationID = "Recoil, sway, and reload improved by 15%." },
                    new LocalizedString { m_LocalizationID = "Recoil, sway, and reload improved by 30%." },
                    new LocalizedString { m_LocalizationID = "Recoil, sway, and reload improved by 50%." },
                    new LocalizedString { m_LocalizationID = "Recoil, sway, and reload improved by 75%." }
                },
                new int[] { 0, 25, 50, 75, 100 },
                null
            });

            registerMethod.Invoke(null, new[] { skillDef });
            MelonLogger.Msg("Shotgun skill registered with FiligranisSkills");
        }

        #endregion

        #region Stat Calculation

        internal static int GetCurrentTier()
        {
            if (!IsAvailable) return 4; 
            (int points, int tier) skill = GetPointsAndTiers(SkillName);
            return skill.tier;
        }

        internal static float GetTierMultiplier()
        {
            int tier = GetCurrentTier();
            return TierMultipliers[Math.Clamp(tier, 0, TierMultipliers.Length - 1)];
        }

        internal static float GetBadStat(float baseValue, float multiplier) => baseValue / multiplier;
        internal static float GetGoodStat(float baseValue, float multiplier) => baseValue * multiplier;
        internal static float GetReloadCooldown() => GetBadStat(BaseReloadCooldown, GetTierMultiplier());
        internal static float GetSwayMaxFatigue() => GetBadStat(BaseSwayMaxFatigue, GetTierMultiplier());
        internal static float GetSwayIncrease() => GetBadStat(BaseSwayIncrease, GetTierMultiplier());
        internal static float GetSwayDecrease() => GetGoodStat(BaseSwayDecrease, GetTierMultiplier());
        internal static float GetRecoilMultiplier() => TierMultipliers[4] / GetTierMultiplier();

        internal static void ApplyToGunItem(GunItem gunItem)
        {
            gunItem.m_ReloadCoolDownSeconds = GetReloadCooldown();
            gunItem.m_SwayValueMaxFatigue = GetSwayMaxFatigue();
            gunItem.m_SwayIncreasePerSecond = GetSwayIncrease();
            gunItem.m_SwayDecreasePerSecond = GetSwayDecrease();
        }

        internal static void MaybeIncreaseSkillOnFire()
        {
            if (IncreaseSkillPoints == null) return;

            // ~33% chance to gain a point per shot
            double rand = new System.Random().NextDouble();
            if (rand < 0.33)
            {
                IncreaseSkillPoints(SkillName, 1);
            }
        }

        #endregion
    }
}
