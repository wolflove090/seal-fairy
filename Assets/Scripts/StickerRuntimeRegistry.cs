using System.Collections.Generic;

/// <summary>
/// ステッカー一覧を管理
/// </summary>
public static class StickerRuntimeRegistry
{
    private static readonly Dictionary<int, bool> fairyByStickerId = new();

    public static void Register(PeelSticker3D sticker, bool hasFairy)
    {
        if (sticker == null)
        {
            return;
        }

        int key = sticker.GetInstanceID();
        fairyByStickerId[key] = hasFairy;
    }

    public static bool TryConsumeFairy(PeelSticker3D sticker, out bool hasFairy)
    {
        hasFairy = false;
        if (sticker == null)
        {
            return false;
        }

        int key = sticker.GetInstanceID();
        if (!fairyByStickerId.TryGetValue(key, out hasFairy))
        {
            return false;
        }

        fairyByStickerId.Remove(key);
        return true;
    }
}
