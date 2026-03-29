# ショップUIと周辺UIブラッシュアップ 作業手順書

## 目的
- HUD 左上の所持金パネルを、コインアイコン付きのピンク基調 UI に変更する。
- HUD 左下の所持シール一覧を、ヘッダー付きピンクパネルとモック準拠のタイル表示へ変更する。
- ショップオーバーレイを右側大型パネルへ刷新し、カードを画像領域 + 名称 + 価格プレート構成へ変更する。
- 既存の購入処理、所持金同期、所持シール更新、画面排他制御を維持する。

## 変更対象
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
- [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml)
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- コインアイコン用の新規画像アセット

## 手順1: コインアイコン画像を追加する
1. UI 用の配置先を決める。候補は `Assets/GameResources/Texture/` 配下、または `Assets/UI/Common/` 配下。
2. コインアイコン画像を `coin_icon.png` のような用途が分かる名前で追加する。
3. Unity 上で `Texture Type` を `Sprite (2D and UI)` に設定する。
4. 後から差し替えしやすいよう、用途が HUD 所持金用であることが分かる命名を維持する。

## 手順2: HudScreen.uxml の HUD 構造を組み替える
1. [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) を開く。
2. `money-panel` の直下をラベル単体から、アイコン要素と数値ラベルを持つ構造へ変更する。
3. `bottom-left-sticker-panel` はヘッダー帯と本体領域に分ける。
4. `preview-count-label` は既存のまま残す。

### 変更後イメージ
```xml
<ui:VisualElement name="root">
    <ui:Label name="preview-count-label" text="残り x0" />
    <ui:VisualElement name="top-bar">
        <ui:VisualElement name="money-panel">
            <ui:VisualElement name="money-icon" />
            <ui:Label name="money-label" text="999,999" />
        </ui:VisualElement>
        <ui:Button name="ready-button" text="シールめくりへ" />
    </ui:VisualElement>

    <ui:VisualElement name="bottom-left-sticker-panel">
        <ui:VisualElement name="sticker-list-header">
            <ui:Label name="sticker-list-title" text="シール一覧" />
        </ui:VisualElement>
        <ui:VisualElement name="sticker-list-body">
            <ui:Label name="empty-sticker-list-label" text="所持シールがありません" />
            <ui:ScrollView name="sticker-scroll-view" mode="Vertical" />
        </ui:VisualElement>
    </ui:VisualElement>

    <ui:VisualElement name="bottom-right-menu">
        <ui:Button name="shop-button" text="ショップ" />
        <ui:Button name="fairy-button" text="妖精" />
    </ui:VisualElement>
</ui:VisualElement>
```

## 手順3: HudScreen.uss をモック準拠の見た目へ更新する
1. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) を開く。
2. `money-panel` を横並び・ピンク背景・白枠風の見た目へ変更する。
3. `money-icon` のサイズと背景画像表示スタイルを追加する。
4. `bottom-left-sticker-panel`、`sticker-list-header`、`sticker-list-body` の見た目を設定する。
5. 既存の `.sticker-cell` 系スタイルは、新しいタイル構成に合わせて作り直す。
6. `shop-button` / `fairy-button` は現行の機能を維持しつつ、新デザインに合う最低限の見た目へ寄せる。

### 追加・更新コード例
```css
#money-panel {
    width: 430px;
    min-height: 144px;
    padding-left: 26px;
    padding-right: 34px;
    padding-top: 18px;
    padding-bottom: 18px;
    flex-direction: row;
    align-items: center;
    background-color: rgb(246, 132, 166);
    border-top-left-radius: 18px;
    border-top-right-radius: 18px;
    border-bottom-left-radius: 18px;
    border-bottom-right-radius: 18px;
}

#money-icon {
    width: 74px;
    height: 74px;
    margin-right: 20px;
    -unity-background-scale-mode: scale-to-fit;
    background-repeat: no-repeat;
    background-position-x: center;
    background-position-y: center;
}

#money-label {
    flex-grow: 1;
    font-size: 64px;
    color: rgb(255, 255, 255);
    -unity-text-align: middle-left;
}

#bottom-left-sticker-panel {
    position: absolute;
    left: 0;
    bottom: 0;
    width: 590px;
    height: 504px;
}

#sticker-list-header {
    height: 84px;
    padding-left: 24px;
    justify-content: center;
    background-color: rgb(255, 91, 122);
    border-top-left-radius: 18px;
    border-top-right-radius: 18px;
}

#sticker-list-body {
    flex-grow: 1;
    padding: 18px;
    background-color: rgb(247, 205, 216);
    border-bottom-left-radius: 18px;
    border-bottom-right-radius: 18px;
}
```

## 手順4: StickerShopScreen.uxml を再構成する
1. [StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml) を開く。
2. ショップパネル内をヘッダーとコンテンツフレームに分ける。
3. 閉じるボタンはヘッダーの右上固定になる構造にする。
4. `sticker-shop-money-label` はコンテンツフレーム上部、またはヘッダー直下に配置する。
5. `sticker-shop-scroll-view` はコンテンツフレーム内へ移動する。

### 変更後イメージ
```xml
<ui:VisualElement name="sticker-shop-overlay">
    <ui:Button name="sticker-shop-backdrop" />
    <ui:VisualElement name="sticker-shop-panel">
        <ui:VisualElement name="sticker-shop-header">
            <ui:Label name="sticker-shop-title" text="シールショップ" />
            <ui:Button name="sticker-shop-close-button" text="X" />
        </ui:VisualElement>
        <ui:VisualElement name="sticker-shop-content-frame">
            <ui:VisualElement name="sticker-shop-money-panel">
                <ui:Label name="sticker-shop-money-label" text="999,999" />
            </ui:VisualElement>
            <ui:Label name="sticker-shop-empty-label" text="販売シールがありません" />
            <ui:ScrollView name="sticker-shop-scroll-view" mode="Vertical" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:VisualElement>
```

## 手順5: StickerShopScreen.uss を全面更新する
1. [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を開く。
2. `sticker-shop-panel` を右寄せの大型ピンクパネルに変更する。
3. `sticker-shop-header`、`sticker-shop-title`、`sticker-shop-close-button` のスタイルを追加する。
4. `sticker-shop-content-frame` に薄ピンクの内側フレーム、余白、スクロール領域スタイルを追加する。
5. `.sticker-shop-card` とその子要素クラスを新しい構成へ差し替える。

### 追加・更新コード例
```css
#sticker-shop-panel {
    position: absolute;
    top: 0;
    right: 0;
    width: 830px;
    bottom: 0;
    padding-top: 18px;
    padding-left: 26px;
    padding-right: 26px;
    padding-bottom: 26px;
    background-color: rgb(245, 127, 155);
}

#sticker-shop-header {
    height: 110px;
    flex-direction: row;
    justify-content: space-between;
    align-items: flex-start;
}

#sticker-shop-title {
    font-size: 64px;
    color: rgb(255, 255, 255);
}

#sticker-shop-close-button {
    width: 92px;
    height: 92px;
    background-color: rgb(255, 255, 255);
    color: rgb(0, 0, 0);
    font-size: 56px;
    border-top-left-radius: 20px;
    border-top-right-radius: 20px;
    border-bottom-left-radius: 20px;
    border-bottom-right-radius: 20px;
}

.sticker-shop-card {
    width: 210px;
    margin-right: 34px;
    margin-bottom: 34px;
    background-color: rgba(0, 0, 0, 0);
}

.sticker-shop-card__image-frame {
    height: 214px;
    padding: 18px;
    justify-content: flex-end;
    align-items: center;
    background-color: rgb(194, 217, 45);
    border-top-left-radius: 18px;
    border-top-right-radius: 18px;
    border-bottom-left-radius: 18px;
    border-bottom-right-radius: 18px;
}

.sticker-shop-card__price-plate {
    margin-top: -6px;
    height: 56px;
    justify-content: center;
    background-color: rgb(255, 255, 255);
    border-top-left-radius: 16px;
    border-top-right-radius: 16px;
    border-bottom-left-radius: 16px;
    border-bottom-right-radius: 16px;
}
```

## 手順6: HudScreenBinder の要素取得と所持金表示を更新する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. `money-label` と `sticker-shop-money-label` のクエリ名は維持できるならそのまま使う。
3. 追加した `money-icon` またはショップ内所持金パネルに対して、必要なら `VisualElement` 参照を追加する。
4. `UpdateMoneyLabels()` の文言を `balance.ToString("N0")` へ変更する。
5. ショップ内ラベルも同じ書式に揃える。

### 変更コード例
```csharp
private void UpdateMoneyLabels()
{
    RefreshMoneyLabelReferences();

    int balance = currencyBalanceSource != null ? currencyBalanceSource.CurrentBalance : 0;
    string text = balance.ToString("N0");

    if (moneyLabel != null)
    {
        moneyLabel.text = text;
    }

    if (stickerShopMoneyLabel != null)
    {
        stickerShopMoneyLabel.text = text;
    }
}
```

## 手順7: HudScreenBinder の所持シールタイル生成を更新する
1. `CreateStickerCell()` を開く。
2. ボタン内に画像要素、シール名ラベル、枚数バッジを追加する構造へ変更する。
3. クラス名は `.sticker-cell__image`、`.sticker-cell__name`、`.sticker-cell__count` のように分ける。
4. シール名は長すぎる場合に崩れないよう短め領域へ収める。

### 変更コード例
```csharp
private Button CreateStickerCell(StickerDefinition sticker)
{
    Button cell = new();
    cell.AddToClassList("sticker-cell");
    cell.tooltip = string.IsNullOrWhiteSpace(sticker?.DisplayName) ? "名称未設定" : sticker.DisplayName;

    VisualElement image = new();
    image.AddToClassList("sticker-cell__image");
    if (sticker != null && sticker.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(sticker.Icon.texture);
    }

    Label nameLabel = new();
    nameLabel.AddToClassList("sticker-cell__name");
    nameLabel.text = string.IsNullOrWhiteSpace(sticker?.DisplayName) ? "名称未設定" : sticker.DisplayName;

    Label countLabel = new();
    countLabel.AddToClassList("sticker-cell__count");
    int count = inventorySource != null ? inventorySource.GetOwnedStickerCount(sticker) : 0;
    countLabel.text = $"x{count}";

    cell.Add(image);
    cell.Add(nameLabel);
    cell.Add(countLabel);
    cell.clicked += () => HandleStickerCellClicked(sticker);
    return cell;
}
```

## 手順8: HudScreenBinder のショップカード生成を更新する
1. `CreateStickerShopCard()` を開く。
2. 画像フレーム、名称、価格プレートを持つ構成へ変更する。
3. 価格は `item.Price.ToString("N0")` で数値のみ表示する。
4. 無効状態クラスをカード全体へ付け、必要なら子要素にもクラスを追加する。

### 変更コード例
```csharp
private Button CreateStickerShopCard(StickerDefinition item)
{
    Button card = new();
    card.AddToClassList("sticker-shop-card");

    VisualElement imageFrame = new();
    imageFrame.AddToClassList("sticker-shop-card__image-frame");

    VisualElement image = new();
    image.AddToClassList("sticker-shop-card__image");
    if (item != null && item.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(item.Icon.texture);
    }

    Label name = new();
    name.AddToClassList("sticker-shop-card__name");
    name.text = string.IsNullOrWhiteSpace(item?.DisplayName) ? "シール名" : item.DisplayName;

    VisualElement pricePlate = new();
    pricePlate.AddToClassList("sticker-shop-card__price-plate");

    Label price = new();
    price.AddToClassList("sticker-shop-card__price");
    price.text = item != null ? item.Price.ToString("N0") : "0";

    imageFrame.Add(image);
    imageFrame.Add(name);
    pricePlate.Add(price);
    card.Add(imageFrame);
    card.Add(pricePlate);

    bool canPurchase = item != null &&
        currencyBalanceSource != null &&
        currencyBalanceSource.CurrentBalance >= item.Price;

    card.EnableInClassList("sticker-shop-card--disabled", !canPurchase);
    card.SetEnabled(canPurchase);

    if (canPurchase)
    {
        card.clicked += () => HandleStickerShopItemClicked(item);
    }

    return card;
}
```

## 手順9: コインアイコンを UI に反映する
1. `money-icon` の背景画像を USS で固定できるなら、その方法で設定する。
2. もし USS で参照しづらい構成なら、`HudScreenBinder` に `SerializeField private Sprite moneyIconSprite;` を追加し、`OnEnable()` で `moneyIcon.style.backgroundImage = new StyleBackground(moneyIconSprite);` を設定する。
3. その場合は [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で `HudScreenBinder` の参照へ割り当てる。

### C# で設定する場合のコード例
```csharp
[SerializeField] private Sprite moneyIconSprite;

private VisualElement moneyIcon;

private void OnEnable()
{
    root = uiDocument.rootVisualElement;
    moneyIcon = root.Q<VisualElement>("money-icon");

    if (moneyIcon != null && moneyIconSprite != null)
    {
        moneyIcon.style.backgroundImage = new StyleBackground(moneyIconSprite);
    }
}
```

## 手順10: 動作確認を行う
1. 通常 HUD で所持金パネルがコインアイコン付き、数値主体表示になっていることを確認する。
2. 左下の所持シール一覧がヘッダー付きピンクパネルになっていることを確認する。
3. ショップを開き、右側大型ピンクパネル、タイトル、`X` ボタン、カード 3 列表示を確認する。
4. 購入可能カードと購入不可カードが見た目で判別できることを確認する。
5. 購入時に残高が減り、HUD とショップ内表示が同じ値へ更新されることを確認する。
6. 購入したシールが左下一覧の先頭に追加されることを確認する。
7. 妖精画面との排他表示、フェーズ切り替え、既存ログ出力が壊れていないことを確認する。
