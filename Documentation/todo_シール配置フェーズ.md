# シール配置フェーズ ToDo

- `GamePhase`、`StickerId`、`WorldPoint`、`PlacedSticker`、`StickerCollection` を追加し、配置フェーズの土台となるドメインモデルを定義する。
- `PlacementPhaseState` と `PlacementPhaseDomainService` を実装し、残数 10、重なり許可、妖精確率 50% の配置ルールを表現する。
- `UnityRandomService` を追加し、妖精判定を Unity 依存側から注入できるようにする。
- `PlacementPhaseUseCase` と `PlacementViewModel` を実装し、入力から配置結果と HUD 表示データを組み立てる。
- `PointerInputAdapter` を実装し、マウスとタッチを共通入力へ正規化する。
- `WorldPlaneRaycaster` を実装し、画面座標を配置平面上のワールド座標へ変換する。
- `PlacementPreviewPresenter` を実装し、配置フェーズ中のみプレビューシールを追従表示する。
- `StickerSpawner` を実装し、配置済みシールの表示オブジェクトを生成する。
- `GameHudPresenter` と `Assets/UI/GameHud.uxml` / `Assets/UI/GameHud.uss` を実装し、残数表示とフェーズ完了ボタンを更新する。
- `SealFairyGameController` と `Assets/Main.unity` を更新し、配置フェーズの初期化と接続を行う。
- EditMode テストと手動確認で、10 枚上限、重なり許可、プレビュー追従、カメラ移動を検証する。
