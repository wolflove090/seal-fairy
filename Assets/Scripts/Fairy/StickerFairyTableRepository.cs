using System.Collections.Generic;
using UnityEngine;

public static class StickerFairyTableRepository
{
    private static readonly Dictionary<string, List<(FairyDefinition fairy, int weight)>> tables = new();
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        tables.Clear();
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        Initialize();
    }

    public static IReadOnlyList<(FairyDefinition fairy, int weight)> GetTable(string stickerId)
    {
        if (!initialized)
        {
            Initialize();
        }

        return !string.IsNullOrWhiteSpace(stickerId) && tables.TryGetValue(stickerId, out List<(FairyDefinition fairy, int weight)> table)
            ? table
            : null;
    }

    private static void Initialize()
    {
        if(initialized)
        {
            return;
        }

        initialized = true;
        tables.Clear();

        foreach(FairyDefinition fairy in FairyCatalogRepository.GetFairies())
        {
            if(fairy?.PreferredStickers == null)
            {
                continue;
            }

            foreach(FairyStickerPreference preference in fairy.PreferredStickers)
            {
                if(preference == null || string.IsNullOrWhiteSpace(preference.StickerId) || preference.Weight <= 0)
                {
                    continue;
                }

                if(!tables.TryGetValue(preference.StickerId, out List<(FairyDefinition fairy, int weight)> table))
                {
                    table = new List<(FairyDefinition fairy, int weight)>();
                    tables[preference.StickerId] = table;
                }

                table.Add((fairy, preference.Weight));
            }
        }
    }

}