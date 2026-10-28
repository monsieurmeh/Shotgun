using Il2Cpp;
using UnityEngine;

namespace Shotgun.ToolBox
{
    internal static class NGUIExtensions
    {
        internal static GameObject GetOrCloneUnanchored(this GameObject template, string name)
        {
            Transform parent = template.transform.parent;
            Transform? existing = parent.Find(name);
            GameObject clone = existing != null ? existing.gameObject : UnityEngine.Object.Instantiate(template, parent, false);
            clone.name = name;
            foreach (UIRect rect in clone.GetComponentsInChildren<UIRect>(true))
                if (rect.isAnchored) rect.SetAnchor((Transform?)null);
            return clone;
        }


        internal static void SetTextIfChanged(this UILabel? label, string text)
        {
            if (label != null && label.text != text) label.text = text;
        }


        internal static string LocalizedOr(string key, string fallback)
        {
            string localized = Localization.Get(key);
            return string.IsNullOrEmpty(localized) || localized == key ? fallback : localized;
        }


        internal static string PromptText(this KeyCode key)
        {
            string name = key.ToString();
            return name.Length == 1 ? name : name.ToUpperInvariant();
        }
    }
}
