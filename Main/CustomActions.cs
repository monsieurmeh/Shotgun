using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shotgun
{
    internal class CustomActions
    {
        internal static void MaybePullTrigger()
        {
            PlayerManager playerManagerComponent = GameManager.GetPlayerManagerComponent();
            Panel_HUD hud = InterfaceManager.GetPanel<Panel_HUD>();
            if (hud != null && hud.m_TwoButtonsChoiceUI != null && hud.m_TwoButtonsChoiceUI.IsManagingInput()) return;
            if (playerManagerComponent.m_ItemInHands == null) return;
            if (GameManager.m_PlayerInVehicle != null && GameManager.m_PlayerInVehicle.IsTransitioning) return;
            if (playerManagerComponent.IsInPlacementMode()) return;
            if (InputManager.IsClickHoldActive()) return;
            if (GameManager.m_vpFPSPlayer == null) return;
            GameManager.m_vpFPSPlayer.PullTrigger();
        }
    }
}
