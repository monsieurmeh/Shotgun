using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using static Il2Cpp.Utils;
using Harmony;

namespace Shotgun.Utility
{
    internal class NullChecks
    {
        internal static Dictionary<Type, List<string>> TypeToPropertyNameDict = new Dictionary<Type, List<string>>()
        {
        };

        internal static void ReportNulls<T>(T t, string optionalDescription = "") where T : MonoBehaviour
        {
            Type tType = t.GetType();
            if (!TypeToPropertyNameDict.TryGetValue(tType, out List<string> propertyNames))
            {
                PropertyInfo[] properties = tType.GetProperties();
                propertyNames = new List<string>();
                foreach (PropertyInfo property in properties)
                {
                    propertyNames.Add(property.Name);
                }
            }
            MelonLogger.Msg("");
            MelonLogger.Msg("");
            MelonLogger.Msg("");
            MelonLogger.Msg("NullCheck on {t.name}");
            if (optionalDescription.Length > 0)
            {
                MelonLogger.Msg(optionalDescription);
            }
            MelonLogger.Msg("");
            foreach (string propertyName in propertyNames)
            {
                t.ReportIfPropertyNull(propertyName);
            }
            MelonLogger.Msg("");
        }
    }

    internal static class NulLCheckExtension
    {
        internal static void ReportIfPropertyNull<T>(this T script, string propertyName) where T : MonoBehaviour
        {
            try
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
            catch (Exception e)
            {
                MelonLogger.Error($"Error while trying to access '{propertyName}'");
            }
        }

        internal static void ReportNullProperties<T>(this T script, string optionalDescription = "") where T : MonoBehaviour
        {
            NullChecks.ReportNulls(script, optionalDescription);
        }
    }
}
