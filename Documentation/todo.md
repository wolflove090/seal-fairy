# 妖精コレクション画面 ToDo

1. [Assets/UI/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/FairyCollectionScreen.uxml) を追加し、背景オーバーレイ、右側パネル、スクロール領域、閉じるボタン、発見数ラベルを定義する。
2. [Assets/UI/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/FairyCollectionScreen.uss) を追加し、ワイヤー準拠のレイアウトと未獲得グレーアウト表現を作る。
3. [Assets/Scripts/Fairy/FairyCollectionService.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCollectionService.cs) に `IsDiscovered` と件数集計 API を追加する。
4. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `fairy-button` の参照、コレクション UXML の初期化、表示切替、一覧構築処理を追加する。
5. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) でカード生成処理を実装し、獲得済み / 未獲得の表示分岐を入れる。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) で背景押下と閉じるボタン押下の両方に close 処理を接続する。
7. [Assets/UI/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml) と [Assets/UI/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss) を必要最小限調整し、HUD 本体との共存を確認する。
8. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で `HubScreenBinder` に `FairyCatalogSource` と新規 `VisualTreeAsset` を割り当てる。
9. 手動確認で、表示 / 閉じる / 背面入力遮断 / 未獲得表示 / 0 件表示 / スクロール表示を検証する。

