# 所持金付きシールショップ機能 ToDo

1. [Assets/Scripts/Sticker/StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs) に価格フィールドと公開プロパティを追加する。
2. [Assets/Scripts/Currency/CurrencyBalanceSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Currency/CurrencyBalanceSource.cs) を新規作成し、初期残高 1000 円、残高参照、減算 API、変更通知を実装する。
3. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `CurrencyBalanceSource` 参照、`money-label` 取得、所持金表示更新処理を追加する。
4. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の購読処理を更新し、所持金変更時に HUD とショップフッターの金額が同期されるようにする。
5. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) のショップカード生成に価格表示と購入可否判定を追加する。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の購入ハンドラを更新し、所持金減算成功時のみ所持シール追加とログ出力を行うようにする。
7. [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) に価格ラベルと購入不可グレーアウト用スタイルを追加する。
8. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) に `CurrencyBalanceSource` を配置し、`HubScreenBinder` 参照と各シール価格を設定する。
9. 手動確認で、初期残高表示、価格表示、購入成功時の減算、購入不可カードのグレーアウト、所持一覧反映、ショップ継続表示、既存フェーズ無影響を検証する。
