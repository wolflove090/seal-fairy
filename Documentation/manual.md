# シールショップ機能 作業手順書

## 目的
- ゲーム開始時の所持シールを 0 件にする。
- ショップでシールカードを押したら、そのシールを所持一覧へ追加する。
- 購入時に所持金減算は行わない。
- 初回購入後も自動選択はせず、所持一覧から選ぶまで未選択のままにする。
- 重複購入を許可し、新規購入シールは所持一覧の先頭に追加する。

## 変更対象
- [Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs)
- [Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: 所持データへ購入追加 API を作る
1. [OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs) を開く。
2. `GetOwnedStickers()` は維持したまま、先頭追加メソッドを追加する。
3. 引数が `null` の場合は何もしない。
4. 追加順は `Insert(0, sticker)` にする。

### 実装コード
```csharp
using System.Collections.Generic;
using UnityEngine;

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

## 手順2: 選択状態の自動初期選択を見直す
1. [StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) を開く。
2. `SelectInitialSticker()` は残してもよいが、今回の実装では購入後や初回表示で呼ばない前提にする。
3. このファイル自体に変更を入れない場合も、[HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) 側で `SelectInitialSticker()` を使わないことを確認する。

### 現状維持コード
```csharp
public sealed class StickerSelectionState
{
    public IReadOnlyList<StickerDefinition> OwnedStickers { get; private set; }
    public StickerDefinition SelectedSticker { get; private set; }

    public void SetOwnedStickers(IReadOnlyList<StickerDefinition> ownedStickers)
    {
        OwnedStickers = ownedStickers;
    }

    public void SelectInitialSticker()
    {
        SelectedSticker = OwnedStickers != null && OwnedStickers.Count > 0 ? OwnedStickers[0] : null;
    }

    public void Select(StickerDefinition sticker)
    {
        SelectedSticker = sticker;
    }

    public void ClearSelection()
    {
        SelectedSticker = null;
    }
}
```

## 手順3: HudScreenBinder の所持一覧追跡構造を変更する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. `private readonly Dictionary<StickerDefinition, VisualElement> stickerCellByDefinition = new();` を削除する。
3. 重複購入対応のため、同じ `StickerDefinition` を複数持てる構造に置き換える。

### 置き換えコード
```csharp
private readonly List<(StickerDefinition sticker, VisualElement cell)> stickerCells = new();
```

## 手順4: BuildStickerList を購入仕様に合わせて書き換える
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `BuildStickerList()` を丸ごと差し替える。
2. 初期所持 0 件では空表示を出し、選択は未選択のままにする。
3. 一覧がある場合でも `SelectInitialSticker()` は呼ばない。
4. 再構築前に現在選択中の `StickerDefinition` を退避し、再構築後もまだ所持していれば維持する。
5. ただし未選択時はそのまま未選択を維持する。

### 差し替えコード
```csharp
private void BuildStickerList()
{
    stickerCells.Clear();
    stickerScrollView.Clear();

    IReadOnlyList<StickerDefinition> ownedStickers = inventorySource != null
        ? inventorySource.GetOwnedStickers()
        : null;

    StickerDefinition currentSelected = selectionState.SelectedSticker;
    selectionState.SetOwnedStickers(ownedStickers);

    if (ownedStickers == null || ownedStickers.Count == 0)
    {
        selectionState.ClearSelection();

        if (emptyStickerListLabel != null)
        {
            emptyStickerListLabel.style.display = DisplayStyle.Flex;
        }

        return;
    }

    if (emptyStickerListLabel != null)
    {
        emptyStickerListLabel.style.display = DisplayStyle.None;
    }

    foreach (StickerDefinition sticker in ownedStickers)
    {
        Button cell = CreateStickerCell(sticker);
        stickerCells.Add((sticker, cell));
        stickerScrollView.Add(cell);
    }

    bool stillOwned = false;
    foreach (StickerDefinition ownedSticker in ownedStickers)
    {
        if (ownedSticker == currentSelected)
        {
            stillOwned = true;
            break;
        }
    }

    if (currentSelected != null && stillOwned)
    {
        selectionState.Select(currentSelected);
    }
    else
    {
        selectionState.ClearSelection();
    }

    RefreshSelectionVisuals();
}
```

## 手順5: CreateStickerCell と RefreshSelectionVisuals を重複購入対応にする
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `RefreshSelectionVisuals()` を差し替える。
2. 同一 `StickerDefinition` を複数所持した場合、同じ定義のカードがすべて選択状態になる現状は許容しない。
3. 厳密に 1 枚だけ選択表示したい場合は、`SelectedSticker` だけでは識別できないため将来はインスタンス ID 管理が必要になる。
4. 今回は定義参照ベースで進める場合、同一シール複数所持時に複数ハイライトになるリスクを把握したうえで実装する。

### 最低限の差し替えコード
```csharp
private void RefreshSelectionVisuals()
{
    foreach ((StickerDefinition sticker, VisualElement cell) in stickerCells)
    {
        cell.EnableInClassList("sticker-cell--selected", selectionState.SelectedSticker == sticker);
    }
}
```

## 手順6: ショップカード押下を購入処理へ変更する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `HandleStickerShopItemClicked(StickerDefinition item)` を追加する。
2. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateStickerShopCard()` 内のクリック処理を差し替える。
3. 購入時は以下の順で処理する。
   - `item` と `inventorySource` を null チェック
   - `inventorySource.AddOwnedStickerToFront(item)`
   - `Debug.Log($"ショップ購入: {displayName}")`
   - `BuildStickerList()`
4. ショップ画面は閉じない。

### 追加コード
```csharp
private void HandleStickerShopItemClicked(StickerDefinition item)
{
    if (item == null || inventorySource == null)
    {
        return;
    }

    inventorySource.AddOwnedStickerToFront(item);

    string displayName = string.IsNullOrWhiteSpace(item.DisplayName)
        ? "名称未設定"
        : item.DisplayName;

    Debug.Log($"ショップ購入: {displayName}");
    BuildStickerList();
}
```

### 置き換えコード
```csharp
private Button CreateStickerShopCard(StickerDefinition item)
{
    Button card = new();
    card.AddToClassList("sticker-shop-card");

    VisualElement image = new();
    image.AddToClassList("sticker-shop-card__image");
    if (item != null && item.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(item.Icon.texture);
    }

    Label name = new();
    name.AddToClassList("sticker-shop-card__name");
    name.text = string.IsNullOrWhiteSpace(item?.DisplayName) ? "名称未設定" : item.DisplayName;

    card.Add(image);
    card.Add(name);
    card.clicked += () => HandleStickerShopItemClicked(item);
    return card;
}
```

## 手順7: Main.unity の初期値を変更する
1. Unity Editor で [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
2. `OwnedStickerInventorySource` を持つ GameObject を選択する。
3. `Owned Stickers` の配列サイズを `0` にする。
4. `StickerShopCatalogSource` を持つ GameObject を選択する。
5. `Items` に販売対象の `StickerDefinition` を設定する。
6. `HubScreenBinder` の `Inventory Source` と `Sticker Shop Catalog Source` と `Sticker Shop Screen Asset` が正しく割り当たっていることを確認する。

## 手順8: 手動確認を行う
1. 起動直後に HUD 左下へ `所持シールがありません` が表示されることを確認する。
2. 所持 0 件のまま `Ready` を押してフェーズ遷移できることを確認する。
3. `ショップ` を開き、任意のシールを購入すると所持一覧先頭に追加されることを確認する。
4. 初回購入直後は何も選択されず、画面タップしても配置されないことを確認する。
5. 所持一覧の購入済みシールを押すと選択状態になり、その後は配置できることを確認する。
6. 別シールを選択中に追加購入しても、選択中シールが変わらないことを確認する。
7. 同じシールを複数回購入すると、同じカードが複数件並ぶことを確認する。
8. Console に `ショップ購入: シール名` が出ることを確認する。
9. 購入時に所持金表示が変わらないことを確認する。
10. ショップ画面は購入後も開いたままで、`閉じる` または背景押下で閉じることを確認する。

## 注意点
- 現状の `StickerSelectionState` は `StickerDefinition` 参照で選択を保持しているため、同一定義を複数所持した場合の「どの 1 枚が選択されているか」の区別はできない。
- 今回の要件達成だけなら購入と配置は成立するが、見た目の選択ハイライトを厳密に 1 枚へ限定したい場合は、将来 `StickerDefinition` とは別に所持インスタンス ID が必要になる。
