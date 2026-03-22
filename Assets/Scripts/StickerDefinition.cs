using UnityEngine;

[System.Serializable]
public sealed class StickerDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private PeelSticker3D stickerPrefab;

    // 質問：プロパティ経由でアクセスさせるのは慣習？それとも明確な必要性がある？
    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public PeelSticker3D StickerPrefab => stickerPrefab;
}