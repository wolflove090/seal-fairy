# シール妖精発見エフェクト ToDo

## フェーズ1: 既存処理の再確認
- [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) の `SpawnSticker` で、配置直後に妖精判定と registry 登録が行われていることを確認する。
- [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) の `Update` で、めくり完了時に即時 `Destroy(gameObject)` している箇所を確認する。
- [StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs) の `TryConsumeFairy` が重複ログ防止に使えることを確認する。

## フェーズ2: エフェクト prefab 参照方法の決定
- `KiraKiraEffect.prefab` を `Resources.Load` で取得できる配置へ移す。
- `TapStickerPlacer` の自動生成構成を壊さず、コードだけで prefab を取得できるようにする。
- 参照取得失敗時でもシール配置処理が落ちないように `null` ガードを入れる。

## フェーズ3: 配置時のエフェクト仕込み追加
- [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) にエフェクト生成 helper を追加する。
- 妖精ありシールにだけ `KiraKiraEffect.prefab` を生成する。
- エフェクトを対象シールの子オブジェクトにする。
- localPosition を `Vector3.zero`、localRotation を `Quaternion.identity`、localScale を `Vector3.one` に初期化する。
- 妖精なしシールやテンプレートシールにはエフェクトを生成しない。

## フェーズ4: めくり完了後の 2 秒遅延破棄
- [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) に完了状態フラグと遅延タイマーを追加する。
- めくり完了時にログ出力と破棄待機開始を 1 回だけ行うようにする。
- 即時 `Destroy(gameObject)` を削除し、2 秒経過後の破棄へ置き換える。
- 遅延中は再タップや重複完了処理が入らないようにする。

## フェーズ5: 既存ログとの整合確認
- 妖精ありシールでは既存ログが 1 回だけ出ることを維持する。
- 妖精なしシールではログが出ないことを維持する。
- 初期デモシールはエフェクト対象外で、未登録でも例外が出ないことを確認する。

## フェーズ6: Unity 上での手動確認
- 妖精ありシールにのみエフェクト子オブジェクトが付くことを確認する。
- 未剥がし状態でキラキラが見えないことを確認する。
- 妖精ありシールをめくるとキラキラが見えることを確認する。
- 妖精なしシールではキラキラが見えないことを確認する。
- めくり完了後、シールが約 2 秒残ってから破棄されることを確認する。
- 同一シールでログ重複、エフェクト二重生成、重複破棄が起きないことを確認する。

## 完了条件
- 配置した妖精ありシールにのみ `KiraKiraEffect.prefab` が子オブジェクトとして仕込まれる。
- 未剥がし状態でエフェクトは見えず、めくり後に見える。
- シールはめくり完了後に即時破棄されず、2 秒後に破棄される。
- 妖精ありシールでのみ既存ログが 1 回出る。
- 妖精なしシールと初期デモシールで例外や不要演出が発生しない。
