using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace LegendaryTerrain.Editor
{
    [CustomPropertyDrawer(typeof(MapTitleDropdownAttribute))]
    public class MapTitleDropdownDrawer : PropertyDrawer
    {
        private static List<Mir2MapInfo> _cachedMapInfos;
        private static string _cachedMirDbPath;
        private static Dictionary<string, string> _cachedZhTitles;
        private static string _lastResolvedPath;

        private static Dictionary<string, string> GetZhTitles(string baseDir)
        {
            if (_cachedZhTitles != null) return _cachedZhTitles;
            _cachedZhTitles = new Dictionary<string, string>();
            string[] candidates = {
                Path.Combine(Application.streamingAssetsPath, "LegendaryData", "MapTitles_zh.txt"),
                Path.Combine(baseDir, "StreamingAssets", "LegendaryData", "MapTitles_zh.txt")
            };
            foreach (var p in candidates)
            {
                string path = Path.GetFullPath(p);
                if (!File.Exists(path)) continue;
                try
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        var s = line.Trim();
                        if (string.IsNullOrEmpty(s) || s.StartsWith("#")) continue;
                        int eq = s.IndexOf('=');
                        if (eq > 0)
                        {
                            string key = s.Substring(0, eq).Trim();
                            string val = s.Substring(eq + 1).Trim();
                            if (!string.IsNullOrEmpty(key)) _cachedZhTitles[key] = val;
                        }
                    }
                    break;
                }
                catch { }
            }
            return _cachedZhTitles;
        }

        private static (List<Mir2MapInfo> list, bool fileFound) GetMapInfosWithStatus()
        {
            string baseDir = Path.GetFullPath(Application.dataPath);
            string[] candidates = {
                Path.Combine(Application.streamingAssetsPath, "LegendaryData", "Server.MirDB"),
                Path.Combine(baseDir, "StreamingAssets", "LegendaryData", "Server.MirDB"),
                Path.Combine(baseDir, "StreamingAssets", "LegendaryData", "Crystal.Database", "Jev", "Server.MirDB")
            };
            string mirDbPath = null;
            foreach (var p in candidates)
            {
                string full = Path.GetFullPath(p);
                if (File.Exists(full)) { mirDbPath = full; break; }
            }
            mirDbPath ??= Path.GetFullPath(candidates[0]);
            _lastResolvedPath = mirDbPath;
            bool fileFound = File.Exists(mirDbPath);

            if (_cachedMapInfos != null && _cachedMirDbPath == mirDbPath) return (_cachedMapInfos, fileFound);
            _cachedMirDbPath = mirDbPath;
            var list = fileFound ? MirDBParser.Parse(mirDbPath) : new List<Mir2MapInfo>();
            if (fileFound && list.Count == 0)
                _cachedMapInfos = null;
            else
                _cachedMapInfos = list;
            return (_cachedMapInfos ?? list, fileFound);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var (mapInfos, fileFound) = GetMapInfosWithStatus();
            if (mapInfos.Count == 0)
            {
                string msg, tooltip;
                if (fileFound)
                {
                    var (version, mapCount, error) = MirDBParser.GetDiagnostics(_lastResolvedPath);
                    msg = string.IsNullOrEmpty(error) ? "解析返回空" : "MirDB 解析失败";
                    tooltip = string.IsNullOrEmpty(error)
                        ? $"版本={version}, 地图数={mapCount}，但解析返回空（可能 ReadMapInfo 格式不匹配）"
                        : $"{error}\n路径: {_lastResolvedPath}";
                }
                else
                {
                    msg = "MirDB 未找到";
                    tooltip = "请先执行 Tools > Legendary > Download Crystal Data，或手动输入地图文件名";
                }
                EditorGUI.PropertyField(position, property, new GUIContent(label.text + $" ({msg})", tooltip));
                return;
            }

            string baseDir = Path.GetFullPath(Application.dataPath);
            var zhTitles = GetZhTitles(baseDir);

            var fileNames = new string[mapInfos.Count];
            var titles = new GUIContent[mapInfos.Count];
            for (int i = 0; i < mapInfos.Count; i++)
            {
                fileNames[i] = mapInfos[i].FileName;
                string en = string.IsNullOrEmpty(mapInfos[i].Title) ? mapInfos[i].FileName : mapInfos[i].Title;
                string zh;
                zhTitles.TryGetValue(mapInfos[i].FileName, out zh);
                string display = string.IsNullOrEmpty(zh) ? en : $"{en} ({zh})";
                titles[i] = new GUIContent(display);
            }

            string current = property.stringValue;
            int selected = -1;
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (fileNames[i] == current) { selected = i; break; }
            }
            if (selected < 0)
            {
                EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (未在 MirDB 中匹配)"));
                return;
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label, selected, titles);
            if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < fileNames.Length)
                property.stringValue = fileNames[newIndex];
        }
    }
}
