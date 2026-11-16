using System;

namespace Fish
{
    // Each success creates a separate owned instance, even for the same species.
    public sealed class FishOwnedCatch
    {
        public string CatchId { get; }
        public FishSpecies Species { get; }
        public int MaximumHP { get; }
        public int CurrentHP { get; private set; }
        public void SetHealth(int hp) => CurrentHP = Math.Max(0, Math.Min(MaximumHP, hp));
        public void Recover() => SetHealth(MaximumHP);

        public FishOwnedCatch(FishSpecies species, string id, int hp, int maximum) { Species = species; CatchId = id; MaximumHP = maximum; SetHealth(hp); }
        public FishOwnedCatch(FishSpecies species)
        {
            Species = species;
            MaximumHP = Math.Max(1, species.health);
            Recover();
            CatchId = Guid.NewGuid().ToString("N");
        }
    }
}

