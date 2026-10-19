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
        internal sealed record Member(string Scene, string Prefab, Vector3 Position, Quaternion Rotation, int Weight, string Set);

        internal sealed class Group
        {
            public string Name = "";
            public Dictionary<string, int> Picks = new(StringComparer.OrdinalIgnoreCase);
            public List<Member> Members = new();
        }

        private static readonly Regex sScene = new(@"^scene\s*=\s*(\S+)$", RegexOptions.IgnoreCase);
        private static readonly Regex sGroup = new(@"^group\s*=\s*(\w+)((?:\s+\w+\s*=\s*\d+)*)$", RegexOptions.IgnoreCase);
        private static readonly Regex sPicks = new(@"(\w+)\s*=\s*(\d+)");
        private static readonly Regex sItem = new(@"^item\s*=\s*(\w+)\s+p\s*=\s*([-\d.,\s]+?)\s+r\s*=\s*([-\d.,\s]+?)(?:\s+c\s*=\s*(\d+))?(?:\s+s\s*=\s*(\w+))?$", RegexOptions.IgnoreCase);

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
            string scene = null;
            Group current = null;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                Match m;
                if ((m = sScene.Match(line)).Success) { scene = m.Groups[1].Value; current = null; continue; }
                if ((m = sGroup.Match(line)).Success)
                {
                    current = null;
                    if (scene == null) continue;
                    string name = m.Groups[1].Value;
                    if (!groups.TryGetValue(name, out current)) groups[name] = current = new Group { Name = name };
                    foreach (Match pick in sPicks.Matches(m.Groups[2].Value)) current.Picks[pick.Groups[1].Value] = Int(pick.Groups[2].Value);
                    continue;
                }
                if (current != null && (m = sItem.Match(line)).Success)
                {
                    int w = m.Groups[4].Success ? Int(m.Groups[4].Value) : 1;
                    current.Members.Add(new Member(scene, m.Groups[1].Value, Vec(m.Groups[2].Value), Quaternion.Euler(Vec(m.Groups[3].Value)), Math.Max(1, w), m.Groups[5].Success ? m.Groups[5].Value : null));
                }
            }
            return groups.Values.ToList();
        }

        private static int Int(string s) => int.Parse(s, CultureInfo.InvariantCulture);

        private static Vector3 Vec(string s)
        {
            float[] v = s.Split(',').Select(x => float.Parse(x.Trim(), CultureInfo.InvariantCulture)).ToArray();
            return new Vector3(v[0], v[1], v[2]);
        }

        internal static string CurrentLevel()
        {
            try
            {
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

        internal static List<List<Member>> Units(Group g) =>
            g.Members.Select((m, i) => (m, key: m.Set ?? "\u0001" + i)).GroupBy(x => x.key, StringComparer.OrdinalIgnoreCase).Select(u => u.Select(x => x.m).ToList()).ToList();

        internal static List<Member> Pick(Group g, string level, System.Random rng)
        {
            List<List<Member>> pool = Units(g);
            List<Member> picked = new();
            int count = Math.Min(PicksFor(g, level), pool.Count);
            for (int i = 0; i < count; i++)
            {
                int total = pool.Sum(u => u.Max(x => x.Weight));
                int roll = rng.Next(total);
                int index = 0;
                for (; index < pool.Count - 1; index++)
                {
                    roll -= pool[index].Max(x => x.Weight);
                    if (roll < 0) break;
                }
                picked.AddRange(pool[index]);
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
