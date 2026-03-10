#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

/// <summary>
/// Converts Lua prototype tables (MoonSharp DynValue/Table) to Newtonsoft JToken
/// for use with the existing LoadJsonPrototypes pipeline.
/// </summary>
public static class LuaPrototypeConverter
{
    /// <summary>
    /// Converts a DynValue (typically a Table returned from Lua) to JToken.
    /// </summary>
    /// <param name="dynValue">The Lua return value (table, string, number, etc.).</param>
    /// <returns>JToken equivalent, or null if conversion fails.</returns>
    public static JToken DynValueToJToken(DynValue dynValue)
    {
        if (dynValue == null || dynValue.IsNil())
        {
            return null;
        }

        switch (dynValue.Type)
        {
            case DataType.Table:
                return TableToJToken(dynValue.Table);
            case DataType.String:
                return new JValue(dynValue.String);
            case DataType.Number:
                return new JValue(dynValue.Number);
            case DataType.Boolean:
                return new JValue(dynValue.Boolean);
            case DataType.Nil:
                return null;
            default:
                UnityDebugger.Debugger.LogWarning("LuaPrototypeConverter", "Unsupported DynValue type: " + dynValue.Type);
                return null;
        }
    }

    private static JToken TableToJToken(Table table)
    {
        if (table == null)
        {
            return null;
        }

        if (IsArrayLike(table, out var arrayValues))
        {
            var jarray = new JArray();
            foreach (var v in arrayValues)
            {
                var token = DynValueToJToken(v);
                if (token != null)
                {
                    jarray.Add(token);
                }
            }
            return jarray;
        }

        var jobject = new JObject();
        foreach (TablePair pair in table.Pairs)
        {
            string key = PairKeyToString(pair.Key);
            if (key == null)
            {
                continue;
            }

            JToken value = DynValueToJToken(pair.Value);
            if (value != null)
            {
                jobject[key] = value;
            }
        }

        return jobject;
    }

    private static bool IsArrayLike(Table table, out System.Collections.Generic.List<DynValue> values)
    {
        values = new System.Collections.Generic.List<DynValue>();
        var indexed = new System.Collections.Generic.SortedDictionary<int, DynValue>();
        foreach (TablePair pair in table.Pairs)
        {
            if (pair.Key.Type != DataType.Number)
            {
                return false;
            }
            int idx = (int)pair.Key.Number;
            if (idx < 1)
            {
                return false;
            }
            indexed[idx] = pair.Value;
        }
        if (indexed.Count == 0)
        {
            return false;
        }
        for (int i = 1; i <= indexed.Count; i++)
        {
            if (!indexed.TryGetValue(i, out var v))
            {
                return false;
            }
            values.Add(v);
        }
        return true;
    }

    private static string PairKeyToString(DynValue key)
    {
        if (key == null || key.IsNil())
        {
            return null;
        }

        switch (key.Type)
        {
            case DataType.String:
                return key.String;
            case DataType.Number:
                return key.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            default:
                return null;
        }
    }
}
