# 妖精発見演出タップ進行改修 ToDo

## フェーズ1: 現行単一クリップ前提の置換準備
- [ ] [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs) の `clipName` 単一フィールド依存箇所を洗い出す。
- [ ] イン演出、タップ待機、アウト演出を表す内部状態を定義する。
- [ ] 多重再生防止と入力ロック解除保証を、新状態遷移でも維持できるよう整理する。

## フェーズ2: 発見演出プレイヤーの状態機械化
- [ ] `clipName` を `introClipName` と `outroClipName` に分割する。
- [ ] `TryPlay(Action onCompleted)` がイン用・アウト用クリップ両方を検証してから再生開始するよう変更する。
- [ ] イン再生完了後に待機状態へ遷移する coroutine を実装する。
- [ ] 待機中に `Input.GetMouseButtonDown(0)` または `TouchPhase.Began` を 1 回だけ受け付ける処理を追加する。
- [ ] タップ後にアウト演出を再生し、完了後に `onCompleted` を呼ぶよう変更する。
- [ ] イン再生中タップとアウト再生中連打を無視し、多重進行を防ぐ。

## フェーズ3: 破棄タイミングとフォールバック整理
- [ ] [Assets/Scripts/Sticker/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs) の `CompletePeel()` が新フロー前提でも 1 回だけ動くことを確認する。
- [ ] 妖精登録と発見ログ出力が `TryPlay()` 前に 1 回だけ実行されることを維持する。
- [ ] 発見演出再生成功時は、アウト演出完了後にのみ `Destroy(gameObject)` されるよう維持する。
- [ ] イン用またはアウト用クリップ未設定時は `Destroy(gameObject, 0.5f)` にフォールバックする経路を維持する。

## フェーズ4: 入力ロックと後始末の保証
- [ ] [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) の `SetPeelingLocked` 利用前提が崩れないことを確認する。
- [ ] `FairyDiscoveryAnimationPlayer` の `OnDisable()` / `OnDestroy()` で待機中でもロック解除されることを保証する。
- [ ] 異常終了時に callback 二重実行や state 残留が起きないよう後始末を入れる。

## フェーズ5: シーン設定の更新
- [ ] [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) の `FairyDiscoveryAnimationPlayer` serialized field を単一 `clipName` からイン用・アウト用へ更新する。
- [ ] `ObiRoot` の `Animation` にイン演出クリップとアウト演出クリップが登録されていることを Unity Editor 上で確認する。
- [ ] `Animation` の自動再生設定が意図せず発火しないことを確認する。

## フェーズ6: 手動確認
- [ ] 妖精ありシールをめくった時、イン演出が自動再生されることを確認する。
- [ ] イン演出完了後、自動では閉じずタップ待機になることを確認する。
- [ ] 待機中の 1 回タップでアウト演出が再生されることを確認する。
- [ ] 待機中タップで他シールの剥がしが開始されないことを確認する。
- [ ] アウト演出完了後にのみ対象シールが破棄されることを確認する。
- [ ] 妖精なしシールでは従来どおり発見演出なしで破棄されることを確認する。
- [ ] イン用またはアウト用クリップを外した異常時でも、入力ロックが残らずフォールバック破棄されることを確認する。
