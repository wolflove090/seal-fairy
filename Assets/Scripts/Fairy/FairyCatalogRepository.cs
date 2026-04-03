using System.Collections.Generic;
using UnityEngine;

public static class FairyCatalogRepository
{
    private static readonly List<FairyDefinition> fairies = new();
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        fairies.Clear();
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        Initialize();
    }

    public static IReadOnlyList<FairyDefinition> GetFairies()
    {
        if(!initialized)
        {
            Initialize();
        }

        return fairies;
    }

    public static void Initialize()
    {
        if(initialized)
        {
            return;
        }

        initialized = true;
        fairies.Clear();
        fairies.AddRange(FairyCatalogLoader.Load());
    }
}
