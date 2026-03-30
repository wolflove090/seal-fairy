# シール一覧UIショップ準拠調整 ToDo

1. [シール一覧UIショップ準拠調整要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/シール一覧UIショップ準拠調整要件書.md) を再確認し、対象が `シール一覧` パネルのみであることを確認する。
2. [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) から、濃いピンク外側コンテナと薄いピンク内側コンテンツの配色・余白・角丸の基準値を拾う。
3. [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) の `bottom-left-sticker-panel`、`sticker-list-header`、`sticker-list-body` の構造を確認し、USS だけで対応できるか判断する。
4. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) を更新し、`bottom-left-sticker-panel` を濃いピンク背景の外側コンテナに変更する。
5. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) を更新し、`sticker-list-body` を薄いピンクの下敷きとして見えるよう余白・角丸・背景色を調整する。
6. 必要に応じて [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) の `sticker-list-header` 背景と余白を調整し、ヘッダーだけ別色に見えないようにする。
7. 背景変更後にセル視認性が落ちる場合のみ、[HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) の `.sticker-cell`、`.sticker-cell--selected`、`.sticker-cell__count` を最小限調整する。
8. UXML 構造変更が必要な場合のみ [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) を更新し、name 属性の互換性を維持する。
9. UXML を変更した場合のみ [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の取得名と null チェックを同期する。
10. Unity 上で通常 HUD、空表示、複数件表示、スクロール、選択状態、ショップ開閉、フェーズ切替の退行がないことを確認する。
