using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gear;
using UnityEngine;

namespace Shotgun
{
    internal static class CustomAmmoHUD
    {
        internal sealed class HUDPreset
        {
            public Sprite LoadedSprite;
            public Sprite UnloadedSprite;
        }

        private const float IconStep = 1.35f;

        private static readonly Dictionary<string, HUDPreset> sHUDPresets = new();
        private static readonly List<UI2DSprite> sIcons = new();


        internal static bool TryRegisterHUDPreset(string gunGearName, string ammoGearName, HUDPreset preset) => sHUDPresets.TryAdd(gunGearName + "." + ammoGearName, preset);


        internal static void Refresh()
        {
            EquipItemPopup popup = InterfaceManager.GetPanel<Panel_HUD>()?.m_EquipItemPopup;
            if (popup != null) Apply(popup);
        }


        private static void Apply(EquipItemPopup popup)
        {
            HUDPreset preset = PresetInHands(out GunItem gun);
            Il2CppReferenceArray<UISprite> slots = popup.m_ListAmmoSprites;
            if (preset == null || slots == null)
            {
                HideFrom(0);
                return;
            }

            int slotCount = Math.Min(gun.m_ClipSize, slots.Length);
            UISprite first = slots[0];
            if (first == null)
            {
                HideFrom(0);
                return;
            }
            Vector3 direction = slots.Length > 1 && slots[1] != null ? (slots[1].transform.localPosition - first.transform.localPosition).normalized : Vector3.left;
            for (int i = 0; i < slotCount; i++)
            {
                if (slots[i] != null) slots[i].alpha = 0f;
                ShowIcon(i, first, direction, i < gun.m_Clip.Count ? preset.LoadedSprite : preset.UnloadedSprite);
            }
            HideFrom(slotCount);
        }


        private static HUDPreset PresetInHands(out GunItem gun)
        {
            gun = null;
            GearItem inHands = GameManager.GetPlayerManagerComponent()?.m_ItemInHands;
            if (inHands == null) return null;
            gun = inHands.m_GunItem;
            if (gun == null || !gun.TryGetSelectedAmmo(out GearItemData ammo) || ammo == null) return null;
            return sHUDPresets.TryGetValue($"{inHands.name}.{ammo.name}", out HUDPreset preset) ? preset : null;
        }


        private static void ShowIcon(int index, UISprite slot, Vector3 direction, Sprite sprite)
        {
            while (sIcons.Count <= index) sIcons.Add(null);
            UI2DSprite icon = sIcons[index];
            if (icon == null || icon.transform.parent != slot.transform.parent)
            {
                if (icon != null) UnityEngine.Object.Destroy(icon.gameObject);
                GameObject iconObject = new($"DoubleBarrelAmmoIcon{index}") { layer = slot.gameObject.layer };
                iconObject.transform.SetParent(slot.transform.parent, false);
                icon = iconObject.AddComponent<UI2DSprite>();
                icon.shader = slot.material?.shader;
                sIcons[index] = icon;
            }

            int width = sprite == null ? slot.width : Mathf.Max(2, Mathf.RoundToInt(slot.height * sprite.rect.width / sprite.rect.height));
            icon.transform.localPosition = slot.transform.localPosition + direction * (index * width * IconStep);
            icon.transform.localRotation = slot.transform.localRotation;
            icon.transform.localScale = slot.transform.localScale;
            icon.pivot = slot.pivot;
            icon.depth = slot.depth + 1;
            icon.sprite2D = sprite;
            icon.height = slot.height;
            icon.width = width;
            icon.alpha = 1f;
        }


        private static void HideFrom(int index)
        {
            for (int i = index; i < sIcons.Count; i++)
            {
                if (sIcons[i] != null) sIcons[i].alpha = 0f;
            }
        }


        [HarmonyPatch(typeof(EquipItemPopup), nameof(EquipItemPopup.UpdateAmmoStatus))]
        internal static class EquipItemPopup_ShowShotgunShells
        {
            internal static void Postfix(EquipItemPopup __instance) => Apply(__instance);
        }
    }
}
