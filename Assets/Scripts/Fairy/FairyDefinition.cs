using UnityEngine;

[System.Serializable]
public sealed class FairyDefinition
{
    public readonly string Id;
    public readonly string DisplayName;
    public readonly int Weight;
    public readonly Sprite Icon;
    public readonly string FavoriteStickerText;
    public readonly string FlavorText;

    public FairyDefinition(
        string id,
        string displayName,
        int weight,
        Sprite icon,
        string favoriteStickerText,
        string flavorText)
    {
        Id = id;
        DisplayName = displayName;
        Weight = weight;
        Icon = icon;
        FavoriteStickerText = favoriteStickerText;
        FlavorText = flavorText;
    }

}
