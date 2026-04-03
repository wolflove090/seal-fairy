using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class FairyDefinition
{
    public readonly string Id;
    public readonly string DisplayName;
    public readonly Sprite Icon;
    public readonly string FavoriteStickerText;
    public readonly string FlavorText;
    public readonly IReadOnlyList<FairyStickerPreference> PreferredStickers;

public FairyDefinition(
        string id,
        string displayName,
        Sprite icon,
        string favoriteStickerText,
        string flavorText,
        IReadOnlyList<FairyStickerPreference> preferredStickers)
    {
        Id = id;
        DisplayName = displayName;
        Icon = icon;
        FavoriteStickerText = favoriteStickerText;
        FlavorText = flavorText;
        PreferredStickers = preferredStickers;
    }

}
