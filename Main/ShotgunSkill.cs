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
                    new LocalizedString { m_LocalizationID = "Novice" },
                    new LocalizedString { m_LocalizationID = "Beginner" },
                    new LocalizedString { m_LocalizationID = "Intermediate" },
                    new LocalizedString { m_LocalizationID = "Advanced" },
                    new LocalizedString { m_LocalizationID = "Expert" }
                },
                new LocalizedString[]
                {
                    new LocalizedString { m_LocalizationID = "Novice" },
                    new LocalizedString { m_LocalizationID = "Beginner" },
                    new LocalizedString { m_LocalizationID = "Intermediate" },
                    new LocalizedString { m_LocalizationID = "Advanced" },
                    new LocalizedString { m_LocalizationID = "Expert" }
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
            if (!IsAvailable) return 3; 
            (int points, int tier) skill = GetPointsAndTiers(SkillName);
            return skill.tier;
        }


        internal static void ApplyToGunItem(GunItem gunItem)
        {

        }

        internal static long LastSkillUpTime;

        internal static void MaybeIncreaseSkill()
        {
            if (DateTime.Now.Millisecond - LastSkillUpTime >= 1000)
            {
                LastSkillUpTime = DateTime.Now.Millisecond;
                IncreaseSkillPoints.Invoke("Shotgun", 1);
            }
        }

        #endregion

        #region Animation Skill Gates

        internal static int DefaultAnimationTier = 3;
        internal static int AnimationTier => IsAvailable ? GetCurrentTier() : DefaultAnimationTier;
        internal static int CatchLeftMinTier = 3;
        internal static int RemoveBothFastMinTier = 3;
        internal static int InsertBothMediumMinTier = 2;
        internal static int InsertBothFastMinTier = 4;
        internal static int CloseToAimMinTier = 3;
        internal static int RecoilMedMinTier = 2; 
        internal static int RecoilLowMinTier = 4;

        internal static bool CanCatchLeftOnAmmoSwitch => AnimationTier >= CatchLeftMinTier;
        internal static bool CanCloseBarrelToAim => AnimationTier >= CloseToAimMinTier;
        internal static int ManualRemoveSpeed  => AnimationTier >= RemoveBothFastMinTier ? 2 : 1;
        internal static int InsertSpeed => AnimationTier >= InsertBothFastMinTier ? 2 : (AnimationTier >= InsertBothMediumMinTier ? 1 : 0);
        internal static int RecoilHandlingLevel => AnimationTier >= RecoilLowMinTier ? 2 : (AnimationTier >= RecoilMedMinTier ? 1 : 0);

        internal static void ApplyAnimatorParams(Animator animator)
        {
            if (animator == null) return;
            animator.SetBool("CanCatchLeftOnAmmoSwitch", CanCatchLeftOnAmmoSwitch);
            animator.SetBool("CanCloseBarrelToAim", CanCloseBarrelToAim);
            animator.SetInteger("ManualRemoveSpeed", ManualRemoveSpeed);
            animator.SetInteger("InsertSpeed", InsertSpeed);
            animator.SetInteger("RecoilHandlingLevel", RecoilHandlingLevel);
        }

        #endregion
    }
}
