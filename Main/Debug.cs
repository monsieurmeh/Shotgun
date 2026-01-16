using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace Shotgun
{
    internal class Debug
    {
        [HarmonyPatch(typeof(InputManager), nameof(InputManager.ProcessFireAction), new Type[] { typeof(MonoBehaviour), typeof(Panel_HUD) })]
        internal static class ProcessFireAction
        {
            internal static void Prefix(MonoBehaviour context, Panel_HUD hud)
            {
                bool firePressed = InputManager.GetFirePressed(context);
                if (firePressed)
                {
                    PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
                    PlayerAnimation playerAnimationComponent = GameManager.GetPlayerAnimationComponent();
                    MelonLogger.Msg($"InputManager.ProcessFireAction with firePressed");
                    MelonLogger.Msg($"ItemInHands: {playerManagerComponent.m_ItemInHands}");
                    if (playerManagerComponent.m_ItemInHands != null)
                    {
                        MelonLogger.Msg($"GunItem in hands: {playerManagerComponent.m_ItemInHands.m_GunItem}");
                    } 
                    else
                    {
                        MelonLogger.Msg($"No GunItem in Hand!");
                    }
                    MelonLogger.Msg($"InterfaceManager.IsOverlayActiveImmediate(): {InterfaceManager.IsOverlayActiveImmediate()}");
                    MelonLogger.Msg($"hud.m_TwoButtonsChoiceUI: {hud.m_TwoButtonsChoiceUI}");
                    MelonLogger.Msg($"hud.m_TwoButtonsChoiceUI.IsManagingInput(): {hud.m_TwoButtonsChoiceUI.IsManagingInput()}");
                    bool playerInVehicleNull = GameManager.m_PlayerInVehicle == null;
                    MelonLogger.Msg($"GameManager.m_PlayerInVehicle is null: {playerInVehicleNull}");
                    bool isTransitioning = playerInVehicleNull ? GameManager.m_PlayerInVehicle.IsTransitioning : false;
                    MelonLogger.Msg($"GameManager.m_PlayerInVehicle.isTransitioning: {isTransitioning}");
                    MelonLogger.Msg($"playerManagerComponent.IsInPlacementMode(): {playerManagerComponent.IsInPlacementMode()}");
                    bool itemInHandsInternalNull = playerManagerComponent.m_ItemInHandsInternal == null;
                    MelonLogger.Msg($"playerManagerComponent.m_ItemInHandsInternal null: {itemInHandsInternalNull}");
                    MelonLogger.Msg($"InputManager.IsClickHoldActive(): {InputManager.IsClickHoldActive()}");
                    if (!itemInHandsInternalNull)
                    {
                        MelonLogger.Msg($"playerManagerComponent.m_ItemInHandsInternal.m_BowItem == null: {playerManagerComponent.m_ItemInHandsInternal.m_BowItem == null}");
                        MelonLogger.Msg($"playerManagerComponent.m_ItemInHandsInternal.m_GunItem == null: {playerManagerComponent.m_ItemInHandsInternal.m_GunItem == null}");
                        MelonLogger.Msg($"GameManager.m_vpFPSPlayer == null: {GameManager.m_vpFPSPlayer == null}");
                    }
                }
            }
        }
    }
}
