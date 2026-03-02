using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 怪物 MonsterIndex 到 Prefab 路径的映射配置。
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterPrefabConfig", menuName = "Legendary/Monster Prefab Config")]
    public class MonsterPrefabConfig : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public int MonsterIndex;
            public string ResourcesPath;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();
        private Dictionary<int, string> _lookup;

        public GameObject GetPrefab(int monsterIndex)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<int, string>();
                foreach (var e in _entries)
                    _lookup[e.MonsterIndex] = e.ResourcesPath;
            }
            if (_lookup.TryGetValue(monsterIndex, out var path) && !string.IsNullOrEmpty(path))
                return Resources.Load<GameObject>(path);
            return null;
        }
    }
}
