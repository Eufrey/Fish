using UnityEngine;
namespace Fish
{
    [CreateAssetMenu(menuName="FISH!/Audio Slots")]
    public sealed class FishAudioSettings : ScriptableObject
    {
        [Header("Optional effects — empty slots are silent")]
        public AudioClip uiClick;
        public AudioClip fishingSuccess;
        public AudioClip fishingFailure;
        public AudioClip battleAttack;
        public AudioClip victory;
        public AudioClip defeat;
        [Tooltip("One looping 2D source on shore. Stops while the battle scene is loaded.")]
        public AudioClip shorelineAmbience;
    }
}
