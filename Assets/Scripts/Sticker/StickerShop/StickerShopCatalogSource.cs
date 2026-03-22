using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StickerShopCatalogSource : MonoBehaviour
{
    [SerializeField] private List<StickerDefinition> items = new();

    public IReadOnlyList<StickerDefinition> GetItems()
    {
        return items;
    }
}