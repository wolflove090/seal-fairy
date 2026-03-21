using System.Collections.Generic;

public sealed class StickerSelectionState
{
    public IReadOnlyList<OwnedStickerDefinition> OwnedStickers {get; private set;}
    public OwnedStickerDefinition SelectedSticker {get; private set;}

    public void SetOwnedStickers(IReadOnlyList<OwnedStickerDefinition> ownedStickers)
    {
        OwnedStickers = ownedStickers;
    }

    public void SelectInitialSticker()
    {
        SelectedSticker = OwnedStickers != null && OwnedStickers.Count > 0 ? OwnedStickers[0] : null;
    }

    public void Select(OwnedStickerDefinition sticker)
    {
        SelectedSticker = sticker;
    }

    public void ClearSelection()
    {
        SelectedSticker = null;
    }
}