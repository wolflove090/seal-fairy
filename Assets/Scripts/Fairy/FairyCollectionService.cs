using UnityEngine;

public static class FairyCollectionService
{
    private static FairyCollectionState state = new();

    // TODO：初期化処理はどこかのクラスにまとめたい。迷子になりそう
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        state = new FairyCollectionState();
    }

    public static bool TryRegisterDiscovery(FairyDefinition fairy, out bool isNewDiscovery)
    {
        isNewDiscovery = false;
        if(fairy == null || string.IsNullOrWhiteSpace(fairy.Id))
            return false;

        isNewDiscovery = state.TryAdd(fairy.Id);
        return true;
    }
}
