# シールショップ機能 ToDo

1. 最初に [Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs) をリファクタリングし、`displayName` を追加しても既存の所持シール一覧と選択処理が壊れない形へ整理する。
2. [Assets/Scripts/StickerShop/StickerShopCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerShop/StickerShopCatalogSource.cs) を追加し、`List<OwnedStickerDefinition>` を Inspector で設定できる `MonoBehaviour` を実装する。
3. [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml) を追加し、オーバーレイ、右側パネル、タイトル、空表示、スクロール領域、フッターを定義する。
4. [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を追加し、右側パネル、3 列カード、閉じるボタン、固定金額表示の見た目を実装する。
5. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `shop-button` の取得、ショップ `VisualTreeAsset` / カタログ参照、初期化処理を追加する。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `OpenStickerShop()`、`CloseStickerShop()`、`RefreshStickerShop()`、`OwnedStickerDefinition` を使ったカード生成、空表示制御、ログ出力処理を実装する。
7. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) で妖精コレクションとショップ画面が同時に開かない排他制御を入れる。
8. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で `HubScreenBinder` にショップ用 `VisualTreeAsset` と `StickerShopCatalogSource` を割り当てる。
9. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) でショップカタログにサンプルシールを登録し、共通 `OwnedStickerDefinition` の表示名と画像を確認できる状態にする。
10. 手動確認で、まず `OwnedStickerDefinition` リファクタ後の既存所持シール一覧 / 選択を確認し、その後ショップ画面の開閉、3 列スクロール、カードタップ時ログ、購入処理未実装維持、空表示、妖精コレクションとの排他動作を検証する。
