# HUD操作ボタン白ベースブラッシュアップ 作業手順書

## 目的
- `HudScreen` の `シールめくりへ`、`ショップ`、`妖精` の3ボタンを、既存UIに馴染む白ベースのプレートデザインへ更新する。
- ボタンのクリック処理、フェーズ切替、ショップ起動、妖精画面起動は維持する。
- 可能な限り USS で完結し、`HudScreenBinder.cs` の変更を避ける。

## 変更対象
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- 参照元: [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)

## 手順1: 参照元の白プレート表現を確認する
1. [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を開く。
2. `#sticker-shop-close-button` の背景、border、角丸、hover / active を確認する。
3. `.sticker-shop-card__price-plate` の白プレート表現と沈み込み量を確認する。
4. これらを `HudScreen` の大型ボタンへ転用する前提で、色と border の関係をメモする。

## 手順2: 現状の `HudScreen` ボタン構造を確認する
1. [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) を開く。
2. `ready-button`、`shop-button`、`fairy-button` が単体 Button 要素として置かれていることを確認する。
3. まずは USS のみで意匠変更する方針を採る。
4. 追加装飾が必要な場合だけ、Button 内に子要素を追加する。

### 現状構造
```xml
<ui:VisualElement name="top-bar">
    <ui:VisualElement name="money-panel">
        <ui:VisualElement name="money-icon" />
        <ui:Label name="money-label" text="999,999"/>
    </ui:VisualElement>
    <ui:Button name="ready-button" text="シールめくりへ"/>
</ui:VisualElement>

<ui:VisualElement name="bottom-right-menu">
    <ui:Button name="shop-button" text="ショップ"/>
    <ui:Button name="fairy-button" text="妖精"/>
</ui:VisualElement>
```

## 手順3: `HudScreen.uss` のボタン定義を共通化する
1. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) を開く。
2. `#ready-button` の単独定義と、`#fairy-button, #shop-button` の定義を整理する。
3. 3ボタン共通の見た目を1つのセレクタへ集約する。
4. グレー背景と border 無しの定義を、白プレート、立体 border、角丸、暖色寄り文字色へ置き換える。

### 変更例
```css
#ready-button,
#shop-button,
#fairy-button {
    background-color: rgb(255, 255, 255);
    color: rgb(134, 80, 0);
    border-top-left-radius: 22px;
    border-top-right-radius: 22px;
    border-bottom-left-radius: 22px;
    border-bottom-right-radius: 22px;
    border-left-width: 2px;
    border-right-width: 2px;
    border-top-width: 3px;
    border-bottom-width: 5px;
    border-left-color: rgb(236, 228, 228);
    border-right-color: rgb(222, 210, 210);
    border-top-color: rgb(255, 255, 255);
    border-bottom-color: rgb(212, 188, 188);
    transition-property: translate, scale, background-color;
    transition-duration: 0.1s;
}
```

## 手順4: 主操作とサブ操作の差を付ける
1. `ready-button` はフェーズ変更の主操作として、サイズまたは文字強度で差分を付ける。
2. `shop-button` と `fairy-button` は同シリーズの意匠に揃える。
3. 差分は最小限にし、色相は共通の白ベース系で統一する。
4. 必要なら `ready-button` だけ `font-size`、`min-height`、`border-bottom-width` を少し強める。

### 変更例
```css
#ready-button {
    min-height: 148px;
    font-size: 54px;
}

#shop-button,
#fairy-button {
    min-height: 140px;
    font-size: 50px;
}
```

## 手順5: hover / active のフィードバックを揃える
1. 3ボタン共通で hover 色を少しだけ暖色寄りにする。
2. active では沈み込み表現を付ける。
3. ショップUIと同系統になるよう、`translate` と `scale` の値は控えめにする。
4. 必要なら active 時に border 幅も微調整する。

### 変更例
```css
#ready-button:hover,
#shop-button:hover,
#fairy-button:hover {
    background-color: rgb(255, 245, 247);
}

#ready-button:active,
#shop-button:active,
#fairy-button:active {
    translate: 0 3px;
    scale: 0.98;
}
```

## 手順6: USS だけで難しい場合のみ UXML を拡張する
1. 文字以外のアクセント帯や内側面が必要なら [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) の Button 内に `VisualElement` を追加する。
2. `ready-button`、`shop-button`、`fairy-button` の `name` は変更しない。
3. クリック可能領域は Button 全体のまま維持する。
4. `ready-button` のテキストが C# から更新されることを忘れず、標準テキストを維持するか Binder を修正する。

### 拡張例
```xml
<ui:Button name="shop-button">
    <ui:VisualElement class="hud-action-button__accent" />
    <ui:Label class="hud-action-button__label" text="ショップ" />
</ui:Button>
```

## 手順7: `HudScreenBinder.cs` は必要な場合だけ修正する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. Button の `name` を維持していれば `OnEnable()` のクエリはそのままでよい。
3. `readyButton.text` 更新を子 `Label` 更新へ切り替える必要がある場合のみ、`UpdateReadyButtonLabel()` を修正する。
4. それ以外のクリックイベント配線には触れない。

### 確認箇所
```csharp
readyButton = root.Q<Button>("ready-button");
fairyButton = root.Q<Button>("fairy-button");
shopButton = root.Q<Button>("shop-button");
```

## 手順8: 動作確認を行う
1. 通常 HUD で3ボタンが白ベースのボタンに変わっていることを確認する。
2. `シールめくりへ` が `ショップ` / `妖精` より主操作として見えることを確認する。
3. hover / active の反応があることを確認する。
4. フェーズ変更で `ready-button` の文言更新が崩れないことを確認する。
5. `ショップ` 押下でショップ画面、`妖精` 押下で妖精一覧が既存どおり開くことを確認する。
6. ショップや妖精画面を閉じたあとも、ボタン意匠とレイアウトが崩れないことを確認する。

## 完了条件
- `ready-button`、`shop-button`、`fairy-button` が白ベースのプレートデザインに更新されている。
- 主操作とサブ操作の差が視認できるが、シリーズ感は保たれている。
- `HudScreenBinder.cs` の既存機能に回帰がない。
