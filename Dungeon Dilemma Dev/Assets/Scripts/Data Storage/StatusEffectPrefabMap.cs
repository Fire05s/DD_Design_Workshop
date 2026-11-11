using System;
using System.Collections.Generic;
using UnityEngine;

namespace Priestess
{
    [CreateAssetMenu(fileName = "StatusEffectPrefabMap", menuName = "Status Effect Map")]
    public class StatusEffectPrefabMap : ScriptableObject
    {
        [Serializable]
        public struct StatusEntry
        {
            public StatusEffectType type;
            public GameObject prefab;
        }

        [SerializeField] private List<StatusEntry> entries;

        private Dictionary<StatusEffectType, GameObject> _map;

        public GameObject GetPrefab(StatusEffectType type)
        {
            if (_map == null)
            {
                _map = new Dictionary<StatusEffectType, GameObject>();
                foreach (var entry in entries)
                {
                    if (entry.prefab != null && !_map.ContainsKey(entry.type))
                        _map.Add(entry.type, entry.prefab);
                }
            }

            return _map.TryGetValue(type, out var prefab) ? prefab : null;
        }
    }
}