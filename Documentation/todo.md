# 妖精発見演出再生 ToDo

1. [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs) を追加し、`ObiRoot.Animation` の `discovery` を再生して完了通知を返す coroutine ベースの処理を実装する。
2. [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) に演出中ロック状態を追加し、現在フェーズと合わせて剥がし入力の有効 / 無効を再計算できるようにする。
3. [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) で妖精あり完了時に `FairyDiscoveryAnimationPlayer` を経由した完了待ち破棄へ変更する。
4. [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) で再生失敗時フォールバックと重複破棄防止を確認する。
5. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で `ObiRoot.Animation` のオート再生を無効化する。
6. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で `FairyDiscoveryAnimationPlayer` を配置し、`ObiRoot.Animation` と `SealPhaseController` を Inspector 接続する。
7. 手動確認で、妖精あり再生、妖精なし非再生、演出完了後破棄、演出中の剥がし禁止、演出後復帰、フォールバック破棄を検証する。
