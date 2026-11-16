using UnityEngine;
using UnityEngine.Serialization;

namespace Fish
{
    [CreateAssetMenu(menuName = "FISH!/Fish Species", fileName = "FishSpecies")]
    public sealed class FishSpecies : ScriptableObject
    {
        public enum Rarity { Common, Uncommon, Rare }

        [Header("Identity and presentation")]
        [Tooltip("Stable unique species ID; owned catches reference this shared asset.")]
        public string speciesId;
        public string displayName;
        public bool isPlaceholder;
        public Sprite icon;
        [Tooltip("Visual-only prefab. Face local +X with Y up; battle presentation fits its renderer bounds. Missing models use a stylized primitive fallback.")]
        public GameObject model;
        [Tooltip("Gameplay catch frequency only; not biological abundance or conservation status.")]
        public Rarity rarity;

        [Header("Names: leave unverified fields empty")]
        public string localName;
        public string englishCommonName;
        public string scientificName;
        public string localNameLanguageOrRegion;

        [Header("Fictional gameplay stats, not biological facts")]
        [Min(0)] public int health;
        [Min(0)] public int attack;
        [Min(0)] public int defense;
        [Min(0)] public int speed;
        public string specialAbilityName;
        [FormerlySerializedAs("specialAbilityEffect")]
        [TextArea] public string specialAbilityDescription;
        public FishSpecialEffect specialAbilityType;
        [Min(1)] public int specialAbilityPower = 1;

        [Header("Verified educational content: leave unresearched fields empty")]
        [TextArea] public string habitat;
        [TextArea] public string description;
        [TextArea] public string educationalFact;

        [Header("Development reference metadata")]
        [TextArea] public string referenceNotes;
        [TextArea] public string referenceUrls;
    }
}

