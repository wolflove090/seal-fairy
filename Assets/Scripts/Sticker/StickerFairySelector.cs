using System.Collections.Generic;
using UnityEngine;

public static class StickerFairySelector
{
    // 妖精の選択処理
    public static FairyDefinition Select(string stickerId, IReadOnlyList<FairyDefinition> fairies)
    {
        if (string.IsNullOrWhiteSpace(stickerId) || fairies == null || fairies.Count == 0)
        {
            return null;
        }

        List<FairyDefinition> fallback = new();
        IReadOnlyList<(FairyDefinition fairy, int weight)> primary = StickerFairyTableRepository.GetTable(stickerId);

        // 重みからランダムで妖精をピックアップ
        FairyDefinition selected = SelectWeighted(primary);
        if (selected != null && !FairyCollectionService.IsDiscovered(selected.Id))
        {
            return selected;
        }    

        // 未発見の妖精を集約
        foreach(FairyDefinition fairy in fairies)
        {
            if (fairy == null || string.IsNullOrWhiteSpace(fairy.Id) || FairyCollectionService.IsDiscovered(fairy.Id))
            {
                continue;
            }

            fallback.Add(fairy);
        }

        // 50%の確率で未発見の内からランダムで妖精をピックアップ
        if (fallback.Count == 0 || Random.value >= 0.5f)
        {
            return null;
        }
        int index = Random.Range(0, fallback.Count);
        return fallback[index];
    }

    private static FairyDefinition SelectWeighted(IReadOnlyList<(FairyDefinition fairy, int weight)> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        // 重みを計算
        int totalWeight = 0;
        foreach((FairyDefinition _, int weight) in candidates)
        {
            totalWeight += weight;
        }

        if(totalWeight <= 0)
        {
            return null;
        }

        // ランダムで妖精を決定。重みが大きいほど当たりやすい
        int roll = Random.Range(1, totalWeight + 1);
        int accumulated = 0;
        foreach((FairyDefinition fairy, int weight) in candidates)
        {
            accumulated += weight;
            if(roll <= accumulated)
            {
                return fairy;
            }
        }

        return null;
    }
}