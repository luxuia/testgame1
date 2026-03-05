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
                return TableToJObject(dynValue.Table);
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

    private static JObject TableToJObject(Table table)
    {
        if (table == null)
        {
            return null;
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
