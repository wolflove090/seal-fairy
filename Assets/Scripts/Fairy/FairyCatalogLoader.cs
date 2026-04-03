using System.Collections.Generic;
using UnityEngine;

public static class FairyCatalogLoader
{
    private const string CatalogResourcePath = "Fairy/fairy_catalog";

    public static List<FairyDefinition> Load()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(CatalogResourcePath);
        if(jsonAsset == null)
        {
            Debug.LogError($"Fairy catalog JSON not found. path={CatalogResourcePath}");
            return new List<FairyDefinition>();
        }

        FairyCatalogDto catalog;
        try
        {
            catalog = JsonUtility.FromJson<FairyCatalogDto>(jsonAsset.text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Fairy catalog JSON parse failed. path={CatalogResourcePath} error={ex.Message}");
            return new List<FairyDefinition>();
        }

        List<FairyDefinition> result = new();
        if(catalog?.fairies == null)
        {
            Debug.LogError("Fairy catalog JSON has no fairies array.");
            return result;
        }

        foreach(FairyRecordDto record in catalog.fairies)
        {
            if(!TryBuild(record, out FairyDefinition fairy))
            {
                continue;
            }

            result.Add(fairy);
        }

        return result;
    }

    private static bool TryBuild(FairyRecordDto record, out FairyDefinition fairy)
    {
        fairy = null;

        if (record == null)
        {
            Debug.LogWarning("Fairy record skipped because record is null.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(record.id))
        {
            Debug.LogWarning($"Fairy record skipped because id is empty. displayName={record.displayName}");
            return false;
        }

        if (record.weight <= 0)
        {
            Debug.LogWarning($"Fairy record skipped because weight is invalid. id={record.id} weight={record.weight}");
            return false;
        }

        Sprite icon = null;
        if(!string.IsNullOrWhiteSpace(record.iconResourcePath))
        {
            icon = Resources.Load<Sprite>(record.iconResourcePath);
            if(icon == null)
            {
                Debug.LogWarning($"Fairy icon load failed. id={record.id} path={record.iconResourcePath}");
            }
        }

        fairy = new FairyDefinition(
            record.id,
            record.displayName,
            record.weight,
            icon,
            record.favoriteStickerText,
            record.flavorText);
        return true;
    }
        
}