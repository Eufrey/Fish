using System;
using System.Collections.Generic;

namespace Fish
{
    // Session-only ordered catch IDs. Species IDs cannot identify individual team members.
    public sealed class FishTeamSelection
    {
        public const int MaximumMembers = 3;
        private readonly List<string> members = new List<string>();
        private readonly Func<string, bool> ownsCatch;
        public IReadOnlyList<string> Members { get; }
        public bool IsConfirmed { get; private set; }

        public FishTeamSelection(Func<string, bool> ownershipCheck)
        {
            ownsCatch = ownershipCheck ?? throw new ArgumentNullException(nameof(ownershipCheck));
            Members = members.AsReadOnly();
        }

        public bool Contains(string catchId) => members.Contains(catchId);

        public bool CanConfirm
        {
            get
            {
                if (members.Count < 1 || members.Count > MaximumMembers) return false;
                foreach (string id in members)
                    if (!ownsCatch(id)) return false;
                return true;
            }
        }

        public bool TryAdd(string catchId)
        {
            if (string.IsNullOrEmpty(catchId) || members.Count >= MaximumMembers ||
                members.Contains(catchId) || !ownsCatch(catchId)) return false;
            members.Add(catchId);
            IsConfirmed = false;
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= members.Count) return false;
            members.RemoveAt(index);
            IsConfirmed = false;
            return true;
        }

        public bool Move(int from, int to)
        {
            if (from < 0 || from >= members.Count || to < 0 || to >= members.Count || from == to)
                return false;
            string id = members[from];
            members.RemoveAt(from);
            members.Insert(to, id);
            IsConfirmed = false;
            return true;
        }

        public bool Confirm()
        {
            IsConfirmed = CanConfirm;
            return IsConfirmed;
        }
    }
}
