using System.Collections.Generic;
using UnityEngine;

public sealed class OwnedStickerInventorySource : MonoBehaviour
{
    [SerializeField] private List<OwnedStickerDefinition> ownedStickers = new();

    public IReadOnlyList<OwnedStickerDefinition>
    GetOwnedStickers()
    {
        return ownedStickers;
    }
}