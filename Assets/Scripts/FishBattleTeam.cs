using UnityEngine;

namespace Fish
{
    public sealed class FishBattleTeam : MonoBehaviour
    {
        [SerializeField] private FishCollection collection;
        private FishTeamSelection selection;
        public FishCollection Collection => collection;
        public bool HasUsableFish
        {
            get { for (int i = 0; i < Selection.Members.Count; i++) if (GetMember(i)?.CurrentHP > 0) return true; return false; }
        }
        public void RecoverTeam() { for (int i = 0; i < Selection.Members.Count; i++) GetMember(i)?.Recover(); }
        public FishTeamSelection Selection => selection ?? (selection = new FishTeamSelection(OwnsCatch));

        public void Configure(FishCollection ownedFish) => collection = ownedFish;

        private bool OwnsCatch(string catchId) => FindCatch(catchId) != null;

        private FishOwnedCatch FindCatch(string catchId)
        {
            if (collection == null) return null;
            foreach (FishOwnedCatch owned in collection.Catches)
                if (owned.CatchId == catchId && owned.Species != null) return owned;
            return null;
        }

        public FishOwnedCatch GetMember(int slot)
        {
            if (slot < 0 || slot >= Selection.Members.Count) return null;
            return FindCatch(Selection.Members[slot]);
        }
    }
}
