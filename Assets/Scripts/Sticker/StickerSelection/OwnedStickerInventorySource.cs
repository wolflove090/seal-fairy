using System.Collections.Generic;
using System;
using UnityEngine;

public sealed class OwnedStickerInventorySource : MonoBehaviour
{
    [Serializable]
    private sealed class OwnedStickerEntry
    {
        [SerializeField] private StickerDefinition sticker;
        [SerializeField] private int count = 1;

        public StickerDefinition Sticker
        {
            get => sticker;
            set => sticker = value;
        }

        public int Count
        {
            get => count;
            set => count = value;
        }
    }

    [SerializeField] private List<OwnedStickerEntry> ownedStickers = new();

    public event Action OwnedStickersChanged;

    public IReadOnlyList<StickerDefinition> GetOwnedStickers()
    {
        List<StickerDefinition> stickers = new(ownedStickers.Count);
        foreach (OwnedStickerEntry entry in ownedStickers)
        {
            if (entry?.Sticker == null || entry.Count <= 0)
            {
                continue;
            }

            stickers.Add(entry.Sticker);
        }

        return stickers;
    }

    public int GetOwnedStickerCount(StickerDefinition sticker)
    {
        OwnedStickerEntry entry = FindEntry(sticker);
        return entry != null ? entry.Count : 0;
    }

    public void AddOwnedStickerToFront(StickerDefinition sticker)
    {
        if (sticker == null)
        {
            return;
        }

        OwnedStickerEntry entry = FindEntry(sticker);
        if (entry != null)
        {
            ownedStickers.Remove(entry);
            entry.Count += 1;
            ownedStickers.Insert(0, entry);
        }
        else
        {
            ownedStickers.Insert(0, new OwnedStickerEntry
            {
                Sticker = sticker,
                Count = 1
            });
        }

        OwnedStickersChanged?.Invoke();
    }

    public bool RemoveOwnedSticker(StickerDefinition sticker)
    {
        if (sticker == null)
        {
            return false;
        }

        OwnedStickerEntry entry = FindEntry(sticker);
        if (entry == null)
        {
            return false;
        }

        entry.Count -= 1;
        if (entry.Count <= 0)
        {
            ownedStickers.Remove(entry);
        }

        OwnedStickersChanged?.Invoke();
        return true;
    }

    private OwnedStickerEntry FindEntry(StickerDefinition sticker)
    {
        foreach (OwnedStickerEntry entry in ownedStickers)
        {
            if (entry?.Sticker == sticker)
            {
                return entry;
            }
        }

        return null;
    }
}
