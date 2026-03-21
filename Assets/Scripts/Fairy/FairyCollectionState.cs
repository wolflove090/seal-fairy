using System.Collections.Generic;

public sealed class FairyCollectionState
{
    private readonly HashSet<string> discoveredFairyIds = new();

    public bool TryAdd(string fairyId)
    {
        return !string.IsNullOrWhiteSpace(fairyId) && discoveredFairyIds.Add(fairyId);
    }

    public bool Contains(string fairyId)
    {
        return !string.IsNullOrWhiteSpace(fairyId) && discoveredFairyIds.Contains(fairyId);
    }

    public void Clear()
    {
        discoveredFairyIds.Clear();
    }
}