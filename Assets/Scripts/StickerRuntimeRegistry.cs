using System.Collections.Generic;

/// <summary>
/// ステッカー一覧を管理
/// </summary>
public static class StickerRuntimeRegistry
{
    private static readonly Dictionary<int, StickerFairyAssignment> assignmentByStickerId = new();
    private static readonly Dictionary<int, PeelSticker3D> stickerById = new();

    public static void Register(PeelSticker3D sticker, StickerFairyAssignment assignment)
    {
        if (sticker == null)
        {
            return;
        }

        int key = sticker.GetInstanceID();
        stickerById[key] = sticker;

        if(assignment != null && assignment.HasFairy)
        {
            assignmentByStickerId[key] = assignment;
            return;
        }

        assignmentByStickerId.Remove(key);
    }

    public static bool TryConsumeFairy(PeelSticker3D sticker, out StickerFairyAssignment assignment)
    {
        assignment = null;
        if (sticker == null)
        {
            return false;
        }

        int key = sticker.GetInstanceID();
        if (!assignmentByStickerId.TryGetValue(key, out assignment))
        {
            return false;
        }

        assignmentByStickerId.Remove(key);
        return assignment != null && assignment.HasFairy;
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
        assignmentByStickerId.Remove(key);
        stickerById.Remove(key);
    }

    public static void ClearAll()
    {
        assignmentByStickerId.Clear();
        stickerById.Clear();
    }
}
