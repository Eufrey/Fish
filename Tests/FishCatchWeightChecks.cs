using System;
using Fish;
public static class FishCatchWeightChecks
{
    public static void Main()
    {
        int[] weights = {40,30,15,10,5}; int[] hits = new int[5];
        for(int ticket=0;ticket<100;ticket++) hits[FishCatchWeights.Choose(weights,ticket)]++;
        for(int i=0;i<5;i++) if(hits[i]!=weights[i]) throw new Exception("Weight interval mismatch");
        if(FishCatchWeights.Choose(weights,-1)!=-1 || FishCatchWeights.Choose(weights,100)!=-1) throw new Exception("Invalid tickets");
        if(FishCatchWeights.Choose(new[]{0,1,0},0)!=1) throw new Exception("Disabled entries");
        if(FishCatchWeights.Choose(new[]{100000,1},100000)!=1) throw new Exception("Smallest weight unreachable");
        if(FishCatchWeights.Choose(new[]{0,0},0)!=-1) throw new Exception("Empty pool");
        var attempt = new FishTimingAttempt(); attempt.Begin(0);
        attempt.Tick(.7f/FishTimingAttempt.IndicatorSpeed,1,false,false); attempt.Tick(0,2,true,true);
        if(!attempt.TryClaimCatch() || attempt.TryClaimCatch()) throw new Exception("Single success claim");
        attempt.Retry(3); attempt.Tick(0,4,false,false); attempt.Tick(0,5,true,true);
        if(attempt.TryClaimCatch()) throw new Exception("Failure award");
        attempt.Retry(6); attempt.Cancel(); if(attempt.TryClaimCatch()) throw new Exception("Cancel award");
        Console.WriteLine("Passed exhaustive 100-ticket distribution, reachability/disabled/boundary and success/failure/cancellation claim checks.");
    }
}
