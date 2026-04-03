public sealed class FairyStickerPreference
{
    public readonly string StickerId;
    public readonly int Weight;
    
    public FairyStickerPreference(string stickerId, int weight)
    {
        StickerId = stickerId;
        Weight = weight;
    }
}