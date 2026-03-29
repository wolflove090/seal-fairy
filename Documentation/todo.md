# ショップUIと周辺UIブラッシュアップ ToDo

1. [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) の HUD 構造を見直し、所持金パネルと所持シール一覧パネルのラッパー要素を追加する。
2. [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) を更新し、ピンク基調の所持金パネル、シール一覧パネル、右下メニューの新スタイルを定義する。
3. コインアイコン用の新規画像アセットを追加し、UI から参照できる配置先と命名を確定する。
4. [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml) を更新し、ヘッダー、閉じるボタン、コンテンツフレームをモック準拠へ組み替える。
5. [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を更新し、ショップオーバーレイ、カード、価格プレート、無効状態の新スタイルを定義する。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の要素取得名と初期化処理を UXML 変更に合わせて更新する。
7. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の所持金表示更新を数値主体フォーマットへ変更する。
8. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateStickerCell()` を更新し、所持シールタイルの内部構造とクラス名を新デザインへ合わせる。
9. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateStickerShopCard()` を更新し、ショップカードを画像領域 + 名称 + 価格プレート構成へ変更する。
10. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) の参照設定を確認し、必要ならコインアイコンや更新済み UXML アセットが正しく割り当たるようにする。
11. Unity 実行確認で、通常 HUD、ショップ表示、購入可否、購入後更新、妖精画面との排他表示、フェーズ切替の退行がないことを検証する。
