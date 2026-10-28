using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gameplay.Tunable;
using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Shotgun
{
    internal static class GroupSpawner
    {
        internal sealed record Member(string Scene, string Prefab, Vector3 Position, Quaternion Rotation);

        internal sealed class Group
        {
            public string Name = "";
            public string Scene;
            public Dictionary<string, int> Picks = new(StringComparer.OrdinalIgnoreCase);
            public List<Member> Members = new();
        }

        private static readonly Regex sGroup = new(@"^group\s*=\s*(\w+)((?:\s+\w+\s*=\s*\w+)*)$", RegexOptions.IgnoreCase);
        private static readonly Regex sKeys = new(@"(\w+)\s*=\s*(\w+)");
        private static readonly Regex sItem = new(@"^item\s*=\s*(\w+)(?:\s+scene\s*=\s*(\w+))?\s+p\s*=\s*([-\d.,\s]+?)\s+r\s*=\s*([-\d.,\s]+?)$", RegexOptions.IgnoreCase);

        private static List<Group> sGroups;

        internal static void SetGroups(IEnumerable<Group> groups) => sGroups = groups.Where(g => g.Members.Count > 0).ToList();

        private static void EnsureLoaded()
        {
            if (sGroups != null) return;
            using Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Shotgun.groupspawns.txt");
            if (s == null) { SetGroups(Array.Empty<Group>()); return; }
            using StreamReader reader = new(s);
            SetGroups(Parse(reader.ReadToEnd()));
        }

        internal static List<Group> Parse(string text)
        {
            Dictionary<string, Group> groups = new(StringComparer.OrdinalIgnoreCase);
            Group current = null;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                Match m;
                if ((m = sGroup.Match(line)).Success)
                {
                    string name = m.Groups[1].Value;
                    if (!groups.TryGetValue(name, out current)) groups[name] = current = new Group { Name = name };
                    foreach (Match key in sKeys.Matches(m.Groups[2].Value))
                    {
                        if (key.Groups[1].Value.Equals("scene", StringComparison.OrdinalIgnoreCase)) current.Scene = key.Groups[2].Value;
                        else if (int.TryParse(key.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)) current.Picks[key.Groups[1].Value] = n;
                    }
                    continue;
                }
                if (current != null && (m = sItem.Match(line)).Success)
                {
                    string scene = m.Groups[2].Success ? m.Groups[2].Value : current.Scene;
                    if (scene == null) continue;
                    current.Members.Add(new Member(scene, m.Groups[1].Value, Vec(m.Groups[3].Value), Quaternion.Euler(Vec(m.Groups[4].Value))));
                }
            }
            return groups.Values.ToList();
        }

        private static Vector3 Vec(string s)
        {
            float[] v = s.Split(',').Select(x => float.Parse(x.Trim(), CultureInfo.InvariantCulture)).ToArray();
            return new Vector3(v[0], v[1], v[2]);
        }

        internal static string CurrentLevel()
        {
            try
            {
                if (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.Custom)
                {
                    CustomExperienceMode custom = GameManager.GetCustomMode();
                    if (custom != null) return custom.m_BaseResourceAvailability.ToString();
                }
                ExperienceMode mode = ExperienceModeManager.Instance?.GetCurrentExperienceMode();
                if (mode != null) return mode.m_BaseResourceAvailability.ToString();
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"GroupSpawner: resource level unavailable ({e.Message}), using High");
            }
            return CustomTunableLMHV.High.ToString();
        }

        internal static int PicksFor(Group g, string level) =>
            g.Picks.TryGetValue(level, out int n) || g.Picks.TryGetValue("default", out n) ? n : 0;

        internal static string GameKey()
        {
            int seed = GameManager.m_SceneTransitionData != null ? GameManager.m_SceneTransitionData.m_GameRandomSeed : 0;
            return $"{seed}:{SaveGameSystem.m_CurrentGameId}";
        }

        internal static System.Random RngFor(string groupName)
        {
            uint h = 2166136261;
            foreach (char c in GameKey() + ":" + groupName.ToLowerInvariant()) h = (h ^ c) * 16777619;
            return new System.Random((int)h);
        }

        internal static List<Member> Pick(Group g, string level, System.Random rng)
        {
            List<Member> pool = new(g.Members);
            List<Member> picked = new();
            int count = Math.Min(PicksFor(g, level), pool.Count);
            for (int i = 0; i < count; i++)
            {
                int index = rng.Next(pool.Count);
                picked.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return picked;
        }

        internal static List<Member> PickedForThisGame(Group g) => Pick(g, CurrentLevel(), RngFor(g.Name));

        private static void SpawnScene(string scene)
        {
            EnsureLoaded();
            foreach (Group g in sGroups)
            {
                if (!g.Members.Any(m => string.Equals(m.Scene, scene, StringComparison.OrdinalIgnoreCase))) continue;
                foreach (Member m in PickedForThisGame(g).Where(m => string.Equals(m.Scene, scene, StringComparison.OrdinalIgnoreCase)))
                {
                    GameObject prefab = Addressables.LoadAssetAsync<GameObject>(m.Prefab).WaitForCompletion();
                    if (prefab == null) { MelonLogger.Warning($"GroupSpawner: no prefab {m.Prefab}"); continue; }
                    GameObject gear = UnityEngine.Object.Instantiate(prefab, m.Position, m.Rotation);
                    gear.name = prefab.name;
                    DisableObjectForXPMode xp = gear.GetComponent<DisableObjectForXPMode>();
                    if (xp != null) UnityEngine.Object.Destroy(xp);
                }
            }
        }

        [HarmonyPatch(typeof(QualitySettingsManager), nameof(QualitySettingsManager.ApplyCurrentQualitySettings))]
        private static class SceneReadyPatch
        {
            private static void Prefix()
            {
                string scene = GameManager.m_ActiveScene;
                if (scene is null or "" or "MainMenu" or "Boot" or "Empty") return;
                try { SpawnScene(scene); }
                catch (Exception e) { MelonLogger.Error($"GroupSpawner: {scene}: {e}"); }
            }
        }
    }
}
