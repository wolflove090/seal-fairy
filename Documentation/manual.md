# 妖精データJSONロード 作業手順書

## 目的
- 妖精マスタを `FairyCatalogSource` の Inspector 配列管理から JSON 管理へ切り替える。
- ゲーム開始時に妖精 JSON を読み込み、既存の抽選処理と妖精コレクション UI がそのデータを参照するようにする。
- 既存の `Main.unity` に入っている 3 件の妖精データを初期 JSON へ転記し、シーン側の二重管理を解消する。

## 変更対象
- [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs)
- [Assets/Scripts/Fairy/FairyCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogSource.cs)
- `Assets/Scripts/Fairy/FairyCatalogDto.cs`
- `Assets/Scripts/Fairy/FairyCatalogLoader.cs`
- `Assets/Scripts/Fairy/FairyCatalogRepository.cs`
- `Assets/GameResources/Resources/Fairy/fairy_catalog.json`
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
- `Assets/GameResources/Resources/Fairy/05.png`
- `Assets/GameResources/Resources/Fairy/07.png`
- `Assets/GameResources/Resources/Fairy/13.png`

## 手順1: 妖精画像を `Resources` 配下へ移す
1. `Assets/GameResources/Resources/Fairy/` を作成する。
2. [Assets/GameResources/Fairy/05.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/05.png)、[Assets/GameResources/Fairy/07.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/07.png)、[Assets/GameResources/Fairy/13.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/13.png) を `Assets/GameResources/Resources/Fairy/` へ移すか複製する。
3. JSON で使うパスは拡張子なしの `Fairy/05`、`Fairy/07`、`Fairy/13` に統一する。
4. Sprite Import 設定が維持されていることを Unity Editor で確認する。

## 手順2: JSON マスタを追加する
1. `Assets/GameResources/Resources/Fairy/fairy_catalog.json` を作成する。
2. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity#L2806) にある 3 件の妖精データを転記する。
3. `iconResourcePath` には `Resources.Load<Sprite>()` 用の相対パスを設定する。

### JSON 例
```json
{
  "fairies": [
    {
      "id": "1",
      "displayName": "バラちゃん",
      "weight": 1,
      "iconResourcePath": "Fairy/13",
      "favoriteStickerText": "自然やお花のシールが好きだぜ！",
      "flavorText": "なんか常にカッコつけている。\n皆からはほんのり嫌われている。"
    },
    {
      "id": "2",
      "displayName": "ウルフちゃん",
      "weight": 1,
      "iconResourcePath": "Fairy/05",
      "favoriteStickerText": "かっこよくて、もふもふなシールがあるといいなぁ",
      "flavorText": "実はとっても寂しがりや。\n怖がられると思っているから、遠くから見つめているよ。"
    },
    {
      "id": "3",
      "displayName": "もっさん",
      "weight": 1,
      "iconResourcePath": "Fairy/07",
      "favoriteStickerText": "お仲間の恐竜さん…。好き。",
      "flavorText": "とっても怠け者。\n口を大きくあけて、ごはんが入るのを待ってるよ。"
    }
  ]
}
```

## 手順3: JSON DTO を追加する
1. `Assets/Scripts/Fairy/FairyCatalogDto.cs` を作成する。
2. `JsonUtility` で配列を扱うため、カタログ全体 DTO と 1 件 DTO を定義する。

### 追加コード
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
    public int weight;
    public string iconResourcePath;
    public string favoriteStickerText;
    public string flavorText;
}
```

## 手順4: `FairyDefinition` をランタイム生成対応に変更する
1. [FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) を開く。
2. Inspector 用 serialized field 中心の形から、constructor で値を受けるランタイムモデルへ置き換える。
3. getter 名は既存利用側に合わせて維持する。

### 変更コード
```csharp
using UnityEngine;

public sealed class FairyDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public int Weight { get; }
    public Sprite Icon { get; }
    public string FavoriteStickerText { get; }
    public string FlavorText { get; }

    public FairyDefinition(
        string id,
        string displayName,
        int weight,
        Sprite icon,
        string favoriteStickerText,
        string flavorText)
    {
        Id = id;
        DisplayName = displayName;
        Weight = weight;
        Icon = icon;
        FavoriteStickerText = favoriteStickerText;
        FlavorText = flavorText;
    }
}
```

## 手順5: ローダーを追加する
1. `Assets/Scripts/Fairy/FairyCatalogLoader.cs` を作成する。
2. `Resources.Load<TextAsset>("Fairy/fairy_catalog")` で JSON を読み込む。
3. `JsonUtility.FromJson<FairyCatalogDto>()` で DTO へ変換する。
4. 各レコードを検証し、不正レコードはスキップしつつ対象が分かるログを出す。
5. `iconResourcePath` から `Resources.Load<Sprite>()` で画像を解決する。

### 追加コード
```csharp
using System.Collections.Generic;
using UnityEngine;

public static class FairyCatalogLoader
{
    private const string CatalogResourcePath = "Fairy/fairy_catalog";

    public static List<FairyDefinition> Load()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(CatalogResourcePath);
        if (jsonAsset == null)
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
        if (catalog?.fairies == null)
        {
            Debug.LogError("Fairy catalog JSON has no fairies array.");
            return result;
        }

        foreach (FairyRecordDto record in catalog.fairies)
        {
            if (!TryBuild(record, out FairyDefinition fairy))
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
        if (!string.IsNullOrWhiteSpace(record.iconResourcePath))
        {
            icon = Resources.Load<Sprite>(record.iconResourcePath);
            if (icon == null)
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
```

## 手順6: リポジトリを追加する
1. `Assets/Scripts/Fairy/FairyCatalogRepository.cs` を作成する。
2. 起動時に 1 回ロードし、以後はキャッシュを返す。
3. Play Mode 再実行時にキャッシュが残らないよう `SubsystemRegistration` でリセットする。

### 追加コード
```csharp
using System.Collections.Generic;
using UnityEngine;

public static class FairyCatalogRepository
{
    private static readonly List<FairyDefinition> fairies = new();
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        fairies.Clear();
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        Initialize();
    }

    public static IReadOnlyList<FairyDefinition> GetFairies()
    {
        if (!initialized)
        {
            Initialize();
        }

        return fairies;
    }

    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        fairies.Clear();
        fairies.AddRange(FairyCatalogLoader.Load());
    }
}
```

## 手順7: `FairyCatalogSource` を置き換える
1. [FairyCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogSource.cs) を開く。
2. Inspector 配列を削除する。
3. repository の値を返す薄いアダプタへ置き換える。

### 変更コード
```csharp
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FairyCatalogSource : MonoBehaviour
{
    public IReadOnlyList<FairyDefinition> GetFairies()
    {
        return FairyCatalogRepository.GetFairies();
    }
}
```

## 手順8: `Main.unity` の旧妖精配列を除去する
1. Unity Editor で [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
2. `FairyCatalogSource` を持つオブジェクトを選択する。
3. `fairies` 配列が出ている場合は削除する。
4. コンポーネント参照自体は残し、`HudScreenBinder` と `TapStickerPlacer` の参照切れを起こさない。
5. シーン保存後、YAML 上で旧妖精データが残っていないことを確認する。

## 手順9: 動作確認
1. Play Mode 起動直後に `FairyCatalogSource.GetFairies()` が 3 件返ることを確認する。
2. シールを配置し、妖精抽選が従来どおり動くことを確認する。
3. 妖精コレクション一覧で JSON 由来の画像と表示名が出ることを確認する。
4. 妖精詳細で JSON 由来の好きなシールとフレーバーが出ることを確認する。
5. `fairy_catalog.json` のファイル名を一時的に変え、`Debug.LogError` が出たうえでゲームが停止しないことを確認する。
6. JSON の 1 レコードだけ `weight: 0` にし、そのレコードだけスキップされ他は残ることを確認する。
7. `iconResourcePath` を壊し、その妖精だけ画像 null になるが UI と抽選が落ちないことを確認する。

## 完了条件
- 妖精マスタが JSON で管理されている。
- ゲーム開始時に妖精一覧がロードされる。
- `TapStickerPlacer` と `HudScreenBinder` が JSON 由来データを使っている。
- `Main.unity` に旧妖精配列が残っていない。
- ロード失敗時と不正レコード時のログが要件どおり出る。
