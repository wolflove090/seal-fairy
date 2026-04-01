# 妖精コレクション未発見画像transparent表示 ToDo

1. [妖精コレクション未発見画像transparent表示要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精コレクション未発見画像transparent表示要件書.md) を再確認し、未発見画像を `transparent` 素材へ差し替えること、補足文は `クール/ワイルド` を維持することを確認する。
2. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `InitializeFairyCollectionUi()` と `CreateFairyCard()` の現状を確認し、`transparent` 素材の読込場所と適用分岐を確定する。
3. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に未発見画像用のキャッシュフィールドを追加し、`Resources.Load<Texture2D>("transparent")` で [transparent.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Texture/Resources/transparent.png) を取得する。
4. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateFairyCard()` を更新し、発見済みは `fairy.Icon.texture`、未発見は `transparent` 素材、取得失敗時のみ `.fairy-card__image--undiscovered` を使う分岐へ変更する。
5. 未発見カードで `？？？` と `クール/ワイルド` が維持され、発見済みカードの画像表示ロジックに影響しないことをコード上で確認する。
6. [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) の `.fairy-card__image--undiscovered` を確認し、`transparent` 素材読込成功時には不要な見た目上書きが発生しないことを整理する。
7. Unity 上で妖精コレクションを開き、発見済みカードは通常アイコン、未発見カードは `transparent` 素材、件数表示 `X/Y`、スクロール、閉じる操作が正常であることを確認する。
8. `transparent` 素材を一時的に読めないケースを想定し、未発見カードがフォールバック表示でもエラー停止しないことを確認する。
