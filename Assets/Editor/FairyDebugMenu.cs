using UnityEditor;
using UnityEngine;

public static class FairyDebugMenu
{
    private const string DiscoverAllMenuPath = "Debug/SealFairy/全ての妖精を発見済みにする";

    [MenuItem(DiscoverAllMenuPath)]
    private static void DiscoverAllFairies()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogWarning("全ての妖精を発見済みにするには Play Mode 中に実行してください。");
            return;
        }

        FairyCatalogSource catalogSource = Object.FindFirstObjectByType<FairyCatalogSource>();
        if(catalogSource == null)
        {
            Debug.LogWarning("FairyCatalogSource がシーン上に見つからないため、妖精を発見済みにできませんでした。");
            return;
        }

        var fairies = catalogSource.GetFairies();
        int registeredCount = FairyCollectionService.RegisterDiscoveries(fairies);
        int discoveredCount = FairyCollectionService.GetDiscoveredCount(fairies);
        int totalCount = fairies?.Count ?? 0;

        Debug.Log($"全ての妖精を発見済みにしました。今回追加: {registeredCount} 件 / 発見済み合計: {discoveredCount}/{totalCount}");
    }

    [MenuItem(DiscoverAllMenuPath, true)]
    private static bool ValidateDiscoverAllFairies()
    {
        return EditorApplication.isPlaying;
    }
}
