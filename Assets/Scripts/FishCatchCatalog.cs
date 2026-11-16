using System;
using System.Collections.Generic;
using UnityEngine;
namespace Fish
{
    [CreateAssetMenu(menuName = "FISH!/Catch Catalog")]
    public sealed class FishCatchCatalog : ScriptableObject
    {
        [Serializable] public sealed class Entry
        {
            public FishSpecies species;
            public bool enabled = true;
            [Range(1, 100000)] public int weight = 1;
        }
        public Entry[] entries;
        public bool IsValid()
        {
            if (entries == null || entries.Length == 0 || entries.Length > 100) return false;
            var ids = new HashSet<string>();
            bool any = false;
            foreach (Entry entry in entries)
            {
                if (entry == null || entry.species == null || string.IsNullOrWhiteSpace(entry.species.speciesId) ||
                    !ids.Add(entry.species.speciesId)) return false;
                if (!entry.enabled) continue;
                if (entry.weight < 1 || entry.weight > 100000) return false;
                any = true;
            }
            return any;
        }
        public FishSpecies Choose()
        {
            if (!IsValid()) return null;
            int[] weights = new int[entries.Length];
            int total = 0;
            for (int i = 0; i < entries.Length; i++)
            { weights[i] = entries[i].enabled ? entries[i].weight : 0; total += weights[i]; }
            // Integer tickets give even the smallest enabled weight a reachable interval.
            return entries[FishCatchWeights.Choose(weights, UnityEngine.Random.Range(0, total))].species;
        }
    }
}
