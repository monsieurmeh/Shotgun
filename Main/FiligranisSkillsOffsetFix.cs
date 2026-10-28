using System.Reflection;
using HarmonyLib;
using Il2Cpp;

namespace Shotgun
{
    [HarmonyPatch(typeof(SkillsManager), nameof(SkillsManager.Awake))]
    internal static class FiligranisSkillsOffsetFix
    {
        private const string FiligranisSkillsType = "FiligranisSkills.FiligranisSkills, FiligranisSkills";

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(SkillsManager __instance)
        {
            Type skills = AccessTools.TypeByName(FiligranisSkillsType);
            FieldInfo offsetField = skills == null ? null : AccessTools.Field(skills, "customSkillOffset");
            if (offsetField == null || AccessTools.Field(skills, "skills")?.GetValue(null) is not Dictionary<string, int> indices) return;
            if (!indices.TryGetValue(ShotgunSkill.SkillName, out int index)) return;
            int position = IndexOfSkill(__instance, "Skill_" + ShotgunSkill.SkillName);
            if (position < 0) return;

            int offset = position - index;
            int current = (int)offsetField.GetValue(null);
            if (current == offset) return;
            offsetField.SetValue(null, offset);
            Main.Instance?.LoggerInstance.Msg($"FiligranisSkills custom skill offset {current} -> {offset} (TLD 2.58 compatibility)");
        }

        private static int IndexOfSkill(SkillsManager skillsManager, string goName)
        {
            for (int i = 0; i < skillsManager.m_Skills.Count; i++)
            {
                Skill sk = skillsManager.m_Skills[i];
                if (sk != null && sk.name == goName) return i;
            }
            return -1;
        }
    }
}
