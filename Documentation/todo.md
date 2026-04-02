# 妖精コレクション詳細画面 ToDo

1. [Documentation/要件書/妖精コレクション詳細画面要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精コレクション詳細画面要件書.md) を再確認し、未発見妖精は選択不可、好きなシールとフレーバーは固定文言であることを確認する。
2. [Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml) に、詳細モーダル用の overlay / backdrop / panel / close button / name / image / text 領域を追加する。
3. [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) に、詳細モーダルのピンク基調スタイルと、カードの選択可能・未発見状態スタイルを追加する。
4. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に、詳細モーダル参照フィールドと固定文言定数を追加する。
5. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `InitializeFairyCollectionUi()` を更新し、詳細モーダル要素を取得して初期状態を非表示にする。
6. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `OnEnable()` / `OnDisable()` を更新し、詳細モーダルの閉じるボタンと背景タップのイベント購読を追加する。
7. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に `OpenFairyDetail()`、`ApplyFairyDetail()`、`CloseFairyDetail()` を追加する。
8. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateFairyCard()` を更新し、発見済み妖精のみ詳細を開けるカードへ変更し、未発見はクリック不可にする。
9. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `OpenFairyCollection()` と `CloseFairyCollection()` を更新し、詳細モーダル残留が起きないようにする。
10. Unity 上で、発見済みカードの詳細表示、未発見カードの非選択、`X` ボタンと背景タップでの閉じる動作、一覧閉鎖時の詳細同時閉鎖、ショップとの排他表示を確認する。
