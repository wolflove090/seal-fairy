using UnityEngine;

[System.Serializable]
public sealed class StickerDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private PeelSticker3D stickerPrefab;
    [SerializeField] private int price = 100;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public PeelSticker3D StickerPrefab => stickerPrefab;
    public int Price => Mathf.Max(0, price);
}
