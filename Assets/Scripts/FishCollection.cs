using System.Collections.Generic;
using UnityEngine;

namespace Fish
{
    public sealed class FishCollection : MonoBehaviour
    {
        // Runtime data belongs to this scene component, never to the species asset or panel.
        private readonly List<FishOwnedCatch> catches = new List<FishOwnedCatch>();
        public IReadOnlyList<FishOwnedCatch> Catches => catches.AsReadOnly();
        public int Count => catches.Count;
        public FishStoryProgress Story { get; } = new FishStoryProgress();

        public void Restore(IEnumerable<FishOwnedCatch> fish, int stage, bool introduction, bool ending) { catches.Clear(); catches.AddRange(fish); Story.Restore(stage, introduction, ending); }
        public bool TryAddCatch(FishTimingAttempt attempt, FishSpecies species)
        {
            // Check setup before claiming: an invalid reference cannot consume the reward.
            if (attempt == null || species == null || !attempt.TryClaimCatch()) return false;
            catches.Add(new FishOwnedCatch(species));
            Story.ObserveOwnedCount(catches.Count);
            FishSave.Request();
            return true;
        }
    }
}


