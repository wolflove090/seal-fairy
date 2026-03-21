using UnityEngine;

public static class FairyDiscoveryLogger
{
    public static void LogDiscovered(FairyDefinition fairy, bool isNewDiscovery)
    {
        if(fairy == null)
            return;

        string name = string.IsNullOrWhiteSpace(fairy.DisplayName) ? fairy.Id : fairy.DisplayName;
        if(isNewDiscovery)
        {
            Debug.Log($"new {name}を発見！");
        }
        else
        {
            Debug.Log($"{name}を発見！");
        }
    }
}