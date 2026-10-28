using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Shotgun
{
    internal static class AmmoSwapPrompt
    {
        private const string PromptName = "ButtonPromptChangeAmmo";
        private const string LabelKey = "GAMEPLAY_Shotgun_ChangeAmmo";
        private const string FallbackLabel = "Change Ammo";

        private static ButtonPrompt? mPrompt;


        [HarmonyPatch(typeof(EquipItemPopup), nameof(EquipItemPopup.Update))]
        internal static class EquipItemPopup_ShowChangeAmmoPrompt
        {
            internal static void Postfix(EquipItemPopup __instance) => Refresh(__instance);
        }


        private static void Refresh(EquipItemPopup popup)
        {
            ButtonPrompt reload = popup.m_ButtonPromptReload;
            if (reload == null) return;
            ButtonPrompt? prompt = GetOrCreate(reload);
            if (prompt == null) return;

            GunItem? gunItem = ShotgunInHands();
            bool show = gunItem != null
                && reload.gameObject.activeSelf
                && !Il2Cpp.Utils.IsGamepadActive();
            if (prompt.gameObject.activeSelf != show) prompt.gameObject.SetActive(show);
            if (!show) return;

            prompt.transform.MirrorLayout(reload.transform);
            prompt.m_KeyboardButtonLabel.SetTextIfChanged(Settings.Instance.AmmoSwapKeyCode.PromptText());
            prompt.m_KeyboardActionLabel.SetTextIfChanged(NGUIExtensions.LocalizedOr(LabelKey, FallbackLabel));
            UILabel? enabledLabel = popup.m_ButtonPromptHolster?.m_KeyboardActionLabel;
            if (prompt.m_KeyboardActionLabel != null)
                prompt.m_KeyboardActionLabel.color = CanSwap(gunItem) ? (enabledLabel != null ? enabledLabel.color : Color.white) : prompt.m_DisabledColor;
            Place(reload, popup.m_ButtonPromptHolster, prompt);
        }


        private static ButtonPrompt? GetOrCreate(ButtonPrompt reload)
        {
            if (mPrompt != null) return mPrompt;
            mPrompt = reload.gameObject.GetOrCloneUnanchored(PromptName).GetComponent<ButtonPrompt>();
            if (mPrompt?.m_KeyboardActionLabel != null)
                mPrompt.m_KeyboardActionLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
            return mPrompt;
        }


        private static void Place(ButtonPrompt reload, ButtonPrompt? holster, ButtonPrompt prompt)
        {
            float gap = holster != null && holster.transform.parent == reload.transform.parent
                ? holster.transform.localPosition.x - reload.transform.localPosition.x - Extent(reload)
                : 0f;
            Vector3 position = reload.transform.localPosition;
            position.x -= gap + Extent(prompt);
            prompt.transform.localPosition = position;
        }


        private static float Extent(ButtonPrompt prompt)
        {
            UILabel label = prompt.m_KeyboardActionLabel;
            return label == null ? 0f : label.transform.localPosition.x + label.width;
        }


        private static bool CanSwap(GunItem? gunItem) => gunItem != null && !gunItem.m_IsJammed && gunItem.GetNextValidAmmoIndex() != gunItem.m_SelectedAmmoIndex;
    }
}
