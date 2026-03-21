using UnityEngine;

[System.Serializable]
public sealed class OwnedStickerDefinition
{
    [SerializeField] private string id;
    [SerializeField] private Sprite icon;
    [SerializeField] private PeelSticker3D stickerPrefab;

    // 質問：プロパティ経由でアクセスさせるのは慣習？それとも明確な必要性がある？
    public string Id => id;
    public Sprite Icon => icon;
    public PeelSticker3D StickerPrefab => stickerPrefab;
}