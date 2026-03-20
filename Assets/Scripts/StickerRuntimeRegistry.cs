using System.Collections.Generic;

/// <summary>
/// ステッカー一覧を管理
/// </summary>
public static class StickerRuntimeRegistry
{
    private static readonly Dictionary<int, bool> fairyByStickerId = new();
    private static readonly Dictionary<int, PeelSticker3D> stickerById = new();

    public static void Register(PeelSticker3D sticker, bool hasFairy)
    {
        if (sticker == null)
        {
            return;
        }

        int key = sticker.GetInstanceID();
        fairyByStickerId[key] = hasFairy;
        stickerById[key] = sticker;
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

    public static IReadOnlyCollection<PeelSticker3D> GetActiveStickers()
    {
        List<PeelSticker3D> results = new();
        foreach (PeelSticker3D sticker in stickerById.Values)
        {
            if (sticker != null)
            {
                results.Add(sticker);
            }
        }

        return results;
    }

    public static void UnRegister(PeelSticker3D sticker)
    {
        if (sticker == null)
        {
            return;
        }

        int key = sticker.GetInstanceID();
        fairyByStickerId.Remove(key);
        stickerById.Remove(key);
    }

    public static void ClearAll()
    {
        fairyByStickerId.Clear();
        stickerById.Clear();
    }
}
