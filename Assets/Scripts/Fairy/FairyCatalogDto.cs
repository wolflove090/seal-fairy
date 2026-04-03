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
    public int weight;
    public string iconResourcePath;
    public string favoriteStickerText;
    public string flavorText;
}