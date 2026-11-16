using System;
using System.Collections.Generic;
using Fish;

public static class FishTeamSelectionChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }

    public static void Main()
    {
        // Distinct IDs stand for individual owned catches, including repeat species.
        var owned = new HashSet<string> { "catch-a", "catch-b", "catch-c", "catch-d" };
        var team = new FishTeamSelection(owned.Contains);
        Check(!team.Confirm() && !team.IsConfirmed, "Empty team cannot confirm");
        Check(!team.TryAdd(null) && !team.TryAdd(""), "Empty ID rejected");
        Check(!team.TryAdd("unowned"), "Only owned catches accepted");
        Check(team.TryAdd("catch-a"), "One owned catch accepted");
        Check(team.Confirm() && team.IsConfirmed, "A one-fish team can confirm");
        Check(!team.TryAdd("catch-a") && team.Members.Count == 1, "One catch cannot occupy two slots");
        Check(team.IsConfirmed, "Rejected change preserves confirmation");
        Check(team.TryAdd("catch-b") && !team.IsConfirmed, "Different owned catch accepted and editing invalidates confirmation");
        Check(team.TryAdd("catch-c"), "Third catch accepted");
        Check(!team.TryAdd("catch-d") && team.Members.Count == 3, "Fourth catch rejected");
        Check(team.Confirm(), "Three-fish team can confirm");
        Check(team.Move(2, 0) && team.Members[0] == "catch-c" && team.Members[1] == "catch-a", "Move to first position preserves ordered catches");
        Check(!team.IsConfirmed, "Reordering needs confirmation again");
        Check(team.Move(0, 2) && team.Members[2] == "catch-c", "Move down preserves order");
        Check(!team.Move(-1, 0) && !team.Move(0, 3) && !team.Move(0, 0), "Invalid moves rejected");
        Check(!team.RemoveAt(-1) && !team.RemoveAt(3), "Invalid removal rejected");
        Check(team.RemoveAt(1) && team.Members.Count == 2 && team.Members[1] == "catch-c", "Removal compacts slots");
        Check(owned.Contains("catch-b"), "Removing from team does not consume catch");
        Check(team.TryAdd("catch-b") && team.Members[2] == "catch-b", "Removed catch can be selected again");
        Check(team.Confirm(), "Edited team can reconfirm");
        IReadOnlyList<string> view = team.Members;
        Check(view[0] == "catch-a" && view[1] == "catch-c" && view[2] == "catch-b", "Reading selection does not reset it");
        bool readOnly = false;
        try { ((IList<string>)view).Add("catch-d"); } catch (NotSupportedException) { readOnly = true; }
        Check(readOnly, "Public member list cannot bypass invariants");
        owned.Remove("catch-c");
        Check(!team.CanConfirm && !team.Confirm(), "No confirmation if a selected catch is no longer owned");
        Check(team.RemoveAt(1) && team.Confirm(), "Invalid catch can be removed and valid remainder confirmed");
        Check(team.RemoveAt(0) && team.RemoveAt(0) && !team.Confirm(), "Removing every member prevents confirmation");
        Console.WriteLine("Passed " + checks + " team-selection checks.");
    }
}
