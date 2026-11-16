using System;

namespace Fish
{
    // A temporary stat/health snapshot, never an owned catch or shared species asset.
    public sealed class FishBattleFighter
    {
        public string Name { get; }
        public string OwnedCatchId { get; }
        public int MaxHealth { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int Health { get; private set; }
        internal int InitialHealth { get; set; }
        public bool IsDefending { get; private set; }
        public FishSpecialEffect SpecialEffect { get; }
        public int SpecialPower { get; }
        public string SpecialName { get; }
        public string SpecialDescription { get; }
        public bool SpecialUsed { get; private set; }
        public bool CanUseSpecial => Health > 0 && !SpecialUsed && SpecialPower > 0 &&
            (SpecialEffect == FishSpecialEffect.StrongerAttack || SpecialEffect == FishSpecialEffect.SelfHeal);

        public FishBattleFighter(string name, int health, int attack, int defense, string ownedCatchId = null,
            FishSpecialEffect specialEffect = FishSpecialEffect.None, int specialPower = 0,
            string specialName = "", string specialDescription = "", int? currentHP = null)
        {
            Name = name;
            OwnedCatchId = ownedCatchId;
            MaxHealth = Math.Max(1, health);
            Health = InitialHealth = Math.Max(0, Math.Min(MaxHealth, currentHP ?? MaxHealth));
            Attack = Math.Max(0, attack);
            Defense = Math.Max(0, defense);
            SpecialEffect = specialEffect;
            SpecialPower = Math.Max(0, specialPower);
            SpecialName = string.IsNullOrEmpty(specialName) ? "Special Ability" : specialName;
            SpecialDescription = specialDescription;
        }

        internal FishBattleFighter FreshCopy() => new FishBattleFighter(Name, MaxHealth, Attack, Defense, OwnedCatchId,
            SpecialEffect, SpecialPower, SpecialName, SpecialDescription, InitialHealth);
        internal void SpendSpecial() => SpecialUsed = true;
        internal int Heal(int amount)
        {
            int restored = Math.Min(Math.Max(0, amount), MaxHealth - Health);
            Health += restored;
            return restored;
        }
        internal void BeginTurn() => IsDefending = false;
        internal void Defend() => IsDefending = true;
        internal void TakeDamage(int damage) => Health = Math.Max(0, Health - damage);
    }
}
