# 妖精ごとのシール排出テーブル ToDo

## フェーズ1: 妖精マスタ構造の更新
- [ ] [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) から `Weight` を削除する。
- [ ] 妖精が複数の好みシール設定を保持できるランタイム構造を追加する。
- [ ] 好みシール設定 1 件を表す `FairyStickerPreference` を追加する。
- [ ] `FairyDefinition` から好みシール一覧を参照できるようにする。

## フェーズ2: JSON DTO / ローダー更新
- [ ] [Assets/Scripts/Fairy/FairyCatalogDto.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogDto.cs) から `weight` を削除する。
- [ ] `PreferredStickerDto` を追加する。
- [ ] [Assets/Scripts/Fairy/FairyCatalogLoader.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogLoader.cs) で `preferredStickers` を読み込めるようにする。
- [ ] 同一妖精の同一 `stickerId` 重複設定をローダーで合算する。
- [ ] 不正な `stickerId` または `weight <= 0` を個別スキップする。
- [ ] 好み設定が 0 件でも妖精レコード自体はロード継続する。

## フェーズ3: 妖精 JSON の更新
- [ ] [Assets/GameResources/Resources/Fairy/fairy_catalog.json](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Resources/Fairy/fairy_catalog.json) から旧 `weight` を削除する。
- [ ] 各妖精レコードへ `preferredStickers` を追加する。
- [ ] `preferredStickers` に `stickerId` と `weight` を設定する。
- [ ] ジョウロやハートなど、実際の [StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs) の `Id` と一致する値になっていることを確認する。

## フェーズ4: 排出テーブルの事前構築
- [ ] `StickerFairyTableRepository` を追加する。
- [ ] ゲーム開始時に全妖精の好み設定からシール別排出テーブルを構築する。
- [ ] `SubsystemRegistration` で排出テーブルキャッシュをリセットする。
- [ ] 対象シールごとの総重みを確認できる構造にする。

## フェーズ5: 抽選ロジックの差し替え
- [ ] `StickerFairySelector` を追加する。
- [ ] キャッシュ済み排出テーブルを使って一次抽選を行う。
- [ ] 発見済み妖精が一次抽選で当選した場合に空振り扱いにする。
- [ ] 一次抽選候補が 0 件、または発見済み妖精当選で空振りしたときだけ、対象シールを好まない未発見妖精で救済候補を作る。
- [ ] 救済候補に対して `50%` 発火の等確率抽選を実装する。

## フェーズ6: 配置処理への統合
- [ ] [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) の `SpawnSticker()` から旧 `FairyWeightedRandomSelector.Select()` 呼び出しを除去する。
- [ ] `50%` の妖精入り判定を維持したまま、新セレクタで `StickerFairyAssignment` を作る。
- [ ] 妖精なしの場合も既存どおりシールを登録できることを確認する。
- [ ] 妖精ありの場合だけ既存の `AttachFairyEffect()` を呼ぶ。

## フェーズ7: 不要コードの整理
- [ ] [Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs) の参照が消えることを確認する。
- [ ] 不要になった `Weight` 関連コードやログ文言を除去する。
- [ ] `rg "Weight"` と `rg "FairyWeightedRandomSelector"` で残存参照を確認する。

## フェーズ8: 動作確認
- [ ] ジョウロに対して `A=7`, `B=4` を設定し、ゲーム開始時に一次抽選レンジが総重み 11 になることを確認する。
- [ ] 発見済み妖精が候補に残り、当選時に空振りになることを確認する。
- [ ] 一次抽選候補なし時、または発見済み妖精当選による空振り時にだけ救済抽選が走ることを確認する。
- [ ] 救済抽選が `50%` で発火することを確認する。
- [ ] 救済抽選が対象シールを好まない未発見妖精から等確率で選ぶことを確認する。
- [ ] 配置時に決まった妖精が、めくり時にそのまま消費されることを確認する。
