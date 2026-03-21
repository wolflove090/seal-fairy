public sealed class StickerFairyAssignment
{
    public FairyDefinition Fairy{get;}
    public string FairyId {get;}
    public bool HasFairy => Fairy != null && !string.IsNullOrWhiteSpace(FairyId);

    public StickerFairyAssignment(FairyDefinition fairy)
    {
        Fairy = fairy;
        FairyId = fairy != null ? fairy.Id : null;
    }
}