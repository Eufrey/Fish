using System;
using Fish;

// Standalone checks for the pure attempt rules; not a Unity scene or UI test.
public static class FishTimingAttemptChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }

    public static void Main()
    {
        var attempt = new FishTimingAttempt();
        Check(attempt.State == FishTimingAttempt.AttemptState.Closed, "Initially closed");
        Check(!attempt.TryClaimCatch(), "Closed attempt awards nothing");
        attempt.Begin(10);
        Check(!attempt.TryClaimCatch(), "Running attempt awards nothing");
        attempt.Tick(1f, 10, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Running && attempt.Position == 0f, "Opening frame ignored");
        attempt.Tick(0.1f, 11, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Running, "Held opener cannot catch");
        attempt.Tick(0.9f, 12, false, false);
        Check(Math.Abs(attempt.Position - 0.65f) < 0.0001f, "Fixed movement speed");
        attempt.Tick(0.5f, 13, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Success, "Inside target succeeds using displayed position");
        Check(attempt.TryClaimCatch(), "Success awards one catch");
        Check(!attempt.TryClaimCatch(), "The same success cannot award twice");
        float stoppedPosition = attempt.Position;
        attempt.Tick(10f, 14, true, true);
        Check(attempt.Position == stoppedPosition && attempt.ResolvedFrame == 13, "Resolved attempt stops and ignores more catches");
        Check(!attempt.Retry(13), "Resolving input cannot also retry");
        Check(attempt.Retry(15), "Retry after result");
        attempt.Tick(1f, 15, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Running && attempt.Position == 0f, "Retry frame cannot resolve or move");
        attempt.Tick(0f, 16, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Running, "Retry requires Space release");
        attempt.Tick(0.1f, 17, false, false);
        attempt.Tick(0f, 18, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Failure, "Outside target fails");
        Check(!attempt.TryClaimCatch(), "Failure awards nothing");
        attempt.Tick(10f, 19, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Failure && attempt.ResolvedFrame == 18, "Failure resolves once");
        Check(attempt.Retry(20), "Retry after failure");
        Check(!attempt.Retry(21), "Cannot restart a running attempt through Retry");
        attempt.Cancel();
        attempt.Tick(10f, 22, true, true);
        Check(attempt.State == FishTimingAttempt.AttemptState.Closed && attempt.ResolvedFrame == -1, "Cancel awards no result");
        Check(!attempt.TryClaimCatch(), "Cancelled attempt awards nothing");
        attempt.Begin(30);
        attempt.Tick(2f, 31, false, false);
        Check(Math.Abs(attempt.Position - 0.7f) < 0.0001f, "Indicator bounces");
        attempt.Tick(100f, 32, false, false);
        Check(attempt.Position >= 0f && attempt.Position <= 1f, "Large frame delta stays in bar");
        Check(FishTimingAttempt.IsInsideTarget(0.6f) && FishTimingAttempt.IsInsideTarget(0.8f), "Target edges inclusive");
        Check(!FishTimingAttempt.IsInsideTarget(0.599f) && !FishTimingAttempt.IsInsideTarget(0.801f), "Outside edges fail");
        attempt.Begin(40);
        Check(attempt.Position == 0f && attempt.State == FishTimingAttempt.AttemptState.Running, "Reopen resets attempt");
        attempt.Tick(1f, 41, false, false);
        attempt.Tick(0f, 42, true, true);
        Check(attempt.TryClaimCatch(), "Another successful attempt can award another catch of the same species");
        Check(!attempt.TryClaimCatch(), "Second success still awards once");
        Check(attempt.Retry(43), "Retry after claimed success");
        attempt.Tick(1f, 44, false, false);
        attempt.Tick(0f, 45, true, true);
        attempt.Cancel();
        Check(!attempt.TryClaimCatch(), "Cancelled success cannot be claimed later");
        Console.WriteLine("Passed " + checks + " timing-attempt checks.");
    }
}
