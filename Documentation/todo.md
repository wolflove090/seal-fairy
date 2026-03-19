# シール配置からシールめくりへのフェーズ遷移 ToDo

- `SealGamePhase` を追加し、配置フェーズとめくりフェーズの列挙型を定義する。
- `SealPhaseEventHub` を追加し、フェーズ切替要求イベントとフェーズ変更通知イベントを仲介する plain C# class として実装する。
- `SealPhaseBootstrap` を追加し、`SealPhaseEventHub` の生成と `HudScreenBinder` / `SealPhaseController` への注入を実装する。
- `SealPhaseController` を追加し、イベント購読、初期フェーズ設定、フェーズ切替、入力有効 / 無効反映、戻り時の未めくりシール全削除を実装する。
- `HudScreenBinder` を追加し、UI Toolkit の `ready-button` を取得してイベント中継トリガーと文言更新を接続する。
- `TapStickerPlacer` に配置入力の有効 / 無効フラグと切替 API を追加する。
- `PeelSticker3D` にめくり入力の実行時有効 / 無効フラグと切替 API を追加する。
- `StickerRuntimeRegistry` を拡張し、妖精有無に加えて配置済みシール参照の追跡と全削除用 API を追加する。
- `Assets/Main.unity` を更新し、フェーズ制御と HUD 接続に必要なコンポーネントを配置する。
- 手動確認で、初期配置可 / 初期めくり不可 / 右上ボタンでの往復遷移 / めくりフェーズ中の配置禁止 / 戻り時の未めくりシール全削除 / ボタン文言更新を検証する。
