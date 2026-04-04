# 妖精発見演出タップ進行改修 作業手順書

## 目的
- 妖精発見演出を単一アニメーション再生から、イン演出とアウト演出の 2 段階構成へ変更する。
- 妖精ありシールをめくった後は、イン演出再生後に画面タップ待ちへ入り、タップを契機にアウト演出へ進める。
- 他シールの剥がし入力ロック、妖精登録、発見ログ、シール破棄タイミングの整合を維持する。

## 変更対象
- [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs)
- [Assets/Scripts/Sticker/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 事前確認
1. [FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs) を開き、現在が `clipName = "discovery"` の単一クリップ再生であることを確認する。
2. [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs) を開き、`CompletePeel()` が `TryPlay(() => Destroy(gameObject))` に成功した場合だけ即 return していることを確認する。
3. Unity Editor で [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開き、`SealPhaseSystem` に `FairyDiscoveryAnimationPlayer` が付いていることを確認する。
4. `ObiRoot` の `Animation` コンポーネントに、イン演出用とアウト演出用の 2 クリップを登録できる状態か確認する。

## 手順1: `FairyDiscoveryAnimationPlayer` を状態機械へ変更する
1. [FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs) の `clipName` を削除し、`introClipName` と `outroClipName` を追加する。
2. `isPlaying` boolean を廃止し、少なくとも `Idle`, `PlayingIntro`, `WaitingForTap`, `PlayingOutro` を表す enum を追加する。
3. 再生中 callback を扱うため、必要なら `Action pendingCompletion` と `Coroutine playingCoroutine` を field に追加する。

### 変更コード例
```csharp
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FairyDiscoveryAnimationPlayer : MonoBehaviour
{
    private enum DiscoveryPlaybackState
    {
        Idle,
        PlayingIntro,
        WaitingForTap,
        PlayingOutro
    }

    [SerializeField] private Animation obiAnimation;
    [SerializeField] private SealPhaseController sealPhaseController;
    [SerializeField] private string introClipName = "discovery_in";
    [SerializeField] private string outroClipName = "discovery_out";

    private DiscoveryPlaybackState playbackState = DiscoveryPlaybackState.Idle;
    private Coroutine playingCoroutine;
    private Action pendingCompletion;
```

4. `TryPlay(Action onCompleted)` は `Idle` 状態でのみ受け付け、イン用とアウト用の両クリップを取得できた場合だけ coroutine を開始するようにする。

### `TryPlay` の変更コード例
```csharp
public bool TryPlay(Action onCompleted)
{
    if (playbackState != DiscoveryPlaybackState.Idle)
    {
        Debug.LogWarning("FairyDiscoveryAnimationPlayer: 発見演出の多重再生はできません。", this);
        return false;
    }

    if (obiAnimation == null)
    {
        Debug.LogWarning("FairyDiscoveryAnimationPlayer: ObiRoot の Animation が未設定です。", this);
        return false;
    }

    AnimationClip introClip = obiAnimation.GetClip(introClipName);
    AnimationClip outroClip = obiAnimation.GetClip(outroClipName);
    if (introClip == null || outroClip == null)
    {
        Debug.LogWarning(
            $"FairyDiscoveryAnimationPlayer: intro='{introClipName}', outro='{outroClipName}' のいずれかが見つかりません。",
            this);
        return false;
    }

    pendingCompletion = onCompleted;
    playingCoroutine = StartCoroutine(PlayRoutine(introClip, outroClip));
    return true;
}
```

## 手順2: イン再生 → タップ待機 → アウト再生の coroutine を実装する
1. 現在の `PlayRoutine(AnimationClip clip, Action onCompleted)` を廃止し、イン用・アウト用を受け取る coroutine に置き換える。
2. 開始時に `sealPhaseController?.SetPeelingLocked(true)` を呼ぶ。
3. イン演出完了まではタップを進行入力として扱わない。
4. イン完了後、タップ待機状態に遷移し、新規タップ入力を 1 回受けたらアウト演出へ進める。
5. アウト完了後にロック解除し、`pendingCompletion?.Invoke()` を実行する。

### `PlayRoutine` の変更コード例
```csharp
private IEnumerator PlayRoutine(AnimationClip introClip, AnimationClip outroClip)
{
    playbackState = DiscoveryPlaybackState.PlayingIntro;
    sealPhaseController?.SetPeelingLocked(true);

    obiAnimation.Stop();
    if (!obiAnimation.Play(introClip.name))
    {
        Debug.LogWarning($"FairyDiscoveryAnimationPlayer: Animation clip '{introClip.name}' の再生に失敗しました。", this);
        ResetPlaybackState(false);
        yield break;
    }

    yield return new WaitForSeconds(introClip.length);

    playbackState = DiscoveryPlaybackState.WaitingForTap;
    yield return new WaitUntil(TryGetAdvanceInputThisFrame);

    playbackState = DiscoveryPlaybackState.PlayingOutro;
    obiAnimation.Stop();
    if (!obiAnimation.Play(outroClip.name))
    {
        Debug.LogWarning($"FairyDiscoveryAnimationPlayer: Animation clip '{outroClip.name}' の再生に失敗しました。", this);
        ResetPlaybackState(true);
        yield break;
    }

    yield return new WaitForSeconds(outroClip.length);

    ResetPlaybackState(true);
}
```

## 手順3: タップ進行用 helper と後始末を追加する
1. マウスとタッチの両方を受ける `TryGetAdvanceInputThisFrame()` を追加する。
2. `ResetPlaybackState(bool invokeCompletion)` のような private method を追加し、ロック解除、state 初期化、callback 実行を一箇所で行う。
3. `OnDisable()` と `OnDestroy()` では待機中でもロック解除だけは確実に行い、callback は実行しない。

### helper 追加コード例
```csharp
private static bool TryGetAdvanceInputThisFrame()
{
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            return true;
        }
    }

    return Input.GetMouseButtonDown(0);
}

private void ResetPlaybackState(bool invokeCompletion)
{
    Action completion = pendingCompletion;
    pendingCompletion = null;
    playingCoroutine = null;
    playbackState = DiscoveryPlaybackState.Idle;
    sealPhaseController?.SetPeelingLocked(false);

    if (invokeCompletion)
    {
        completion?.Invoke();
    }
}

private void ReleaseLockIfNeeded()
{
    if (playbackState == DiscoveryPlaybackState.Idle)
    {
        return;
    }

    pendingCompletion = null;
    playingCoroutine = null;
    playbackState = DiscoveryPlaybackState.Idle;
    sealPhaseController?.SetPeelingLocked(false);
}
```

## 手順4: `PeelSticker3D` 側の契約を維持する
1. [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/PeelSticker3D.cs) の `CompletePeel()` は大きく変えなくてよいが、成功時が「アウト完了後破棄」であることが読み取れる形に整理する。
2. 直接 `Destroy(gameObject)` をラムダで渡す代わりに private method に切り出してもよい。
3. フォールバックは現行どおり `Destroy(gameObject, 0.5f)` を維持する。

### 調整コード例
```csharp
private void CompletePeel()
{
    if (isPeelComplete)
    {
        return;
    }

    isPeelComplete = true;
    isAutoPeeling = false;

    if (!StickerRuntimeRegistry.TryConsumeFairy(this, out StickerFairyAssignment assignment) || assignment == null || !assignment.HasFairy)
    {
        Destroy(gameObject, 0.5f);
        return;
    }

    FairyCollectionService.TryRegisterDiscovery(assignment.Fairy, out bool isNewDiscovery);
    FairyDiscoveryLogger.LogDiscovered(assignment.Fairy, isNewDiscovery);

    FairyDiscoveryAnimationPlayer animationPlayer = GetDiscoveryAnimationPlayer();
    if (animationPlayer != null && animationPlayer.TryPlay(HandleDiscoveryAnimationCompleted))
    {
        return;
    }

    Destroy(gameObject, 0.5f);
}

private void HandleDiscoveryAnimationCompleted()
{
    Destroy(gameObject);
}
```

## 手順5: `Main.unity` の serialized field を更新する
1. Unity Editor で [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
2. `SealPhaseSystem` の `FairyDiscoveryAnimationPlayer` を選択する。
3. 旧 `clipName` の代わりに、イン用とアウト用の 2 フィールドが Inspector に表示される状態にする。
4. イン用には `discovery_in`、アウト用には `discovery_out` を仮設定する。
5. 実際のアニメーションクリップ名が異なる場合は、その名前に合わせて field を修正する。
6. `ObiRoot` の `Animation` に両クリップが登録されていることを確認する。

## 手順6: Unity 上で動作確認する
1. Play モードで妖精ありシールをめくる。
2. イン演出が自動で再生されることを確認する。
3. イン演出完了後に自動で閉じず、待機状態になることを確認する。
4. 待機中に 1 回タップすると、アウト演出が再生されることを確認する。
5. アウト演出完了後にのみ、対象シールが破棄されることを確認する。
6. 待機中タップで他シールがめくれないことを確認する。
7. 妖精なしシールでは従来どおり発見演出なしで消えることを確認する。
8. イン用またはアウト用クリップ名を一時的に外し、警告ログが出て 0.5 秒フォールバック破棄になることを確認する。

## 完了条件
- 妖精ありシールで、イン演出再生後にタップ待機へ入る。
- 待機中タップ 1 回でアウト演出へ進む。
- 他シールの剥がし入力は演出完了までロックされる。
- 妖精登録と発見ログは 1 回だけ実行される。
- 対象シールはアウト演出完了後に破棄される。
- クリップ未設定などの異常時でも入力ロックが残らず、シールが残留しない。

## 補足
- `WaitUntil(TryGetAdvanceInputThisFrame)` は毎フレーム評価されるため、helper は新規 `Down` / `Began` のみ返すようにする。
- 実際のクリップ名が `discovery_in` / `discovery_out` でない場合は、コード上の初期値より Inspector 設定を優先してよい。
- 今回の作業では、専用の「タップして進む」UI 表示は追加しない。
