# シール剥がしフェーズ ToDo

- `StickerStatus`、`PeelingPhaseDomainService`、`PeelingPhaseResult`、`StickerLogMessageFactory` を追加し、剥がしフェーズのドメインモデルを定義する。
- `PeelingPhaseUseCase`、`PeelingViewModel`、`StickerRegistry` を実装し、シール選択、演出完了、ログ出力を仲介できるようにする。
- `StickerHitTestAdapter` を実装し、クリック対象の `StickerId` を取得する。
- `StickerPeelPresenter` を実装し、既存 `PeelSticker3D` を参考にした剥がし演出を再構成する。
- `StickerCleanupPresenter` を実装し、剥がし完了時とフェーズ終了時の表示オブジェクト削除を統一する。
- `UnityDebugLogger` を実装し、妖精入りシール剥がし時のみログを出力する。
- `GameFlowService` と `SealFairyGameController` を更新し、剥がしフェーズ遷移と未剥がしシール全削除を組み込む。
- `Assets/Main.unity` と HUD を更新し、剥がしフェーズ時の UI 文言と入力を接続する。
- 必要に応じて `PeelSticker3D` を演出専用へ整理し、直接入力責務を外す。
- EditMode テストと手動確認で、剥がし開始、重複剥がし拒否、剥がし後の消滅、妖精ログ、フェーズ終了時全消去を検証する。
