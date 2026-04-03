[System.Serializable]
public sealed class FairyCatalogDto
{
    public FairyRecordDto[] fairies;
}

[System.Serializable]
public sealed class FairyRecordDto
{
    public string id;
    public string displayName;
    public string iconResourcePath;
    public string favoriteStickerText;
    public string flavorText;
    public PreferredStickerDto[] preferredStickers;
}

[System.Serializable]
public sealed class PreferredStickerDto
{
    public string stickerId;
    public int weight;
}