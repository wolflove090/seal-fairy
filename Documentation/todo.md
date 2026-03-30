# 妖精コレクション画面ブラッシュアップ ToDo

1. [妖精コレクション画面ブラッシュアップ要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精コレクション画面ブラッシュアップ要件書.md) を再確認し、右側固定パネル、右上 `X`、3 列カード、右下 `X/Y` が必須要件であることを確認する。
2. [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml) の現状構造を整理し、維持すべき `name` が `fairy-collection-overlay`、`fairy-collection-backdrop`、`fairy-collection-scroll-view`、`fairy-collection-empty-label`、`fairy-collection-count-label`、`fairy-collection-close-button` であることを確認する。
3. [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml) を、ヘッダー、内側フレーム、フッターを持つ新レイアウトへ更新する。
4. [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) の既存灰色オーバーレイと単純カード定義を、右側大型ピンクパネルと黄緑カード前提のスタイルへ置き換える。
5. スクロールコンテナのカード幅、余白、パネル幅を調整し、1920x1080 基準で 3 列レイアウトになるようにする。
6. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `RefreshFairyCollection()` を確認し、空表示、発見数表示、スクロールビュー更新が新レイアウトでも成立するよう文言を調整する。
7. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateFairyCard()` を更新し、名前、画像フレーム、`好きなシール：`、値表示の 4 ブロック構造へ組み替える。
8. 未発見カードが `？？？` と `未発見` を表示し、発見済みカードが `クール/ワイルド` を表示することを C# と USS の両面で確認する。
9. Unity 上で開閉導線、ショップとの排他表示、3 列表示、縦スクロール、右下 `X/Y` 表示、未発見見た目を確認する。
