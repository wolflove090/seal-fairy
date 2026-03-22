# シールショップ機能 作業手順書

## 目的
- 最初に `OwnedStickerDefinition` のリファクタリングを行い、保持シールとショップ表示で共通利用できる基盤を整える。
- HUD の `ショップ` ボタンから、右側オーバーレイ形式のシールショップ画面を開けるようにする。
- ショップ画面にはシールを 3 列基準のスクロール一覧で表示する。
- 各シールをタップした時は購入処理を行わず、シール名を `Debug.Log` に出力する。
- UI の開閉挙動と見た目は既存の妖精コレクション UI を踏襲する。

## 変更対象
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs)
- [Assets/Scripts/StickerShop/StickerShopCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerShop/StickerShopCatalogSource.cs)
- [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml)
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: 共通シール定義とショップカタログを整備する
1. `Assets/Scripts/StickerShop/` フォルダを作成する。
2. [Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs) を開く。
3. 最初の作業として、このファイルを今回のリファクタ対象にする。
4. 既存の `id`、`icon`、`stickerPrefab` に加えて `displayName` を追加する。
5. 既存参照を壊さないよう、読み取り専用プロパティ `DisplayName` を追加する。
6. `OwnedStickerInventorySource`、`StickerSelectionState`、`HudScreenBinder` の既存利用箇所でコンパイルエラーや参照切れが出ない前提の変更に留める。

### 実装例
```csharp
using UnityEngine;

[System.Serializable]
public sealed class OwnedStickerDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private PeelSticker3D stickerPrefab;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public PeelSticker3D StickerPrefab => stickerPrefab;
}
```

7. [Assets/Scripts/StickerShop/StickerShopCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerShop/StickerShopCatalogSource.cs) を追加する。
8. `MonoBehaviour` として `List<OwnedStickerDefinition>` を保持し、一覧取得メソッドを公開する。

### 実装例
```csharp
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StickerShopCatalogSource : MonoBehaviour
{
    [SerializeField] private List<OwnedStickerDefinition> items = new();

    public IReadOnlyList<OwnedStickerDefinition> GetItems()
    {
        return items;
    }
}
```

## 手順2: ショップ画面の UXML を追加する
1. `Assets/UI/StickerShopScreen/UXML/` を作成する。
2. [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml) を追加する。
3. ルート構造は妖精コレクション画面と同じ考え方にし、以下の要素名を使う。
- `sticker-shop-overlay`
- `sticker-shop-backdrop`
- `sticker-shop-panel`
- `sticker-shop-title`
- `sticker-shop-empty-label`
- `sticker-shop-scroll-view`
- `sticker-shop-footer`
- `sticker-shop-close-button`
- `sticker-shop-money-label`

### 実装例
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss" />
    <ui:VisualElement name="sticker-shop-overlay">
        <ui:Button name="sticker-shop-backdrop" />
        <ui:VisualElement name="sticker-shop-panel">
            <ui:Label name="sticker-shop-title" text="シールショップ" />
            <ui:Label name="sticker-shop-empty-label" text="販売シールがありません" />
            <ui:ScrollView name="sticker-shop-scroll-view" mode="Vertical" />
            <ui:VisualElement name="sticker-shop-footer">
                <ui:Button name="sticker-shop-close-button" text="閉じる" />
                <ui:Label name="sticker-shop-money-label" text="お金：999円" />
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

## 手順3: ショップ画面の USS を追加する
1. `Assets/UI/StickerShopScreen/USS/` を作成する。
2. [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を追加する。
3. オーバーレイと右側パネルは [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) を踏襲する。
4. `ScrollView` の content container は折り返し設定を入れる。
5. カード用 class を用意する。
- `.sticker-shop-card`
- `.sticker-shop-card__image`
- `.sticker-shop-card__name`

### スタイル要点
- `#sticker-shop-overlay`: 全画面 absolute、初期 `display: none`
- `#sticker-shop-backdrop`: 半透明グレー、枠線なし
- `#sticker-shop-panel`: 右固定、縦並び、背景グレー
- `#sticker-shop-scroll-view .unity-scroll-view__content-container`: `flex-direction: row` と `flex-wrap: wrap`
- `.sticker-shop-card`: 幅固定、カード全体が押せるボタン
- `.sticker-shop-card__image`: `-unity-background-scale-mode: scale-to-fit`

## 手順4: HudScreenBinder にショップ機能を追加する
1. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に以下の SerializeField を追加する。
- `StickerShopCatalogSource stickerShopCatalogSource`
- `VisualTreeAsset stickerShopScreenAsset`
2. 取得済み UI 参照に `shopButton` を追加する。
3. ショップ用の参照フィールドを追加する。
- `VisualElement stickerShopOverlay`
- `Button stickerShopBackdrop`
- `VisualElement stickerShopPanel`
- `ScrollView stickerShopScrollView`
- `Label stickerShopEmptyLabel`
- `Button stickerShopCloseButton`
- `Label stickerShopMoneyLabel`
4. `OnEnable` で `shop-button` を取得し、クリックイベントを購読する。
5. `OnDisable` で購読解除する。
6. `InitializeStickerShopUi(VisualElement root)` を追加し、必要に応じて `stickerShopScreenAsset.CloneTree(root)` を呼ぶ。
7. 初期化後は `stickerShopOverlay.style.display = DisplayStyle.None;` にする。

## 手順5: ショップ画面の開閉処理を実装する
1. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `OpenStickerShop()` を追加する。
2. 開く前に `CloseFairyCollection()` を呼び、同時表示を防ぐ。
3. `RefreshStickerShop()` で一覧を更新してからオーバーレイを表示する。
4. `CloseStickerShop()` を追加し、`DisplayStyle.None` を設定する。
5. `stickerShopCloseButton.clicked` と `stickerShopBackdrop.clicked` の両方で `CloseStickerShop()` を呼ぶ。

### 実装例
```csharp
private void OpenStickerShop()
{
    if (stickerShopOverlay == null || stickerShopScrollView == null)
    {
        return;
    }

    CloseFairyCollection();
    RefreshStickerShop();
    stickerShopOverlay.style.display = DisplayStyle.Flex;
}

private void CloseStickerShop()
{
    if (stickerShopOverlay == null)
    {
        return;
    }

    stickerShopOverlay.style.display = DisplayStyle.None;
}
```

## 手順6: ショップ一覧生成とログ出力を実装する
1. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `RefreshStickerShop()` を追加する。
2. `stickerShopCatalogSource.GetItems()` から一覧取得する。
3. `ScrollView.Clear()` 後、0 件なら空表示ラベルを出し、一覧があれば空表示を隠す。
4. 各要素について `CreateStickerShopCard(OwnedStickerDefinition item)` を呼ぶ。
5. カードは `Button` で生成し、カード全体クリックでログ出力する。
6. 表示名が空なら `名称未設定`、画像が空なら画像未設定のまま表示する。

### 実装例
```csharp
private void RefreshStickerShop()
{
    if (stickerShopScrollView == null || stickerShopEmptyLabel == null)
    {
        return;
    }

    IReadOnlyList<OwnedStickerDefinition> items =
        stickerShopCatalogSource != null ? stickerShopCatalogSource.GetItems() : null;

    stickerShopScrollView.Clear();

    if (items == null || items.Count == 0)
    {
        stickerShopEmptyLabel.style.display = DisplayStyle.Flex;
        return;
    }

    stickerShopEmptyLabel.style.display = DisplayStyle.None;

    foreach (OwnedStickerDefinition item in items)
    {
        stickerShopScrollView.Add(CreateStickerShopCard(item));
    }
}

private Button CreateStickerShopCard(OwnedStickerDefinition item)
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
    card.clicked += () => Debug.Log($"ショップシール選択: {name.text}");
    return card;
}
```

## 手順7: Main.unity で参照設定を行う
1. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を Unity Editor で開く。
2. `HubScreenBinder` を持つ GameObject を選択する。
3. `stickerShopScreenAsset` に `StickerShopScreen.uxml` を設定する。
4. `stickerShopCatalogSource` にショップデータソースを持つ GameObject を設定する。
5. 必要なら新規 GameObject を作成して `StickerShopCatalogSource` を追加する。
6. `items` にサンプルシールを登録する。
- `displayName`: 画面とログに出すシール名
- `icon`: 既存の `Assets/GameResources/Seal/` 配下の画像
- `stickerPrefab`: 対応する prefab
7. 再生前に `shop-button` が既存 HUD に残っていることを確認する。

## 手順8: 動作確認
1. まず Play モードで既存の所持シール一覧とシール選択が、`OwnedStickerDefinition` リファクタ後も従来どおり動くことを確認する。
2. 続けて HUD の `ショップ` ボタンを押す。
3. 右側にショップ画面が開くことを確認する。
4. シールカードが 3 列基準で並ぶことを確認する。
5. 一覧件数が多い場合はスクロールできることを確認する。
6. 任意のカードを押し、Console に `ショップシール選択: シール名` が出ることを確認する。
7. カード押下では購入処理や金額変更が起きないことを確認する。
8. `閉じる` ボタンと背景押下の両方で閉じることを確認する。
9. 妖精コレクションを開いてからショップを開き、ショップだけが表示されることを確認する。
10. `items` を 0 件にした状態でも、空表示のままエラーなく開くことを確認する。

## 作業時の注意
- `OwnedStickerInventorySource` と `StickerShopCatalogSource` は役割を分けるが、個々のシール情報は共通 `OwnedStickerDefinition` を使う。
- 実装着手順は、必ず `OwnedStickerDefinition` のリファクタリングを先に行う。
- `HubScreenBinder` の `OnEnable` / `OnDisable` でイベント購読解除漏れを作らない。
- `CloneTree(root)` は重複追加を避けるため、既存要素があるか先に `Q()` で確認する。
- ショップと妖精コレクションの `name` は重複させない。
- ログ処理は購入処理へ発展しやすい位置なので、メソッドを分けておくと次フェーズで差し替えやすい。
