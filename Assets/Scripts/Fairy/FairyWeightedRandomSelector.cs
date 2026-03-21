using System.Collections.Generic;
using UnityEngine;

public static class FairyWeightedRandomSelector
{
    public static FairyDefinition Select(IReadOnlyList<FairyDefinition> fairies)
    {
        if(fairies == null || fairies.Count == 0)
            return null;

        // 合計の重みを計算
        int totalWeight = 0;
        foreach(FairyDefinition fairy in fairies)
        {
            if(!IsSelectable(fairy))
                continue;

            totalWeight += fairy.Weight;
        }

        if(totalWeight <= 0)
            return null;

        int roll = Random.Range(0, totalWeight);
        int accumulated = 0;
        foreach(FairyDefinition fairy in fairies)
        {
            if(!IsSelectable(fairy))
                continue;

            accumulated += fairy.Weight;
            if(roll < accumulated)
                return fairy;
        }

        return null;
    }

    private static bool IsSelectable(FairyDefinition fairy)
    {
        return fairy != null && !string.IsNullOrWhiteSpace(fairy.Id) && fairy.Weight > 0;
    }
}