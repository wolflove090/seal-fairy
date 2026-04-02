# 妖精ごとの好きなシールとフレーバー個別設定 ToDo

1. [Documentation/要件書/妖精ごとの好きなシールとフレーバー個別設定要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精ごとの好きなシールとフレーバー個別設定要件書.md) を再確認し、好きなシールは自由入力文字列、未設定時は `*****` フォールバックであることを確認する。
2. [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) に、`favoriteStickerText` と `flavorText` の serialized field、および公開プロパティを追加する。
3. [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) の `flavorText` に `TextArea` 属性を付け、Inspector で複数行入力しやすくする。
4. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の固定文言定数を、個別設定未入力時のフォールバック定数 `*****` へ置き換える。
5. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に、`ResolveFavoriteStickerText(FairyDefinition fairy)` と `ResolveFlavorText(FairyDefinition fairy)` を追加する。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `ApplyFairyDetail()` を更新し、好きなシールとフレーバーを `FairyDefinition` から表示するよう変更する。
7. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateFairyCard()` を更新し、発見済み妖精カードの「好きなシール」欄に個別設定値を表示する。
8. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) で、未発見妖精カードでは個別設定値を参照しないことを確認する。
9. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を Unity Editor で開き、`FairyCatalogSource` の各妖精に好きなシールとフレーバーを入力する。
10. Unity 上で、妖精ごとの差分表示、未設定時の `*****`、未発見時の伏せ表示、既存開閉導線に回帰がないことを確認する。
