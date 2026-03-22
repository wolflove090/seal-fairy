# シールショップ機能 実装計画

## 実装方針
- ショップ購入で所持シールを増やすため、所持データの更新責務を [OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs) に集約する。初期一覧取得専用のままにせず、追加 API と変更後一覧取得 API を持たせる。
- シール定義は既存の [StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs) をそのまま共通利用する。ショップ一覧と所持一覧で別定義を作らず、同一インスタンスをやり取りする。
- 所持シール一覧の再構築とショップ購入イベントの仲介は [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に集約する。購入時はショップ一覧更新ではなく、所持一覧更新を主処理にする。
- 初期所持 0 件を前提に、[StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) の未選択状態を壊さない。初回購入時も自動選択せず、ユーザーが一覧から選ぶまで `SelectedSticker` は `null` のままにする。
- 所持一覧の並び順は「新規購入シールを先頭へ追加」で統一する。既存選択がある場合は購入後も選択を維持する。
- 既存のショップ UI 資産は流用しつつ、カード押下時の `Debug.Log` のみだった処理を購入処理へ置き換える。画面は閉じず、続けて購入できる挙動を維持する。
- `Ready` ボタンによるフェーズ遷移は、所持シール 0 件でもそのまま許可する。配置処理側では `SelectedSticker == null` を既存どおり許容する。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs)
  - 初期所持 0 件前提のデータ管理へ変更する。
  - `AddOwnedStickerToFront(StickerDefinition sticker)` のような購入反映 API を追加する。
  - 内部リストを実行中に更新できる形へ整理する。
- [Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs)
  - 初期所持 0 件と購入後未選択を扱えるように現行挙動を確認し、必要なら初期選択適用条件を分離する。
  - 購入後の自動選択を行わない前提で責務を明確化する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - 初期所持 0 件時の一覧表示を維持したまま、購入後に所持一覧を再構築する処理を追加する。
  - ショップカード押下時に `OwnedStickerInventorySource` へ追加し、所持一覧 UI と選択状態を更新する。
  - 初回購入時に自動選択しないよう、既存の `hasAppliedInitialSelection` と `SelectInitialSticker()` の使い方を見直す。
  - ショップ画面と妖精コレクション画面の排他表示は維持する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `OwnedStickerInventorySource` の初期所持リストを 0 件にする。
  - `StickerShopCatalogSource` に販売シールを設定し、`HubScreenBinder` の参照が正しく割り当たる状態にする。
- [Documentation/manual.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/manual.md)
  - 実装手順とコードスニペットを今回の正しい変更対象へ更新する。

### 既存資産をそのまま利用する想定
- [Assets/Scripts/Sticker/StickerShop/StickerShopCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerShop/StickerShopCatalogSource.cs)
  - すでにショップ一覧取得 API があるため、基本構造は維持する。
- [Assets/Scripts/Sticker/StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs)
  - `DisplayName`、`Icon`、`StickerPrefab` がそろっており、今回の購入仕様でも追加フィールドなしで利用可能。
- [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml)
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
  - レイアウトは現行資産を流用し、必要最小限の文言調整のみ検討する。

## データフロー / 処理フロー
1. 起動時、[OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs) は空の `ownedStickers` を返す。
2. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `BuildStickerList()` は空一覧を受け取り、空表示ラベルを出しつつ `selectionState.SelectedSticker` を `null` のまま維持する。
3. プレイヤーが HUD の `ショップ` ボタンを押すと `OpenStickerShop()` が呼ばれ、ショップ一覧を表示する。
4. プレイヤーがショップカードを押すと `HandleStickerShopItemClicked(StickerDefinition item)` を呼ぶ。
5. `HandleStickerShopItemClicked` は `inventorySource.AddOwnedStickerToFront(item)` を実行し、購入成功ログを出力する。
6. 続けて `BuildStickerList()` を再実行し、所持一覧 UI を再構築する。
7. 再構築時、新規購入シールは一覧先頭に表示される。既存選択がない場合でも自動選択は行わない。
8. プレイヤーが所持一覧カードを押したときだけ `selectionState.Select(sticker)` を実行し、配置対象シールが決まる。
9. `TapStickerPlacer` は `SelectedSticker == null` の間は配置せず、選択後のみ配置可能になる。
10. `Ready` ボタン押下時は所持数に関係なく既存どおりフェーズ遷移する。

## 処理詳細

### 1. 所持データの可変化
- `OwnedStickerInventorySource` の `ownedStickers` は Inspector 設定値を実行時にも保持する内部リストとして扱う。
- 追加 API は null ガードを持たせ、無効な定義を追加しない。
- 先頭追加は `List.Insert(0, sticker)` を用いる。
- 外部公開は `IReadOnlyList<StickerDefinition>` のままとし、更新責務を `OwnedStickerInventorySource` のみに閉じる。

### 2. 初期選択制御の見直し
- 現状 `BuildStickerList()` は初回だけ `SelectInitialSticker()` を自動適用するため、今回の要件と衝突する。
- 対策として、「初期ロード時に所持シールがあれば選択する」処理を削除するか、明示的な条件分岐へ変更する。
- 今回の仕様ではシーン初期所持が 0 件のため、実質的には自動選択を行わない構成へ寄せる。
- 既存選択維持は `BuildStickerList()` 前後で `selectionState.SelectedSticker` を温存し、同一参照がまだ一覧にあればそのまま維持する。

### 3. ショップ購入処理
- ショップカード生成は `CreateStickerShopCard(StickerDefinition item)` を継続利用する。
- `card.clicked += () => HandleStickerShopItemClicked(item);` に変更し、ログ専用ラムダをやめる。
- `HandleStickerShopItemClicked` 内では以下のみを行う。
  - 購入対象の null チェック
  - 所持一覧への先頭追加
  - `Debug.Log($"ショップ購入: {displayName}")`
  - 所持一覧の再構築
- ショップ画面は閉じない。必要ならショップ一覧自体は再生成せず、そのまま維持する。

### 4. 所持一覧 UI 再構築
- `BuildStickerList()` は毎回 `selectionState.SetOwnedStickers()` を最新一覧で更新する。
- 一覧 0 件なら空表示ラベルを表示し、選択ハイライトは全解除する。
- 一覧ありならボタンを再生成し、`stickerCellByDefinition` の扱いが重複購入で破綻しないよう見直す。
- 現状の `Dictionary<StickerDefinition, VisualElement>` は同一定義の重複購入に対応できないため、購入仕様に合わせてキー構造を変更する必要がある。

### 5. 重複購入対応
- 同一 `StickerDefinition` を複数回所持できるため、`Dictionary<StickerDefinition, VisualElement>` ではカード追跡が上書きされる。
- 対応案は以下のいずれか。
  - `List<(StickerDefinition sticker, VisualElement cell)>` で UI 参照を保持する。
  - `Dictionary<VisualElement, StickerDefinition>` として視覚更新時に全走査する。
- 今回は一覧件数が小さい前提のため、単純な `List` ベースでの追跡が妥当。

## リスクと対策
- 重複購入時に所持一覧の選択ハイライト管理が崩れる可能性がある。
  - `Dictionary<StickerDefinition, VisualElement>` をやめ、重複を許容するデータ構造へ変更する。
- `BuildStickerList()` の自動初期選択ロジックが残ると、要件違反になる。
  - 自動選択の発火条件を整理し、購入後再構築では決して選択しないようにする。
- 初期所持 0 件でフェーズ遷移後、配置や復帰時に null 参照が起こる可能性がある。
  - [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) の `SelectedSticker == null` ガードが有効であることを前提に、関連処理を崩さない。
- シーン設定で `OwnedStickerInventorySource` に既存サンプルが残ると要件を満たさない。
  - `Main.unity` の Inspector 値を手動確認項目へ含める。

## 検証方針
- 手動確認1: 起動直後、HUD 左下に「所持シールがありません」が表示され、例外が出ないこと。
- 手動確認2: 所持 0 件のまま `Ready` を押してフェーズ遷移できること。
- 手動確認3: ショップ画面を開き、任意のシール購入で所持一覧先頭に新規カードが追加されること。
- 手動確認4: 初回購入直後は新規カードが自動選択されず、配置できないこと。
- 手動確認5: 所持一覧でそのカードを選択すると配置可能になること。
- 手動確認6: 別シールを選択中に購入しても、現在選択が維持されること。
- 手動確認7: 同じシールを複数回購入すると、同一シールカードが複数件先頭側へ並ぶこと。
- 手動確認8: 購入時に所持金減算が発生せず、Console に購入ログのみ出ること。
- 手動確認9: ショップ画面は購入後も閉じず、背景または `閉じる` でのみ閉じること。

## コードスニペット
```csharp
public sealed class OwnedStickerInventorySource : MonoBehaviour
{
    [SerializeField] private List<StickerDefinition> ownedStickers = new();

    public IReadOnlyList<StickerDefinition> GetOwnedStickers()
    {
        return ownedStickers;
    }

    public void AddOwnedStickerToFront(StickerDefinition sticker)
    {
        if (sticker == null)
        {
            return;
        }

        ownedStickers.Insert(0, sticker);
    }
}
```

```csharp
private void HandleStickerShopItemClicked(StickerDefinition item)
{
    if (item == null || inventorySource == null)
    {
        return;
    }

    inventorySource.AddOwnedStickerToFront(item);
    Debug.Log($"ショップ購入: {item.DisplayName}");
    BuildStickerList();
}
```

```csharp
private readonly List<(StickerDefinition sticker, VisualElement cell)> stickerCells = new();

private void RefreshSelectionVisuals()
{
    foreach ((StickerDefinition sticker, VisualElement cell) in stickerCells)
    {
        cell.EnableInClassList("sticker-cell--selected", selectionState.SelectedSticker == sticker);
    }
}
```
