using System.Collections.Generic;

namespace Fish
{
    // Owned by the existing collection for this Play session. No static or saved flags.
    public sealed class FishStoryProgress
    {
        // 0 = catching, 1-3 = available encounter, 4 = completed.
        public int Stage { get; private set; }
        private readonly Queue<string> notices = new Queue<string>();
        public bool OpeningShown => openingShown; public bool EndingShown => endingShown;
        public void Restore(int stage, bool opening, bool ending) { Stage = stage; openingShown = opening; endingShown = ending; notices.Clear(); }
        private bool openingShown;
        private bool endingShown;
        public bool TryPresentOpening() { if (openingShown) return false; openingShown = true; return true; }
        public bool TryPresentEnding() { if (Stage != 4 || endingShown) return false; endingShown = true; return true; }
        public string Objective => Stage == 0 ? "Catch three fish." : Stage == 1 ? "Defeat the First Disturbance." :
            Stage == 2 ? "Defeat the Growing Threat." : Stage == 3 ? "Defeat the environmental boss." : "Completed";
        public bool CanEnter(int encounter) => encounter >= 1 && encounter <= 3 && Stage == encounter;
        public void ObserveOwnedCount(int count)
        {
            if (Stage != 0 || count < 3) return;
            Stage = 1;
            notices.Enqueue("First encounter unlocked. Confirm a team with T, then interact with its marker.");
        }
        public bool ApplyVictory(int encounter)
        {
            if (!CanEnter(encounter)) return false;
            Stage++;
            notices.Enqueue(Stage == 2 ? "Second encounter unlocked. Confirm your team before interacting with its marker." :
                Stage == 3 ? "Boss encounter unlocked. Prepare your team." :
                "Completed.");
            return true;
        }
        public bool TryTakeNotice(out string text)
        {
            text = notices.Count > 0 ? notices.Dequeue() : null;
            return text != null;
        }
    }
}


