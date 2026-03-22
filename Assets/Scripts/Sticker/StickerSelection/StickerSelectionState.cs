using System.Collections.Generic;

public sealed class StickerSelectionState
{
    public IReadOnlyList<StickerDefinition> OwnedStickers {get; private set;}
    public StickerDefinition SelectedSticker {get; private set;}

    public void SetOwnedStickers(IReadOnlyList<StickerDefinition> ownedStickers)
    {
        OwnedStickers = ownedStickers;
    }

    public void SelectInitialSticker()
    {
        SelectedSticker = OwnedStickers != null && OwnedStickers.Count > 0 ? OwnedStickers[0] : null;
    }

    public void Select(StickerDefinition sticker)
    {
        SelectedSticker = sticker;
    }

    public void ClearSelection()
    {
        SelectedSticker = null;
    }
}