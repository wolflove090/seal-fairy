# 配置シール選択機能 ToDo

- 所持品管理データから所持シール一覧を取得するための `OwnedStickerDefinition` と `OwnedStickerInventorySource` を追加する。
- 選択中シールと未選択状態を保持する `StickerSelectionState` を追加する。
- `HudScreen.uxml` に左下の所持シール一覧パネルとスクロール要素を追加する。
- `HudScreen.uss` に左下パネル、画像セル、選択ハイライト、空状態表示のスタイルを追加する。
- `HudScreenBinder` を拡張し、所持シール一覧の動的生成、セル選択、選択状態の見た目更新、フェーズごとの表示切替を実装する。
- `TapStickerPlacer` を拡張し、固定 `templateSticker` 依存を選択中シール定義参照へ置き換える。
- `TapStickerPlacer` に未選択時の配置禁止、選択変更時のプレビュー差し替え、UI 操作中の入力抑止を追加する。
- `SealPhaseController` と必要な連携先を調整し、`StickerPlacement` 再入時に選択解除されるようにする。
- `Assets/Main.unity` の参照設定を更新し、一覧データ供給元と HUD バインダを接続する。
- 手動確認で、起動直後の先頭選択、一覧からの選択切替、選択中シールの配置反映、`StickerPeeling` 中の一覧非表示、再入時の未選択化、UI 操作中の誤配置防止、所持 0 件時の空表示を検証する。
