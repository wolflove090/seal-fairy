# 妖精コレクション機能 実装計画

## 実装方針
- 現状の妖精処理は [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) の配置時抽選、[StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs) のランタイム保持、[PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) の発見ログに分散している。今回の実装では責務を「妖精マスターデータ」「配置済みシールへの割当」「獲得状態管理」「ログ出力」に分離する。
- 妖精マスターデータは `MonoBehaviour` 配列で保持する。Inspector で登録できる一覧コンポーネントをシーンへ置き、配置処理はそこから `IReadOnlyList` を取得する。
- シール配置時は、登録済み妖精一覧から重み付きランダムで 1 体を選択し、そのシールの割当情報として `StickerRuntimeRegistry` に登録する。妖精一覧が 0 件、または有効 weight がない場合は妖精なしとして扱う。
- 既存の `KiraKiraEffect` は「妖精ありシール」共通の演出として継続利用し、妖精種別ごとの prefab 差し替えは行わない。
- 妖精獲得状態はセッション中のみメモリ保持する。将来的に `PlayerPrefs` へ置き換えやすいよう、獲得状態の読み書きは専用サービスを経由させる。
- 発見ログは固定文言を `PeelSticker3D` に埋め込まず、妖精名と新規/既発見フラグを受け取って出力する専用クラスへ切り出す。
- 既存のシール選択 UI やフェーズ UI は今回のスコープ外なので変更しない。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs)
  - `Random.value < 0.5f` を廃止し、妖精カタログからの重み付き抽選へ置き換える。
  - シーン上の妖精一覧供給元参照を受け取り、シール生成時に割当情報を `StickerRuntimeRegistry` へ登録する。
  - 妖精ありシールにのみ既存の `AttachFairyEffect()` を適用する。
- [Assets/Scripts/StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs)
  - `Dictionary<int, bool>` を、妖精割当情報を保持できる辞書へ変更する。
  - `TryConsumeFairy()` を、発見した妖精情報を返せる API に変更する。
- [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs)
  - 剥がし完了時に registry から妖精割当を消費する。
  - 発見時に獲得状態サービスを更新し、その結果に応じてログクラスを呼ぶ。
  - 妖精なしシールではログを出さない。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - 妖精カタログコンポーネントを追加し、`TapStickerPlacer` に参照を割り当てる。

### 新規作成予定
- `Assets/Scripts/Fairy/FairyDefinition.cs`
  - `id`、`displayName`、`weight`、将来 UI 用の参照を持つ妖精定義データ。
- `Assets/Scripts/Fairy/FairyCatalogSource.cs`
  - Inspector で複数妖精を登録する `MonoBehaviour`。
- `Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs`
  - 妖精一覧から重み付き抽選するユーティリティ。
- `Assets/Scripts/Fairy/StickerFairyAssignment.cs`
  - シール単位で保持する妖精割当情報。
- `Assets/Scripts/Fairy/FairyCollectionState.cs`
  - セッション中の獲得済み妖精 ID 群をメモリ保持する状態クラス。
- `Assets/Scripts/Fairy/FairyCollectionService.cs`
  - 獲得状態の読み書き窓口。将来の永続保存差し替え点。
- `Assets/Scripts/Fairy/FairyDiscoveryLogger.cs`
  - 妖精名と新規/既発見判定に応じたログ出力を行う。

## データフロー / 処理フロー
1. シーン開始時、`FairyCatalogSource` が Inspector 登録済みの妖精一覧を保持する。
2. `TapStickerPlacer` がシール配置時に `FairyCatalogSource` から妖精一覧を取得する。
3. `FairyWeightedRandomSelector` が有効な妖精定義だけを対象に総 weight を計算し、重み付きで 1 体を選ぶ。
4. 選ばれた妖精があれば `StickerFairyAssignment` を作成し、なければ妖精なしとして扱う。
5. `TapStickerPlacer` が `StickerRuntimeRegistry.Register(sticker, assignment)` を呼び、シール参照と割当情報を保存する。
6. `assignment` が妖精ありなら `KiraKiraEffect` をシールへ付与する。
7. `PeelSticker3D` が剥がし完了時に `StickerRuntimeRegistry.TryConsumeFairy()` を呼び、対象シールの割当情報を 1 回だけ取り出す。
8. 妖精割当が存在する場合、`FairyCollectionService` が獲得済み判定を更新し、新規発見かどうかを返す。
9. `FairyDiscoveryLogger` が妖精名付きのログを出力する。
10. フェーズ遷移でシールを全消去したときは `StickerRuntimeRegistry` だけをクリアし、獲得済み情報はセッション中維持する。

## 処理詳細

### 妖精定義データ
- `FairyDefinition` は `[System.Serializable]` なデータクラスとする。
- 持たせる項目は以下。
- `string id`
- `string displayName`
- `int weight`
- `Sprite icon` など将来 UI 表示で使う参照
- `weight <= 0`、`id` 空文字、null 要素は抽選対象外とする。

### 妖精カタログ供給
- `FairyCatalogSource` は `List<FairyDefinition>` を SerializeField で保持する。
- 公開 API は `IReadOnlyList<FairyDefinition> GetFairies()` とする。
- UI や他機能から再利用できるよう、抽選責務は持たせない。

### 重み付きランダム
- 抽選は総 weight を使った累積方式とする。
- 有効な妖精定義だけを集計対象にする。
- 総 weight が 0 の場合は `null` を返す。
- 同一妖精が複数回選ばれてよい。

### 配置済みシール割当
- `StickerRuntimeRegistry` は以下を保持する。
- `Dictionary<int, PeelSticker3D> stickerById`
- `Dictionary<int, StickerFairyAssignment> assignmentByStickerId`
- `StickerFairyAssignment` は少なくとも `FairyDefinition Fairy` と `string FairyId` を持つ。
- `TryConsumeFairy()` はシールごとの割当を一度だけ返し、返却後に辞書から除去する。
- 妖精なしシールは assignment 未登録、または `HasFairy == false` の割当として扱う。利用側分岐が単純な形を優先する。

### 獲得状態管理
- `FairyCollectionState` は `HashSet<string>` を用いて獲得済み妖精 ID を保持する。
- `FairyCollectionService` は `TryRegisterDiscovery(FairyDefinition fairy, out bool isNewDiscovery)` のような API を持つ。
- 新規獲得時のみ ID を追加し、再発見時は状態を変えない。
- 起動中のみ状態を保持し、Play 再開始時は初期化されるよう `RuntimeInitializeOnLoadMethod` を使う。
- 将来的に `PlayerPrefs` を導入する場合も、このサービスだけ差し替えれば済むようにする。

### 発見ログ
- `FairyDiscoveryLogger` は `LogDiscovered(FairyDefinition fairy, bool isNewDiscovery)` を持つ。
- ログには必ず妖精名を含める。
- 新規発見と既発見で Console 上の文面を明確に分ける。
- 最終文言は実装時に調整するが、判別可能性を落とさない。

### フェーズ遷移との整合
- [SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) の `ClearRemainingStickers()` は現状どおり `StickerRuntimeRegistry.ClearAll()` を呼ぶ。
- ここでは配置済みシールとその妖精割当だけを破棄し、獲得済み情報は消さない。
- 全リセット機能が必要になった場合のみ、獲得状態クリア API を追加する。

## リスクと対策
- `TapStickerPlacer` に抽選、割当生成、演出付与を詰め込みすぎると保守性が落ちる。
  - 重み付き抽選と獲得状態管理は別クラス化し、`TapStickerPlacer` は配置時の組み立て役に留める。
- 妖精データ設定ミスで `id` 空文字や `weight=0` が混ざる可能性がある。
  - 無効データは抽選対象外にし、クラッシュや例外停止を防ぐ。
- `StickerRuntimeRegistry` の API 変更で既存フェーズ処理が壊れる可能性がある。
  - `GetActiveStickers()` と `ClearAll()` の契約は維持し、変更は妖精関連 API に閉じる。
- セッション状態を static で持つ場合、Play 再実行時の残留が起こりうる。
  - 起動初期化フックで状態を再生成する。

## 検証方針
- 手動確認1:
  - Inspector から複数妖精を登録でき、各妖精に重みを設定できることを確認する。
- 手動確認2:
  - 妖精 0 件の状態でシール配置と剥がしを行っても例外が出ず、妖精関連ログも出ないことを確認する。
- 手動確認3:
  - 重みを変えた複数妖精を登録し、多数回の配置で重い妖精ほど出やすいことを概観確認する。
- 手動確認4:
  - 初めて見つけた妖精では新規発見ログが出ることを確認する。
- 手動確認5:
  - 同じ妖精を再度見つけた場合は既発見ログに切り替わることを確認する。
- 手動確認6:
  - 妖精ありシールでは既存の `KiraKiraEffect` が付き、妖精種別に関係なく共通演出であることを確認する。
- 手動確認7:
  - フェーズ往復で配置済みシールは消えても、同一セッション中の獲得済み判定が維持されることを確認する。

## コードスニペット
```csharp
[System.Serializable]
public sealed class FairyDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int weight = 1;

    public string Id => id;
    public string DisplayName => displayName;
    public int Weight => weight;
}
```

```csharp
public static class FairyWeightedRandomSelector
{
    public static FairyDefinition Select(IReadOnlyList<FairyDefinition> fairies)
    {
        int totalWeight = 0;
        foreach (FairyDefinition fairy in fairies)
        {
            if (fairy == null || string.IsNullOrWhiteSpace(fairy.Id) || fairy.Weight <= 0)
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
            if (fairy == null || string.IsNullOrWhiteSpace(fairy.Id) || fairy.Weight <= 0)
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
}
```

```csharp
if (StickerRuntimeRegistry.TryConsumeFairy(this, out StickerFairyAssignment assignment)
    && assignment != null
    && assignment.HasFairy
    && FairyCollectionService.TryRegisterDiscovery(assignment.Fairy, out bool isNewDiscovery))
{
    FairyDiscoveryLogger.LogDiscovered(assignment.Fairy, isNewDiscovery);
}
```
