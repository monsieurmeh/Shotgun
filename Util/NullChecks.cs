using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

namespace Shotgun.Utility
{
    internal class NullChecks
    {
        internal static Dictionary<Type, List<string>> TypeToPropertyNameDict = new Dictionary<Type, List<string>>()
        {
            {
                typeof(vp_FPSWeapon), new List<string>()
                {
                    "Controller",
                    "m_DefaultState",
                    "m_PositionSpring",
                    "m_PositionSpring",
                    "m_PositionSpring2",
                    "m_PositionPivotSpring",
                    "m_WeaponCamera",
                    "m_WeaponGroup",
                    "m_Pivot",
                    "m_RotationSpring",
                    "m_RotationSpring2",
                    "m_FXGameObject",
                    "m_LightIndoor",
                    "m_LightOutdoor",
                    "m_WeaponCameraComponent",
                    "m_Animator",
                    "m_GearItem",
                    "m_GunItem",
                    "m_BowItem",
                    "m_FirstPersonWeaponRightHand",
                    "m_FirstPersonWeaponLeftHand",
                    "m_FirstPersonWeaponShoulder",
                    "m_FirstPersonWeaponRightHandPrefab",
                    "m_FirstPersonWeaponLeftHandPrefab",
                    "m_FirstPersonWeaponShoulderPrefab",
                    "m_RightTemporaryParent",
                    "m_LeftTemporaryParent",
                    "m_ShoulderTemporaryParent",
                    "m_MovedChild",
                    "m_NewChild",
                    "m_FixedUpdateCallback"
                }
            },
        };

        internal static void ReportNulls<T>(T t) where T : MonoBehaviour
        {
            if (!TypeToPropertyNameDict.TryGetValue(t.GetType(), out List<string> propertyNames))
            {
                MelonLogger.Error($"[ReportNulls] type not registered for null checks: {typeof(T).Name}");
                return;
            }

            foreach (string propertyName in propertyNames)
            {
                t.ReportIfPropertyNull(propertyName);
            }
        }
    }

    internal static class NulLCheckExtension
    {
        internal static void ReportIfPropertyNull<T>(this T script, string propertyName) where T : MonoBehaviour
        {
            PropertyInfo propertyInfo = script.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                MelonLogger.Error($"'{propertyName}' not found.");
                return;
            }
            if (propertyInfo.GetValue(script, null) == null)
            {
                MelonLogger.Msg($"'{propertyName}' is null");
            }
        }

        internal static void ReportNullProperties<T>(this T script) where T : MonoBehaviour
        {
            NullChecks.ReportNulls(script);
        }
    }
}
