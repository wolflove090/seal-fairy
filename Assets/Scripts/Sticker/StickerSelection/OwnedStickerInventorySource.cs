using System.Collections.Generic;
using UnityEngine;

public sealed class OwnedStickerInventorySource : MonoBehaviour
{
    [SerializeField] private List<StickerDefinition> ownedStickers = new();

    public IReadOnlyList<StickerDefinition>
    GetOwnedStickers()
    {
        return ownedStickers;
    }
}