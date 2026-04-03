# 妖精ごとのシール排出テーブル 作業手順書

## 目的
- 妖精ごとに「どのシールをどれだけ好むか」を設定できるようにする。
- ゲーム開始時に、全妖精の好み設定からシール別排出テーブルを構築する。
- 配置時は `50%` で妖精入りシールにし、一次抽選が空振りした場合だけ `50%` で救済抽選する。
- めくり時は、配置時に決めた妖精だけを参照し、再抽選しない。

## 変更対象
- [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs)
- [Assets/Scripts/Fairy/FairyCatalogDto.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogDto.cs)
- [Assets/Scripts/Fairy/FairyCatalogLoader.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogLoader.cs)
- `Assets/Scripts/Fairy/FairyStickerPreference.cs`
- `Assets/Scripts/Fairy/StickerFairySelector.cs`
- `Assets/Scripts/Fairy/StickerFairyTableRepository.cs`
- [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs)
- [Assets/GameResources/Resources/Fairy/fairy_catalog.json](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Resources/Fairy/fairy_catalog.json)
- [Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs)

## 手順1: 好みシールのランタイムモデルを追加する
1. `Assets/Scripts/Fairy/FairyStickerPreference.cs` を新規作成する。
2. 1 件の好み設定が `StickerId` と `Weight` を保持するだけの不変オブジェクトにする。

### 追加コード
```csharp
public sealed class FairyStickerPreference
{
    public string StickerId { get; }
    public int Weight { get; }

    public FairyStickerPreference(string stickerId, int weight)
    {
        StickerId = stickerId;
        Weight = weight;
    }
}
```

## 手順2: `FairyDefinition` を更新する
1. [FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) から `Weight` を削除する。
2. `IReadOnlyList<FairyStickerPreference>` を保持するように変更する。
3. constructor で好みシール一覧を受け取るようにする。

### 変更コード
```csharp
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class FairyDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public Sprite Icon { get; }
    public string FavoriteStickerText { get; }
    public string FlavorText { get; }
    public IReadOnlyList<FairyStickerPreference> PreferredStickers { get; }

    public FairyDefinition(
        string id,
        string displayName,
        Sprite icon,
        string favoriteStickerText,
        string flavorText,
        IReadOnlyList<FairyStickerPreference> preferredStickers)
    {
        Id = id;
        DisplayName = displayName;
        Icon = icon;
        FavoriteStickerText = favoriteStickerText;
        FlavorText = flavorText;
        PreferredStickers = preferredStickers;
    }
}
```

## 手順3: DTO を更新する
1. [FairyCatalogDto.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogDto.cs) の `weight` を削除する。
2. `PreferredStickerDto` を追加する。
3. `FairyRecordDto` に `preferredStickers` を持たせる。

### 変更コード
```csharp
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
```

## 手順4: ローダーで好み設定を組み立てる
1. [FairyCatalogLoader.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogLoader.cs) の `weight` 検証を削除する。
2. `preferredStickers` を `FairyStickerPreference` 一覧へ変換する処理を追加する。
3. 同一 `stickerId` が重複していたら、Dictionary で合算してから `FairyStickerPreference` 化する。
4. `weight <= 0` や空 `stickerId` はその要素だけスキップする。

### 変更イメージ
```csharp
private static IReadOnlyList<FairyStickerPreference> BuildPreferences(FairyRecordDto record)
{
    Dictionary<string, int> merged = new();
    if (record?.preferredStickers == null)
    {
        return new List<FairyStickerPreference>();
    }

    foreach (PreferredStickerDto dto in record.preferredStickers)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.stickerId) || dto.weight <= 0)
        {
            continue;
        }

        if (merged.TryGetValue(dto.stickerId, out int current))
        {
            merged[dto.stickerId] = current + dto.weight;
        }
        else
        {
            merged[dto.stickerId] = dto.weight;
        }
    }

    List<FairyStickerPreference> result = new(merged.Count);
    foreach ((string stickerId, int weight) in merged)
    {
        result.Add(new FairyStickerPreference(stickerId, weight));
    }

    return result;
}
```

### `TryBuild` の変更イメージ
```csharp
fairy = new FairyDefinition(
    record.id,
    record.displayName,
    icon,
    record.favoriteStickerText,
    record.flavorText,
    BuildPreferences(record));
```

## 手順5: JSON を更新する
1. [fairy_catalog.json](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Resources/Fairy/fairy_catalog.json) の各妖精レコードから `weight` を削除する。
2. `preferredStickers` を追加する。
3. `stickerId` は実際の `StickerDefinition.Id` と一致させる。

### JSON 例
```json
{
  "fairies": [
    {
      "id": "1",
      "displayName": "バラちゃん",
      "iconResourcePath": "Fairy/13",
      "favoriteStickerText": "自然やお花のシールが好きだぜ！",
      "flavorText": "なんか常にカッコつけている。\n皆からはほんのり嫌われている。",
      "preferredStickers": [
        { "stickerId": "watering-can", "weight": 7 },
        { "stickerId": "heart", "weight": 3 }
      ]
    },
    {
      "id": "2",
      "displayName": "ウルフちゃん",
      "iconResourcePath": "Fairy/05",
      "favoriteStickerText": "かっこよくて、もふもふなシールがあるといいなぁ",
      "flavorText": "実はとっても寂しがりや。\n怖がられると思っているから、遠くから見つめているよ。",
      "preferredStickers": [
        { "stickerId": "watering-can", "weight": 4 }
      ]
    }
  ]
}
```

## 手順6: シール別排出テーブルを事前構築する
1. `Assets/Scripts/Fairy/StickerFairyTableRepository.cs` を新規作成する。
2. ゲーム開始時に全妖精の `PreferredStickers` を走査し、`stickerId` ごとに `(fairy, weight)` を蓄積する。
3. `GetTable(string stickerId)` で配置時に参照できるようにする。

### 追加コード例
```csharp
using System.Collections.Generic;
using UnityEngine;

public static class StickerFairyTableRepository
{
    private static readonly Dictionary<string, List<(FairyDefinition fairy, int weight)>> tables = new();
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        tables.Clear();
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        Initialize();
    }

    public static IReadOnlyList<(FairyDefinition fairy, int weight)> GetTable(string stickerId)
    {
        if (!initialized)
        {
            Initialize();
        }

        return !string.IsNullOrWhiteSpace(stickerId) && tables.TryGetValue(stickerId, out List<(FairyDefinition fairy, int weight)> table)
            ? table
            : null;
    }

    private static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        tables.Clear();

        foreach (FairyDefinition fairy in FairyCatalogRepository.GetFairies())
        {
            if (fairy?.PreferredStickers == null)
            {
                continue;
            }

            foreach (FairyStickerPreference preference in fairy.PreferredStickers)
            {
                if (preference == null || string.IsNullOrWhiteSpace(preference.StickerId) || preference.Weight <= 0)
                {
                    continue;
                }

                if (!tables.TryGetValue(preference.StickerId, out List<(FairyDefinition fairy, int weight)> table))
                {
                    table = new List<(FairyDefinition fairy, int weight)>();
                    tables[preference.StickerId] = table;
                }

                table.Add((fairy, preference.Weight));
            }
        }
    }
}
```

## 手順7: シール別抽選セレクタを追加する
1. `Assets/Scripts/Fairy/StickerFairySelector.cs` を新規作成する。
2. `Select(string stickerId, IReadOnlyList<FairyDefinition> fairies)` を用意する。
3. `StickerFairyTableRepository.GetTable(stickerId)` から一次抽選テーブルを取得する。
4. 一次抽選では発見済み妖精も含めて抽選し、当選妖精が発見済みなら空振り扱いにする。
5. 一次抽選が候補なし、または発見済み当選で空振りなら、対象シールを好まない未発見妖精から救済候補を組み立てる。
6. 救済候補があり、かつ `Random.value < 0.5f` のときだけ救済当選させる。

### 追加コード例
```csharp
using System.Collections.Generic;
using UnityEngine;

public static class StickerFairySelector
{
    public static FairyDefinition Select(string stickerId, IReadOnlyList<FairyDefinition> fairies)
    {
        if (string.IsNullOrWhiteSpace(stickerId) || fairies == null || fairies.Count == 0)
        {
            return null;
        }

        List<FairyDefinition> fallback = new();
        IReadOnlyList<(FairyDefinition fairy, int weight)> primary = StickerFairyTableRepository.GetTable(stickerId);

        FairyDefinition selected = SelectWeighted(primary);
        if (selected != null && !FairyCollectionService.IsDiscovered(selected.Id))
        {
            return selected;
        }

        foreach (FairyDefinition fairy in fairies)
        {
            if (fairy == null || string.IsNullOrWhiteSpace(fairy.Id) || FairyCollectionService.IsDiscovered(fairy.Id))
            {
                continue;
            }

            if (!HasPreferenceForSticker(fairy, stickerId))
            {
                fallback.Add(fairy);
            }
        }

        if (fallback.Count == 0 || Random.value >= 0.5f)
        {
            return null;
        }

        int index = Random.Range(0, fallback.Count);
        return fallback[index];
    }

    private static FairyDefinition SelectWeighted(IReadOnlyList<(FairyDefinition fairy, int weight)> candidates)
    {
        int totalWeight = 0;
        foreach ((FairyDefinition _, int weight) in candidates)
        {
            totalWeight += weight;
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = Random.Range(1, totalWeight + 1);
        int accumulated = 0;
        foreach ((FairyDefinition fairy, int weight) in candidates)
        {
            accumulated += weight;
            if (roll <= accumulated)
            {
                return fairy;
            }
        }

        return null;
    }

    private static bool HasPreferenceForSticker(FairyDefinition fairy, string stickerId)
    {
        IReadOnlyList<FairyStickerPreference> preferences = fairy?.PreferredStickers;
        if (preferences == null)
        {
            return false;
        }

        foreach (FairyStickerPreference preference in preferences)
        {
            if (preference != null && preference.StickerId == stickerId && preference.Weight > 0)
            {
                return true;
            }
        }

        return false;
    }
}
```

## 手順8: `TapStickerPlacer` を差し替える
1. [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) の `SpawnSticker()` から `FairyWeightedRandomSelector.Select()` 呼び出しを削除する。
2. `selectedSticker.Id` を使って `StickerFairySelector.Select()` を呼ぶ。
3. その前段で `50%` の妖精入り判定を残す。

### 変更コード
```csharp
private void SpawnSticker(Vector3 worldPoint)
{
    PeelSticker3D sticker = Instantiate(templateSticker);
    sticker.name = "Peel Sticker";
    sticker.transform.SetPositionAndRotation(worldPoint, templateSticker.transform.rotation);
    sticker.transform.localScale = templateSticker.transform.localScale;
    sticker.gameObject.SetActive(true);
    sticker.PeelAmount = 0f;
    sticker.SetTapPeelEnabled(false);

    StickerFairyAssignment assignment = ResolveFairyAssignment(selectionState?.SelectedSticker);
    StickerRuntimeRegistry.Register(sticker, assignment);

    if (assignment != null && assignment.HasFairy)
    {
        AttachFairyEffect(sticker);
    }
}

private StickerFairyAssignment ResolveFairyAssignment(StickerDefinition selectedSticker)
{
    if (selectedSticker == null || string.IsNullOrWhiteSpace(selectedSticker.Id))
    {
        return null;
    }

    if (UnityEngine.Random.value >= 0.5f)
    {
        return null;
    }

    FairyDefinition fairy = StickerFairySelector.Select(
        selectedSticker.Id,
        fairyCatalogSource != null ? fairyCatalogSource.GetFairies() : null);

    return fairy != null ? new StickerFairyAssignment(fairy) : null;
}
```

## 手順9: 旧抽選コードを整理する
1. [FairyWeightedRandomSelector.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs) の参照がなくなったら削除する。
2. `FairyDefinition.Weight` を参照するコードが残っていないことを確認する。

## 手順10: 動作確認
1. ジョウロの `StickerDefinition.Id` を確認する。
2. JSON でジョウロに対し、妖精 A に `7`、妖精 B に `4` を設定する。
3. 未発見状態でジョウロを複数配置し、A と B が一次抽選レンジに入ることを確認する。
4. 片方を発見済みにした後、同じジョウロ配置でその妖精のレンジに当たると空振りになることを確認する。
5. 好み設定を持たないシール、または発見済み当選で空振りしたケースでだけ `50%` で救済抽選されることを確認する。
6. 救済抽選時、対象シールを好まない未発見妖精から等確率で選ばれることを確認する。
7. 剥がし時に、配置時に割り当てられた妖精だけが発見処理されることを確認する。
