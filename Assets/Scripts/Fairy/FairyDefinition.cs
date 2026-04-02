using UnityEngine;

[System.Serializable]
public sealed class FairyDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int weight = 1;
    [SerializeField] private Sprite icon;
    [SerializeField] private string favoriteStickerText;
    [SerializeField, TextArea(3, 6)] private string flavorText;

    public string Id => id;
    public string DisplayName => displayName;
    public int Weight => weight;
    public Sprite Icon => icon;
    public string FavoriteStickerText => favoriteStickerText;
    public string FlavorText => flavorText;
}
