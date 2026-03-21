# 妖精コレクション機能 作業手順書

## 目的
- 複数の妖精をデータ登録できるようにする。
- シール配置時に、登録済み妖精から重み付きランダムで 1 体を割り当てる。
- 妖精発見時にセッション中の獲得状態を更新し、新規発見か既発見かをログで判別できるようにする。
- 永続保存は実装せず、将来 `PlayerPrefs` へ差し替えやすい構造を作る。

## 作成・更新するもの
- `Assets/Scripts/Fairy/FairyDefinition.cs`
- `Assets/Scripts/Fairy/FairyCatalogSource.cs`
- `Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs`
- `Assets/Scripts/Fairy/StickerFairyAssignment.cs`
- `Assets/Scripts/Fairy/FairyCollectionState.cs`
- `Assets/Scripts/Fairy/FairyCollectionService.cs`
- `Assets/Scripts/Fairy/FairyDiscoveryLogger.cs`
- [Assets/Scripts/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs)
- [Assets/Scripts/StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs)
- [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: 妖精用フォルダを作成
1. `Assets/Scripts` 配下に `Fairy` フォルダを作成する。
2. 今回追加する妖精関連クラスはすべてこのフォルダに置く。

## 手順2: 妖精定義データを作成
1. `Assets/Scripts/Fairy/FairyDefinition.cs` を作成する。
2. 内容は以下をベースにする。

```csharp
using UnityEngine;

[System.Serializable]
public sealed class FairyDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int weight = 1;
    [SerializeField] private Sprite icon;

    public string Id => id;
    public string DisplayName => displayName;
    public int Weight => weight;
    public Sprite Icon => icon;
}
```

3. `icon` は今回未使用でも残してよい。将来のコレクション UI 用の参照枠として扱う。
4. `id` は実行時識別子なので、配列 index を識別子代わりに使わない。

## 手順3: 妖精カタログ供給元を作成
1. `Assets/Scripts/Fairy/FairyCatalogSource.cs` を作成する。
2. Inspector で複数妖精を設定できる `MonoBehaviour` にする。

```csharp
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FairyCatalogSource : MonoBehaviour
{
    [SerializeField] private List<FairyDefinition> fairies = new();

    public IReadOnlyList<FairyDefinition> GetFairies()
    {
        return fairies;
    }
}
```

3. 抽選ロジックはここに書かない。

## 手順4: 重み付き抽選クラスを作成
1. `Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs` を作成する。
2. 以下をベースに実装する。

```csharp
using System.Collections.Generic;
using UnityEngine;

public static class FairyWeightedRandomSelector
{
    public static FairyDefinition Select(IReadOnlyList<FairyDefinition> fairies)
    {
        if (fairies == null || fairies.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        foreach (FairyDefinition fairy in fairies)
        {
            if (!IsSelectable(fairy))
            {
                continue;
            }

            totalWeight += fairy.Weight;
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        int accumulated = 0;
        foreach (FairyDefinition fairy in fairies)
        {
            if (!IsSelectable(fairy))
            {
                continue;
            }

            accumulated += fairy.Weight;
            if (roll < accumulated)
            {
                return fairy;
            }
        }

        return null;
    }

    private static bool IsSelectable(FairyDefinition fairy)
    {
        return fairy != null
            && !string.IsNullOrWhiteSpace(fairy.Id)
            && fairy.Weight > 0;
    }
}
```

3. `weight <= 0`、null、`id` 空文字は抽選対象外にする。
4. 総 weight が 0 のときは `null` を返し、妖精なし扱いにする。

## 手順5: シール割当情報を作成
1. `Assets/Scripts/Fairy/StickerFairyAssignment.cs` を作成する。
2. シールに割り当てた妖精情報をまとめる。

```csharp
public sealed class StickerFairyAssignment
{
    public StickerFairyAssignment(FairyDefinition fairy)
    {
        Fairy = fairy;
        FairyId = fairy != null ? fairy.Id : null;
    }

    public FairyDefinition Fairy { get; }
    public string FairyId { get; }
    public bool HasFairy => Fairy != null && !string.IsNullOrWhiteSpace(FairyId);
}
```

## 手順6: 獲得状態管理を作成
1. `Assets/Scripts/Fairy/FairyCollectionState.cs` を作成する。

```csharp
using System.Collections.Generic;

public sealed class FairyCollectionState
{
    private readonly HashSet<string> discoveredFairyIds = new();

    public bool TryAdd(string fairyId)
    {
        return !string.IsNullOrWhiteSpace(fairyId) && discoveredFairyIds.Add(fairyId);
    }

    public bool Contains(string fairyId)
    {
        return !string.IsNullOrWhiteSpace(fairyId) && discoveredFairyIds.Contains(fairyId);
    }

    public void Clear()
    {
        discoveredFairyIds.Clear();
    }
}
```

2. `Assets/Scripts/Fairy/FairyCollectionService.cs` を作成する。

```csharp
using UnityEngine;

public static class FairyCollectionService
{
    private static FairyCollectionState state = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        state = new FairyCollectionState();
    }

    public static bool TryRegisterDiscovery(FairyDefinition fairy, out bool isNewDiscovery)
    {
        isNewDiscovery = false;
        if (fairy == null || string.IsNullOrWhiteSpace(fairy.Id))
        {
            return false;
        }

        isNewDiscovery = state.TryAdd(fairy.Id);
        return true;
    }
}
```

3. 今回はメモリ保持のみとし、`PlayerPrefs` の書き込みは入れない。

## 手順7: 発見ログクラスを作成
1. `Assets/Scripts/Fairy/FairyDiscoveryLogger.cs` を作成する。
2. ログ文言は実装時に微調整してよいが、妖精名と新規/既発見の区別を必須とする。

```csharp
using UnityEngine;

public static class FairyDiscoveryLogger
{
    public static void LogDiscovered(FairyDefinition fairy, bool isNewDiscovery)
    {
        if (fairy == null)
        {
            return;
        }

        string name = string.IsNullOrWhiteSpace(fairy.DisplayName) ? fairy.Id : fairy.DisplayName;
        if (isNewDiscovery)
        {
            Debug.Log($"新しい妖精を発見: {name}");
            return;
        }

        Debug.Log($"既に発見済みの妖精: {name}");
    }
}
```

## 手順8: StickerRuntimeRegistry を更新
1. [StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs) を開く。
2. `fairyByStickerId` を `assignmentByStickerId` に置き換える。
3. 目標形は以下。

```csharp
using System.Collections.Generic;

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

        if (assignment != null && assignment.HasFairy)
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
}
```

4. `UnRegister()` と `ClearAll()` でも `assignmentByStickerId` を確実に消す。
5. `GetActiveStickers()` は既存契約を維持する。

## 手順9: TapStickerPlacer を更新
1. [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) に妖精カタログ参照を追加する。

```csharp
[SerializeField] private FairyCatalogSource fairyCatalogSource;
```

2. `SpawnSticker()` 内の `bool hasFairy = Random.value < 0.5f;` を削除する。
3. 以下の流れへ置き換える。
- 妖精一覧取得
- 重み付き抽選
- `StickerFairyAssignment` 作成
- registry 登録
- 妖精ありならエフェクト付与

4. 例:

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

    FairyDefinition selectedFairy = FairyWeightedRandomSelector.Select(
        fairyCatalogSource != null ? fairyCatalogSource.GetFairies() : null);

    StickerFairyAssignment assignment = selectedFairy != null
        ? new StickerFairyAssignment(selectedFairy)
        : null;

    StickerRuntimeRegistry.Register(sticker, assignment);

    if (assignment != null && assignment.HasFairy)
    {
        AttachFairyEffect(sticker);
    }
}
```

5. 妖精 0 件時は `selectedFairy == null` なので、配置は継続し、妖精ログも出ない。

## 手順10: PeelSticker3D を更新
1. [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) の `CompletePeel()` を修正する。
2. 固定の `Debug.Log("妖精を発見！")` を削除する。
3. registry、獲得状態サービス、ログクラスを使う形にする。

```csharp
private void CompletePeel()
{
    if (isPeelComplete)
    {
        return;
    }

    isPeelComplete = true;
    isAutoPeeling = false;

    if (StickerRuntimeRegistry.TryConsumeFairy(this, out StickerFairyAssignment assignment)
        && assignment != null
        && assignment.HasFairy
        && FairyCollectionService.TryRegisterDiscovery(assignment.Fairy, out bool isNewDiscovery))
    {
        FairyDiscoveryLogger.LogDiscovered(assignment.Fairy, isNewDiscovery);
    }

    Destroy(gameObject, 0.5f);
}
```

4. 妖精なしシールではロガーを呼ばない。

## 手順11: Main.unity を設定
1. [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
2. `FairyCatalog` などの名前で空 GameObject を追加する。
3. `FairyCatalogSource` をアタッチする。
4. Inspector で複数妖精を登録する。
5. 例:
- `id = fairy_blue`
- `displayName = 青い妖精`
- `weight = 60`
- `id = fairy_gold`
- `displayName = 金の妖精`
- `weight = 10`

6. `TapStickerPlacer` の `fairyCatalogSource` にそのコンポーネントを割り当てる。

## 手順12: 動作確認
1. Play モードで複数妖精を登録した状態でシールを配置し、剥がした時に妖精名付きログが出ることを確認する。
2. 同じ妖精を再度見つけた時、ログが既発見向け文言に変わることを確認する。
3. `FairyCatalogSource` を空配列にしても、配置と剥がしが例外なく動作し、妖精ログが出ないことを確認する。
4. `weight` を極端に変えた妖精を登録し、重い妖精が出やすいことを概観確認する。
5. フェーズを往復した後も、同じセッション中なら既発見判定が維持されることを確認する。

## 完了条件
- 妖精データを Inspector から複数登録できる。
- シールごとに妖精割当情報を保持できる。
- 発見ログが新規/既発見で分岐する。
- 妖精 0 件時もゲーム進行が止まらない。
- 獲得状態管理の差し替え窓口が分離されている。
