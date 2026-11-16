using System.Collections.Generic;
namespace Fish
{
    public static class FishCatchWeights
    {
        public static int Choose(IReadOnlyList<int> weights, int ticket)
        {
            if (weights == null || ticket < 0) return -1;
            for (int i = 0; i < weights.Count; i++)
            {
                int weight = weights[i];
                if (weight <= 0) continue;
                if (ticket < weight) return i;
                ticket -= weight;
            }
            return -1;
        }
    }
}
