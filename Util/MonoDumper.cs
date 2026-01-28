using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

static class MonoDumper
{
    const int MAX_DEPTH = 3;

    public static void Dump(object obj)
    {
        DumpInternal(obj, 0, new HashSet<object>());
    }

    static void DumpInternal(object obj, int depth, HashSet<object> visited)
    {
        if (obj == null) return;
        if (depth > MAX_DEPTH) return;

        // Prevent cycles
        if (!IsSimple(obj))
        {
            if (visited.Contains(obj)) return;
            visited.Add(obj);
        }

        var type = obj.GetType();
        string indent = new string(' ', depth * 2);

        try
        {
            // Handle iterables (but not strings)
            if (obj is IEnumerable enumerable && obj is not string)
            {
                int i = 0;
                foreach (var item in enumerable)
                {
                    DumpInternal(item, depth + 1, visited);
                    if (++i > 20) break; // hard cap to avoid spam
                }
                return;
            }

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    var val = f.GetValue(obj);
                    MelonLogger.Msg($"{indent}{type.Name}.{f.Name}: {val}");
                    DumpInternal(val, depth + 1, visited);
                }
                catch { }
            }

            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                try
                {
                    if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
                    var val = p.GetValue(obj);
                    MelonLogger.Msg($"{indent}{type.Name}.{p.Name}: {val}");
                    DumpInternal(val, depth + 1, visited);
                }
                catch { }
            }
        }
        catch { }
    }

    static bool IsSimple(object obj)
    {
        var t = obj.GetType();
        return t.IsPrimitive
            || obj is string
            || obj is decimal
            || obj is Vector2
            || obj is Vector3
            || obj is Quaternion;
    }
}