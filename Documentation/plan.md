# シールショップ機能 実装計画

## 実装方針
- 最初の作業は [OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs) のリファクタリングとし、ショップ表示で必要な `displayName` を追加しても既存の所持シール選択処理が維持される状態を先に作る。
- 既存の HUD は [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) と [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を中心に構成されているため、ショップ画面も妖精コレクションと同じく `VisualTreeAsset` を `HubScreenBinder` から差し込む方式で追加する。
- ショップ一覧データは、既存の所持シール一覧 `OwnedStickerInventorySource` とは分離した専用の `StickerShopCatalogSource` を新設して管理する。一方で個々のシール情報は保持シールと共通の [OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs) を利用する。
- UI は新規 UXML / USS を追加し、右側パネル、背景オーバーレイ、`ScrollView`、`閉じる` ボタン、固定文言の所持金表示を妖精コレクションと同じレイアウト思想で実装する。
- ショップカードは 3 列グリッドを基本とし、`ScrollView` の content container を折り返しレイアウトにして件数増加に耐える構成にする。
- シールタップ時の挙動は購入処理ではなく `Debug.Log` のみとし、ログ対象のシール名は `OwnedStickerDefinition` に追加した表示名を使う。
- 妖精コレクションとショップ画面は同時表示させず、いずれかを開く際にもう片方を閉じる制御を `HubScreenBinder` に集約する。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs)
  - 所持シールとショップ表示で共通利用できるようリファクタリングし、`displayName` を追加する。
  - 既存の `OwnedStickerInventorySource` と `StickerSelectionState` からの利用に破壊的変更を出さない。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - `shop-button` の取得とクリック購読を追加する。
  - ショップ画面の `VisualTreeAsset` とショップデータソース参照を追加する。
  - ショップオーバーレイの初期化、開閉、一覧生成、空表示制御、シールタップ時ログ出力を実装する。
  - 妖精コレクションとショップ画面が排他的に開くようにする。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `HubScreenBinder` を持つ GameObject にショップ用 `VisualTreeAsset` と `StickerShopCatalogSource` を割り当てる。
  - 必要であればショップデータソース用の GameObject / Component を追加する。

### 新規作成予定
- [Assets/Scripts/StickerShop/StickerShopCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerShop/StickerShopCatalogSource.cs)
  - `List<OwnedStickerDefinition>` を Inspector で保持し、一覧取得 API を公開する。
- [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml)
  - ショップオーバーレイの UI 構造を定義する。
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
  - ショップ画面専用スタイルを定義する。

## データフロー / 処理フロー
1. 最初に `OwnedStickerDefinition` をリファクタリングし、`displayName` を追加したうえで既存の所持シール選択 UI が維持されることを確認する。
2. プレイヤーが HUD の `ショップ` ボタンを押す。
3. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `shopButton.clicked` が `OpenStickerShop()` を呼ぶ。
4. `OpenStickerShop()` は、妖精コレクションが開いていれば先に閉じる。
5. ショップオーバーレイ未初期化時は `StickerShopScreen.uxml` を root へ `CloneTree` し、必要な要素参照を取得する。
6. `RefreshStickerShop()` が `StickerShopCatalogSource.GetItems()` を呼び、一覧を再構築する。
7. 一覧 0 件なら空表示ラベルを表示し、カード生成をスキップする。
8. 一覧ありなら各 `OwnedStickerDefinition` からカードを生成し、`ScrollView` に追加する。
9. プレイヤーがカードを押すと `Debug.Log($"ショップシール選択: {item.DisplayName}")` を出力する。
10. `閉じる` ボタンまたは背景オーバーレイ押下で、ショップオーバーレイを `DisplayStyle.None` に戻す。

## UI 設計

### ルート構成
- `sticker-shop-overlay`
  - 画面全体を覆うオーバーレイ。初期状態は非表示。
- `sticker-shop-backdrop`
  - 半透明の全面ボタン。押下で閉じる。
- `sticker-shop-panel`
  - 右側固定の大型パネル。

### パネル内要素
- `sticker-shop-title`
  - タイトル `シールショップ` を表示するラベル。
- `sticker-shop-empty-label`
  - 一覧 0 件時だけ表示する空表示ラベル。
- `sticker-shop-scroll-view`
  - ショップカードを表示する縦スクロール領域。
- `sticker-shop-footer`
  - 下部行コンテナ。
- `sticker-shop-close-button`
  - 閉じるボタン。
- `sticker-shop-money-label`
  - 固定文言 `お金：999円` を表示するラベル。

### カード構成
- `Button` ベースのカードにして、カード全体をタップ可能にする。
- カード内に画像用 `VisualElement` と名称用 `Label` を持たせる。
- class は `.sticker-shop-card`, `.sticker-shop-card__image`, `.sticker-shop-card__name` を基本とする。
- 3 列で並ぶ前提で幅を固定し、`ScrollView` content container を `flex-wrap: wrap` にする。

## 処理詳細

### 1. ショップ専用データソース
- `FairyCatalogSource` と同じ粒度の `MonoBehaviour` として `StickerShopCatalogSource` を追加する。
- `StickerShopCatalogSource` が保持する一覧要素は `OwnedStickerDefinition` とし、既存所持シール定義を共通利用する。
- [OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs) は今回の作業対象としてリファクタリングし、`displayName` を追加する。
- 当面の UI 利用は `displayName` と `icon` が中心だが、将来の購入処理や配置連携を見据えて既存の `stickerPrefab` もそのまま利用する。

### 2. HubScreenBinder の拡張
- 既存の `fairyButton` と同様に `shopButton` を取得する。
- 新規 SerializeField:
- `StickerShopCatalogSource stickerShopCatalogSource`
- `VisualTreeAsset stickerShopScreenAsset`
- 新規参照フィールド:
- `VisualElement stickerShopOverlay`
- `Button stickerShopBackdrop`
- `ScrollView stickerShopScrollView`
- `Label stickerShopEmptyLabel`
- `Button stickerShopCloseButton`
- `Label stickerShopMoneyLabel`
- `OnEnable` で購読し、`OnDisable` で解除する。
- `InitializeStickerShopUi(root)` を追加し、必要なら UXML を root へ複製する。
- `OpenStickerShop()`, `CloseStickerShop()`, `RefreshStickerShop()`, `CreateStickerShopCard(OwnedStickerDefinition item)` を追加する。
- `OpenFairyCollection()` と `OpenStickerShop()` で相互に片方を閉じる。

### 3. ショップカード生成
- カードは `Button` で生成し、押下時に `HandleStickerShopItemClicked(item)` を呼ぶ。
- `item.Icon` がある場合は背景画像へ設定する。
- `item.Icon` が未設定でも例外を出さず、空背景のまま表示する。
- `item.DisplayName` が null または空の場合は、暫定で `名称未設定` を表示するフォールバックを入れる。

### 4. ログ出力
- クリック時のログは実装確認しやすい固定フォーマットにする。
- 例: `Debug.Log($"ショップシール選択: {displayName}")`
- 購入や状態変更を示唆するログ文言は使わない。

### 5. シーン設定
- `Main.unity` 上でショップデータソースへサンプルシールを登録する。
- `HubScreenBinder` の Inspector に `StickerShopCatalogSource` と `StickerShopScreen` の `VisualTreeAsset` を設定する。
- サンプルシール画像は既存の `Assets/GameResources/Seal/` 配下の画像や prefab に紐づけて確認できる状態にする。

## リスクと対策
- 既存 `OwnedStickerDefinition` にシール名がないため、そのままではログと UI 表示が成立しない。
  - `OwnedStickerDefinition` に `displayName` を追加し、既存所持シール用途でも破綻しないよう null 安全に扱う。
- 妖精コレクションとショップ画面の両方を `CloneTree(root)` するため、要素名競合や開閉制御の抜けが起こりうる。
  - 各画面で一意な name を採用し、開く側で他方を閉じる制御を明示する。
- `ScrollView` のカード横並び設定を誤ると 3 列にならず崩れる可能性がある。
  - `.unity-scroll-view__content-container` に `flex-direction: row` と `flex-wrap: wrap` を設定する。
- 画像未設定やデータ 0 件で null 参照が起こる可能性がある。
  - `RefreshStickerShop()` とカード生成で null / 件数 0 を明示的に扱う。

## 検証方針
- 手動確認0:
  - `OwnedStickerDefinition` リファクタ後も既存のシール一覧表示とシール配置選択が正常動作すること。
- 手動確認1:
  - HUD の `ショップ` ボタン押下でショップ画面が開くこと。
- 手動確認2:
  - 一覧件数が多い場合でも、縦スクロールで最後まで見えること。
- 手動確認3:
  - カードが右側パネル内で 3 列基準に並ぶこと。
- 手動確認4:
  - 任意のカード押下で `ショップシール選択: シール名` が Console に出ること。
- 手動確認5:
  - カード押下では購入処理や画面クローズが発生しないこと。
- 手動確認6:
  - `閉じる` ボタンまたは背景押下でショップ画面が閉じること。
- 手動確認7:
  - 妖精コレクションが開いている状態からショップを開くと、妖精コレクションが閉じてショップのみ表示されること。
- 手動確認8:
  - データ 0 件や画像未設定でもエラーなく表示できること。

## コードスニペット
```csharp
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
```

```csharp
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
