# 妖精データJSONロード ToDo

## フェーズ1: データ配置の準備
- [ ] `Assets/GameResources/Resources/Fairy/` ディレクトリを作成する。
- [ ] [Assets/GameResources/Fairy/05.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/05.png) を `Resources` 配下へ移設または複製する。
- [ ] [Assets/GameResources/Fairy/07.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/07.png) を `Resources` 配下へ移設または複製する。
- [ ] [Assets/GameResources/Fairy/13.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/13.png) を `Resources` 配下へ移設または複製する。
- [ ] `Resources.Load<Sprite>()` で読めるパス命名を統一する。

## フェーズ2: JSON マスタの作成
- [ ] `Assets/GameResources/Resources/Fairy/fairy_catalog.json` を追加する。
- [ ] [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity#L2806) にある `バラちゃん` の `id`、`displayName`、`weight`、`favoriteStickerText`、`flavorText` を JSON へ転記する。
- [ ] [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity#L2806) にある `ウルフちゃん` の `id`、`displayName`、`weight`、`favoriteStickerText`、`flavorText` を JSON へ転記する。
- [ ] [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity#L2806) にある `もっさん` の `id`、`displayName`、`weight`、`favoriteStickerText`、`flavorText` を JSON へ転記する。
- [ ] 各レコードへ `iconResourcePath` を設定する。

## フェーズ3: ローダーとリポジトリの追加
- [ ] `FairyCatalogDto` と `FairyRecordDto` を追加する。
- [ ] `FairyCatalogLoader` を追加し、`Resources.Load<TextAsset>()` と `JsonUtility.FromJson()` で JSON を読み込めるようにする。
- [ ] ローダーにレコード検証処理を追加する。
- [ ] 不正レコードスキップ時に、対象レコードが分かるログを出す。
- [ ] JSON 欠落または解析失敗時に `Debug.LogError` を出す。
- [ ] `FairyCatalogRepository` を追加し、起動時に 1 回だけロードする。

## フェーズ4: 既存参照経路の置換
- [ ] [Assets/Scripts/Fairy/FairyCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogSource.cs) から Inspector 配列を除去する。
- [ ] [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) を JSON DTO から生成できる形へ変更する。
- [ ] [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) が追加修正なし、または最小修正で継続利用できることを確認する。
- [ ] [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) が追加修正なし、または最小修正で継続利用できることを確認する。

## フェーズ5: シーン移行
- [ ] [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) から `FairyCatalogSource.fairies` の旧直列化データを除去する。
- [ ] シーン上の `FairyCatalogSource` コンポーネント参照が維持されていることを確認する。
- [ ] Play Mode 再実行時にキャッシュが初期化されることを確認する。

## フェーズ6: 動作確認
- [ ] 起動直後に妖精一覧が 3 件ロードされることを確認する。
- [ ] シール配置時の妖精抽選が動作することを確認する。
- [ ] 妖精コレクション一覧に JSON 由来の表示名と画像が出ることを確認する。
- [ ] 妖精詳細に JSON 由来の好きなシールとフレーバーが出ることを確認する。
- [ ] JSON 欠落時に `Debug.LogError` が出て安全継続することを確認する。
- [ ] 不正レコードのみスキップされ、他レコードは利用可能なことを確認する。
- [ ] `iconResourcePath` 不正時に画像だけ null 扱いとなり致命的エラーにならないことを確認する。
