# HUD操作ボタン白ベースブラッシュアップ ToDo

1. [HUD操作ボタン白ベースブラッシュアップ要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/HUD操作ボタン白ベースブラッシュアップ要件書.md) を再確認し、対象が `ready-button`、`shop-button`、`fairy-button` の3ボタンであることを確認する。
2. [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) の `#sticker-shop-close-button` と `.sticker-shop-card__price-plate` を参照し、白プレート表現の基準値を抽出する。
3. [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) で各ボタンがテキストのみで構成されていることを確認し、USS だけで対応できるか判断する。
4. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) の `#ready-button`、`#shop-button`、`#fairy-button` の既存グレー定義を整理し、共通スタイルへ統合する。
5. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) で白ベース、濃い文字色、立体的なボーダー、hover / active 状態を追加する。
6. `ready-button` にだけ主操作差分を入れ、`shop-button` / `fairy-button` は同シリーズのサブ操作として意匠を揃える。
7. USS だけで表現しきれない場合のみ [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) に装飾用子要素を追加し、Button の `name` を維持する。
8. `ready-button` の文言更新方式に影響が出る場合のみ [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を最小限修正する。
9. Unity 上で通常 HUD、フェーズ変更時のラベル更新、ショップ起動、妖精画面起動、hover / active 見た目、レイアウト崩れの有無を確認する。
