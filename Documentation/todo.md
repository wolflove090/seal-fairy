# 妖精コレクション機能 ToDo

- `Assets/Scripts/Fairy/FairyDefinition.cs` を追加し、`id`、`displayName`、`weight` を持つ妖精定義データを作成する。
- `Assets/Scripts/Fairy/FairyCatalogSource.cs` を追加し、Inspector から複数妖精を設定できる `MonoBehaviour` を作成する。
- `Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs` を追加し、重み付きランダム抽選ロジックを実装する。
- `Assets/Scripts/Fairy/StickerFairyAssignment.cs` を追加し、シール単位の妖精割当情報を表現する。
- `Assets/Scripts/StickerRuntimeRegistry.cs` を更新し、`bool hasFairy` ではなく妖精割当情報を保持・消費できるようにする。
- `Assets/Scripts/Fairy/FairyCollectionState.cs` を追加し、セッション中の獲得済み妖精 ID を保持できるようにする。
- `Assets/Scripts/Fairy/FairyCollectionService.cs` を追加し、将来の保存差し替えを見据えた獲得状態の読み書き窓口を用意する。
- `Assets/Scripts/Fairy/FairyDiscoveryLogger.cs` を追加し、新規発見/既発見のログ出力を分けられるようにする。
- `Assets/Scripts/TapStickerPlacer.cs` を更新し、妖精カタログ参照、重み付き抽選、割当登録へ置き換える。
- `Assets/Scripts/PeelSticker3D.cs` を更新し、剥がし完了時に割当妖精を消費し、獲得状態更新とログ出力を行うようにする。
- `Assets/Main.unity` を更新し、`FairyCatalogSource` をシーンへ追加して `TapStickerPlacer` から参照させる。
- 手動確認で、複数妖精登録、重み付き出現、0 件時の無ログ動作、新規発見ログ、既発見ログ、セッション中の獲得状態保持を検証する。
