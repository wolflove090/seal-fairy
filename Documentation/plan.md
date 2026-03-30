# HUD操作ボタン白ベースブラッシュアップ 実装計画

## 実装方針
- 対象は [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) 上の `ready-button`、`shop-button`、`fairy-button` の3ボタンに限定し、クリック処理やフェーズ制御ロジックは変更しない。
- 見た目の基準は [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) の `#sticker-shop-close-button` と `.sticker-shop-card__price-plate` とし、白プレート、薄い縁、押し込み表現を `HudScreen` 側へ移植する。
- 既存の `HudScreenBinder.cs` は `root.Q<Button>("ready-button")` / `shop-button` / `fairy-button` で要素取得しているため、ボタン自身の `name` は維持する。
- 3ボタンのシリーズ感を USS の共通クラスまたは共通セレクタで整理しつつ、`ready-button` のみサイズや装飾で主操作差分を持たせる。
- 装飾が USS だけで成立しない場合のみ UXML に内包ラッパーやアクセント用 `VisualElement` / `Label` を追加するが、Binder が取得する Button 本体は最上位に残す。

## 変更対象ファイル一覧
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
  - `#ready-button`、`#shop-button`、`#fairy-button` の単色グレー定義を白ベースの共通ボタンスタイルへ置き換える。
  - hover / active の挙動をショップUI系の押し込み表現に揃える。
  - 必要に応じて3ボタン共通クラス、主ボタン差分、サブボタン差分のスタイルを追加する。
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
  - USS だけで表現しきれない場合に限り、ボタン内の装飾用子要素やラベルラッパーを追加する。
  - `ready-button`、`shop-button`、`fairy-button` の `name` は維持する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - UXML 構造変更でボタンテキスト更新や子要素取得が必要になった場合のみ最小限修正する。
  - Button の `name` を維持できれば原則変更しない。

## データフロー / 処理フロー
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `OnEnable()` が `ready-button`、`shop-button`、`fairy-button` を取得する。
2. `ready-button.clicked` は `HandleReadyButtonClicked()` に接続され、フェーズ状態に応じて `UpdateReadyButtonLabel()` がボタン文言を更新する。
3. `shop-button.clicked` は `OpenStickerShop()`、`fairy-button.clicked` は `OpenFairyCollection()` に接続される。
4. 今回は 1-3 のロジックを維持し、主に UXML/USS で見た目だけを白ベースへ刷新する。
5. UXML にボタン内子要素を追加する場合は、`Button.text` に依存する構造を避けるか、`UpdateReadyButtonLabel()` の更新先と両立する構成にする。

## 処理フロー詳細

### 1. 参照デザインの抽出
- [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss#L48) の `#sticker-shop-close-button` から、白背景、角丸、明暗のあるボーダー、hover / active の押し込み量を参照する。
- 同ファイルの [.sticker-shop-card__price-plate](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss#L168) から、白プレート系UIの縁の付け方と背景変化を参照する。
- 参照値をそのまま複製するのではなく、`HudScreen` の大きいCTAボタンとして自然に見えるサイズへ拡張する。

### 2. `HudScreen` ボタンの共通化
- [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss#L84) 付近の `#ready-button` 単独定義と、同ファイル後半の `#shop-button` / `#fairy-button` 定義を統合する。
- 共通スタイルでは、白ベース背景、濃い文字色、角丸、立体感のあるボーダー、`translate` / `scale` を使った状態遷移を持たせる。
- `ready-button` には主操作としての差分を付ける。候補は高さ、フォントサイズ、上部または内側のアクセント帯、内側余白の強化。
- `shop-button` と `fairy-button` はシリーズ感を優先し、サイズと装飾を揃える。

### 3. UXML 構造の判断
- [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml#L10) と [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml#L23) のボタンは現状テキストだけで構成されている。
- 文字以外の装飾が擬似要素なしでは表現しづらい場合は、Button の子に `VisualElement` を追加して背景アクセントや補助ラベル帯を置く。
- その場合でも Binder が取得する `Button` の `name` は変更せず、クリック領域を子要素で分断しない構造にする。
- `ready-button` の文言は C# から変わるため、子 `Label` 追加方式を採るなら `UpdateReadyButtonLabel()` の変更が必要になる。

### 4. フィードバック設計
- 通常状態は白基調、hover はやや暖色寄りまたは淡いピンク寄りに寄せて反応を示す。
- active は `translate: 0 3px; scale: 0.98;` 前後を基準にし、ショップUIと同系統の沈み込み表現を持たせる。
- `ready-button` の押下時はサブボタンより少しだけ存在感のある沈み込みにしてもよいが、過剰な差は付けない。

### 5. Binder 影響確認
- [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs#L62) から [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs#L64) の要素取得は `name` 維持であればそのまま使える。
- `UpdateReadyButtonLabel()` が `readyButton.text` を更新している前提を崩す場合のみ、文言更新先を子 `Label` へ差し替える修正を入れる。
- `shop-button` と `fairy-button` は静的文言なので、追加修正が必要になるのは主に `ready-button` のみと想定する。

## リスクと対策
- Button 内部に子要素を追加すると、UI Toolkit の標準テキスト表示と競合し、二重表示や余白崩れが起きる可能性がある。
  - 装飾を USS のみで成立させる案を優先し、子要素追加は必要最低限に留める。
- `ready-button` のラベル更新が `Button.text` に依存しているため、カスタム構造へ変えると文言が更新されなくなる可能性がある。
  - `ready-button` は標準テキスト描画を維持するか、変更する場合は `UpdateReadyButtonLabel()` の更新先を同時に修正する。
- 白ベースにすると背景が明るいシーンで輪郭が弱くなる可能性がある。
  - 外周ボーダーと下側に重心のある縁色を入れ、必要なら淡い影色に相当する差分を border 色で補強する。
- 主操作とサブ操作の差を付けすぎるとシリーズ感が崩れる。
  - 差分は1要素に限定し、色相は共通化する。

## 検証方針
- 手動確認1: 通常 HUD 表示時、`シールめくりへ`、`ショップ`、`妖精` の3ボタンが白ベースのプレート表現になっている。
- 手動確認2: `シールめくりへ` がサブボタン2つより主操作として視認しやすい。
- 手動確認3: `ショップ` / `妖精` が同一シリーズとして揃って見える。
- 手動確認4: hover / active でショップUIと同系統のフィードバックが出る。
- 手動確認5: `ready-button` のフェーズ文言更新、ショップ起動、妖精画面起動が既存どおり動く。
- 手動確認6: ショップオーバーレイや妖精コレクションを開閉してもボタン位置と意匠が崩れない。

## コードスニペット
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

```csharp
private void OnEnable()
{
    readyButton = root.Q<Button>("ready-button");
    fairyButton = root.Q<Button>("fairy-button");
    shopButton = root.Q<Button>("shop-button");
}
```
