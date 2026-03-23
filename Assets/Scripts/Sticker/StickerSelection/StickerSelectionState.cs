using System;
using System.Collections.Generic;

public sealed class StickerSelectionState
{
    public event Action<StickerDefinition> SelectedStickerChanged;

    public IReadOnlyList<StickerDefinition> OwnedStickers {get; private set;}
    public StickerDefinition SelectedSticker {get; private set;}

    public void SetOwnedStickers(IReadOnlyList<StickerDefinition> ownedStickers)
    {
        OwnedStickers = ownedStickers;
    }

    public void SelectInitialSticker()
    {
        Select(OwnedStickers != null && OwnedStickers.Count > 0 ? OwnedStickers[0] : null);
    }

    public void Select(StickerDefinition sticker)
    {
        if (SelectedSticker == sticker)
        {
            return;
        }

        SelectedSticker = sticker;
        SelectedStickerChanged?.Invoke(SelectedSticker);
    }

    public void ClearSelection()
    {
        if (SelectedSticker == null)
        {
            return;
        }

        SelectedSticker = null;
        SelectedStickerChanged?.Invoke(null);
    }
}
