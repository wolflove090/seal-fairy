# シール妖精配置と剥がしログ更新 ToDo

## フェーズ1: 既存処理の確認
- [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) を読み、実際にシールを生成している箇所を特定する。
- [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) を読み、めくり完了後に `Destroy(gameObject)` している箇所を特定する。
- 妖精有無を `MonoBehaviour` に直接持たせない方針で進めることを確認する。

## フェーズ2: 妖精管理データの追加
- `Assets/Scripts/StickerRuntimeRegistry.cs` を新規作成する。
- シールと妖精有無を対応付ける管理データを実装する。
- `Register` と `TryConsumeFairy` の 2 操作を最低限用意する。
- 同じシールで二重にログが出ないよう、参照時にデータを削除する実装にする。

## フェーズ3: 配置時の妖精割り当て追加
- [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) の `SpawnSticker` 周辺を修正する。
- 実際に生成したシールだけを登録対象にする。
- `UnityEngine.Random` を直接使い、50% 判定を配置時に 1 回だけ行う。
- 判定結果を `StickerRuntimeRegistry` に登録する。
- テンプレート用の非表示シールを誤って登録しないことを確認する。

## フェーズ4: めくり完了時ログの追加
- [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) の自動めくり完了分岐を修正する。
- `Destroy(gameObject)` の前に `StickerRuntimeRegistry` へ問い合わせる。
- 妖精がいた場合のみ `Debug.Log` を出力する。
- 妖精がいなかった場合、または registry 未登録の場合は何も出力しない。
- ログは「めくり完了時」に 1 回だけ出ることを担保する。

## フェーズ5: 後始末と安全性確認
- めくり完了後に registry 側のデータが削除されることを確認する。
- 未登録シールや初期デモシールをめくっても例外が出ないことを確認する。
- 既存のシール配置や自動めくり破棄の流れを壊していないことを確認する。

## フェーズ6: 手動確認
- シールを複数枚配置し、通常どおり配置できることを確認する。
- 複数のシールを順にめくり、妖精がいたシールでのみ Console にログが出ることを確認する。
- 妖精がいなかったシールではログが出ないことを確認する。
- 同じシールで同一ログが重複しないことを確認する。
- めくり後にシールが既存どおり破棄されることを確認する。

## 完了条件
- 配置した各シールに対して、50% の確率で妖精有無が内部登録される。
- 妖精有無は `MonoBehaviour` 外の管理データで保持される。
- めくり完了時に、妖精がいたシールでのみ Console ログが出る。
- 妖精がいないシールではログが出ない。
- 同一シールで重複ログが出ない。
