# シールショップ機能 ToDo

1. [OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs) に、所持シールを先頭追加できる購入反映 API を追加する。
2. [StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) の初期選択責務を確認し、自動選択しない仕様へ合わせて必要なら整理する。
3. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の所持一覧管理で、重複購入に対応できない `Dictionary<StickerDefinition, VisualElement>` を置き換える。
4. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `BuildStickerList()` を更新し、初期所持 0 件表示、購入後再構築、未選択維持を満たすようにする。
5. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) にショップ購入ハンドラを追加し、カード押下で所持一覧先頭へ追加して購入ログを出すようにする。
6. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) で、既存選択がある状態の購入後も選択が維持されることを確認し、必要な補正を入れる。
7. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) の `OwnedStickerInventorySource` 初期リストを 0 件にする。
8. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) の `StickerShopCatalogSource` に販売シールを設定し、`HubScreenBinder` の参照割り当てを確認する。
9. 手動確認で、初期所持 0 件、`Ready` 遷移、初回購入後未選択、手動選択後の配置、重複購入、購入ログ、ショップ画面継続表示を検証する。
