using UnityEngine;

namespace Fish
{
    [CreateAssetMenu(menuName = "FISH!/Enemy Team", fileName = "EnemyTeam")]
    public sealed class FishEnemyTeam : ScriptableObject
    {
        public FishSpecies[] members;
        [Tooltip("Use a configured special once per fish, on its first turn at half HP or lower.")]
        public bool useSpecialAtHalfHealth;

        public bool TryCreateFighters(out FishBattleFighter[] fighters)
        {
            fighters = null;
            if (members == null || members.Length == 0) return false;
            var result = new FishBattleFighter[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                FishSpecies species = members[i];
                if (species == null) return false;
                result[i] = new FishBattleFighter(species.displayName, species.health, species.attack, species.defense, null,
                    species.specialAbilityType, species.specialAbilityPower, species.specialAbilityName, species.specialAbilityDescription);
            }
            fighters = result;
            return true;
        }
    }
}
