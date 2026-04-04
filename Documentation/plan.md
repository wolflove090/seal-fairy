# 妖精発見演出タップ進行改修 実装計画

## 実装方針
- 現在の発見演出は [FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs) が単一クリップ `discovery` を `WaitForSeconds(clip.length)` で待機する構成である。これを「イン再生」「タップ待機」「アウト再生」の状態遷移を持つ専用フローへ置き換える。
- 演出進行中の入力制御は既存の [SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) の `SetPeelingLocked` を引き続き利用し、他シールの剥がし禁止は既存責務のまま維持する。
- タップ待機中に許可する唯一の入力は `FairyDiscoveryAnimationPlayer` 内で検知し、`PeelSticker3D` や `SealPhaseController` に待機状態専用の分岐を増やしすぎない。
- [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs) は「妖精登録とログ出力を 1 回だけ行う」「演出プレイヤーへ完了コールバックを渡す」「フォールバック破棄へ落とす」の責務に留める。
- シーン設定は [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) 上の `FairyDiscoveryAnimationPlayer` Inspector 設定を更新し、単一 `clipName` からイン用・アウト用クリップ名を持てる構成へ合わせる。

## 変更対象ファイル一覧
- [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs)
  - 単一クリップ前提の再生処理を、イン/待機/アウトの状態機械へ変更する。
- [Assets/Scripts/Sticker/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs)
  - 発見演出起動時の完了待ち契約を新フローに合わせ、フォールバック破棄条件を明示する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `FairyDiscoveryAnimationPlayer` の serialized field をイン用・アウト用クリップ名へ差し替える。
- [Documentation/要件書/妖精発見演出タップ進行改修要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精発見演出タップ進行改修要件書.md)
  - 実装判断の基準として参照する。

## データフロー / 処理フロー
1. [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs) の `CompletePeel()` が妖精ありシールを確定する。
2. `StickerRuntimeRegistry.TryConsumeFairy()` で割り当て済み妖精を 1 回だけ消費し、`FairyCollectionService.TryRegisterDiscovery()` と `FairyDiscoveryLogger.LogDiscovered()` を現行どおり実行する。
3. `PeelSticker3D` は `FairyDiscoveryAnimationPlayer.TryPlay(onCompleted)` を呼び、成功時は破棄責務をコールバック側へ委譲する。
4. `FairyDiscoveryAnimationPlayer` は `sealPhaseController.SetPeelingLocked(true)` を呼び、剥がし入力を全停止する。
5. プレイヤーはイン用クリップを再生し、終了までは追加タップを無視する。
6. イン用クリップ終了後、プレイヤーは待機状態へ遷移し、`Input.GetMouseButtonDown(0)` または `TouchPhase.Began` を 1 回検知するまで待つ。
7. タップを検知したらアウト用クリップを再生し、完了後にロック解除して `onCompleted` を呼ぶ。
8. `PeelSticker3D` は `onCompleted` で対象シールを破棄する。
9. 途中で `Animation` 未設定、クリップ未登録、再生失敗があれば、プレイヤーはロック解除を保証して `false` を返すか完了相当処理へ落とし、`PeelSticker3D` は既存の短い遅延破棄へフォールバックする。

## 詳細設計

### 1. `FairyDiscoveryAnimationPlayer` の状態機械
- 現状の `isPlaying` boolean だけでは「イン再生中」と「タップ待機中」を区別できないため、少なくとも以下を表せる内部状態へ変更する。
  - Idle
  - PlayingIntro
  - WaitingForTap
  - PlayingOutro
- `TryPlay(Action onCompleted)` は `Idle` でのみ受け付ける。
- イン用クリップの長さ待機は現状どおり `AnimationClip.length` を使用する。
- 待機中は coroutine 内で毎フレーム入力を監視するか、`Update` ベースの 1 箇所へ集約する。既存ファイル構成を崩さないため、この計画では coroutine 内待機を優先する。
- アウト完了時は `onCompleted` 実行前に state を `Idle` に戻し、入力ロック解除漏れを防ぐ。

### 2. クリップ参照の serialized field 設計
- 現在の `clipName` は [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity#L3093) で `discovery` が設定されている。
- これを `introClipName` と `outroClipName` の 2 フィールドへ分割する。
- 初期値はアニメーション資産の命名が確定するまで仮に `discovery_in` / `discovery_out` とし、実装時に実在クリップ名へ合わせてシーン設定を更新する。
- `GetClip()` は両クリップに対して個別に行い、どちらかが欠けた時点で再生開始しない。

### 3. タップ待機中の入力判定
- `PeelSticker3D` 側も `Input.GetMouseButtonDown(0)` / `TouchPhase.Began` を使っているため、待機中タップと剥がし開始タップが競合しないよう、`SealPhaseController.SetPeelingLocked(true)` を維持したままプレイヤー側だけが入力を見る。
- タップ受付関数は `TryGetAdvanceInputThisFrame()` のような private static helper に切り出し、マウスとタッチの分岐を 1 箇所にまとめる。
- イン再生中に押されたままの入力を誤って待機直後に拾わないよう、待機開始フレームから新規 `Began` / `Down` のみを対象にする。

### 4. `PeelSticker3D` 側の契約整理
- 現状の `CompletePeel()` は `TryPlay(() => Destroy(gameObject))` 成功時にのみ即 return し、失敗時は `Destroy(gameObject, 0.5f)` へ落としている。
- この契約は維持し、成功時は「アウト演出完了後に破棄」、失敗時は「0.5 秒遅延破棄」とする。
- 妖精登録とログ出力は `TryPlay()` 呼び出し前に既に完了しているため、新フローでも多重実行防止は `isPeelComplete` と `TryConsumeFairy()` に依存して成立する。
- `Destroy(gameObject)` をコールバック内で直接呼ぶ構成はそのままでよいが、将来の可読性のため `HandleDiscoveryAnimationCompleted()` のような private method に切り出す余地を残す。

### 5. ライフサイクル異常時の後始末
- 現状の `OnDisable()` / `OnDestroy()` は `ReleaseLockIfNeeded()` のみを行う。これに加えて、待機中 callback の二重呼び出しを防ぐため、完了 callback を field 保持するなら null クリアが必要になる。
- 再生失敗時に `isPlaying = false` として終了している箇所は、state 設計へ置き換えたうえで必ず `sealPhaseController?.SetPeelingLocked(false)` を通るよう整理する。
- `Object.FindAnyObjectByType<FairyDiscoveryAnimationPlayer>()` を使う [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs#L233) の取得方法は今回は変更しない。計画対象は演出進行改修に限定する。

## リスクと対策
- イン/アウトどちらかのクリップ名がシーン設定と一致しないと、妖精ありシールだけ演出なしフォールバックになる。
  - 対策: `GetClip(introClipName)` と `GetClip(outroClipName)` の失敗ログを分け、`Main.unity` の serialized value を同一変更で更新する。
- 待機中タップをプレイヤー側で拾うと、モバイルと Editor で入力差異が出る可能性がある。
  - 対策: 既存 `PeelSticker3D.TryGetPointerDownPosition()` と同じ条件を使う helper を再利用または同等実装にする。
- イン完了直後に、イン再生開始時のタップが誤って待機入力として消費される可能性がある。
  - 対策: `Began` / `GetMouseButtonDown` のみ採用し、押しっぱなし状態は無視する。
- 途中でオブジェクトが無効化されると、入力ロックだけ残る可能性がある。
  - 対策: `OnDisable()` と `OnDestroy()` で state と callback を破棄し、`ReleaseLockIfNeeded()` を継続利用する。
- `PeelSticker3D` 側に待機ロジックまで持ち込むと責務が崩れる。
  - 対策: 待機判定は `FairyDiscoveryAnimationPlayer` に閉じ込め、`PeelSticker3D` は bool 成功/失敗の契約のみ保つ。

## 検証方針
- 手動確認1: 妖精ありシールをめくると、イン演出が再生された後、自動では終了せず待機状態に入ることを確認する。
- 手動確認2: 待機中に 1 回タップするとアウト演出が再生され、その完了後にシールが破棄されることを確認する。
- 手動確認3: 待機中のタップで他シールの剥がしが始まらないことを確認する。
- 手動確認4: イン演出中に連打しても、アウト演出が先走らないことを確認する。
- 手動確認5: アウト演出中の連打で多重破棄や例外が発生しないことを確認する。
- 手動確認6: 妖精なしシールでは従来どおり演出なしで 0.5 秒後に破棄されることを確認する。
- 手動確認7: イン用またはアウト用クリップ名を意図的に外した状態で、警告ログが出たうえで入力ロックが残らずシールがフォールバック破棄されることを確認する。

## コードスニペット
```csharp
[SerializeField] private string introClipName = "discovery_in";
[SerializeField] private string outroClipName = "discovery_out";

private enum DiscoveryPlaybackState
{
    Idle,
    PlayingIntro,
    WaitingForTap,
    PlayingOutro
}
```

```csharp
public bool TryPlay(Action onCompleted)
{
    if (playbackState != DiscoveryPlaybackState.Idle)
    {
        Debug.LogWarning("FairyDiscoveryAnimationPlayer: 発見演出の多重再生はできません。", this);
        return false;
    }

    if (!TryGetClips(out AnimationClip introClip, out AnimationClip outroClip))
    {
        return false;
    }

    StartCoroutine(PlayRoutine(introClip, outroClip, onCompleted));
    return true;
}
```

```csharp
private IEnumerator PlayRoutine(AnimationClip introClip, AnimationClip outroClip, Action onCompleted)
{
    playbackState = DiscoveryPlaybackState.PlayingIntro;
    sealPhaseController?.SetPeelingLocked(true);

    if (!obiAnimation.Play(introClip.name))
    {
        ReleaseLockIfNeeded();
        yield break;
    }

    yield return new WaitForSeconds(introClip.length);

    playbackState = DiscoveryPlaybackState.WaitingForTap;
    yield return new WaitUntil(TryGetAdvanceInputThisFrame);

    playbackState = DiscoveryPlaybackState.PlayingOutro;
    if (!obiAnimation.Play(outroClip.name))
    {
        ReleaseLockIfNeeded();
        onCompleted?.Invoke();
        yield break;
    }

    yield return new WaitForSeconds(outroClip.length);

    playbackState = DiscoveryPlaybackState.Idle;
    sealPhaseController?.SetPeelingLocked(false);
    onCompleted?.Invoke();
}
```
