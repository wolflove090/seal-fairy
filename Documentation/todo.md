# 配置シールマウス追従プレビュー ToDo

1. [Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) に選択変更通知イベントを追加する。
2. [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) にプレビュー更新処理、表示可否制御、HUD へのスクリーン座標通知を追加する。
3. [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) の既存配置処理を調整し、プレビュー用インスタンスと配置本体生成が両立することを確認する。
4. [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) に `preview-count-label` を追加する。
5. [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) にプレビュー残数ラベル用スタイルを追加する。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) にプレビュー残数ラベルの取得、位置更新、文言更新、表示切り替え処理を追加する。
7. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) で `StickerSelectionState` と `OwnedStickerInventorySource` の通知を受け、残数表示を同期する。
8. [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) を必要に応じて調整し、フェーズ切替時にプレビューが確実に消えるようにする。
9. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) の参照設定と実行時表示を確認する。
10. 手動確認で、プレビュー追従、残数表示同期、残数 0 での非表示、フェーズ切替、UI 操作時の非配置、既存機能の退行なしを検証する。
