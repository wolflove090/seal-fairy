# 妖精発見演出再生 作業手順書

## 目的
- 妖精ありシールをめくり切った時に、`ObiRoot` のレガシー `Animation` で `discovery` を再生する。
- `discovery` の再生完了を待ってから対象シールを破棄する。
- 演出中は他シールをめくれないようにし、演出完了後に剥がし操作を再開する。

## 変更対象
- [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs)
- [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs)
- [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: 発見演出プレイヤーを追加する
1. [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs) を新規作成する。
2. `MonoBehaviour` として実装し、以下の SerializeField を持たせる。
- `Animation obiAnimation`
- `SealPhaseController sealPhaseController`
- `string clipName = "discovery"`
3. 実行時の多重再生防止用に `bool isPlaying` を private field で持たせる。
4. 外部公開 API は `TryPlay(Action onCompleted)` のようにし、成功時だけ `true` を返す形にする。
5. `obiAnimation == null`、`obiAnimation.GetClip(clipName) == null`、`isPlaying == true` のいずれかなら `false` を返し、必要なら `Debug.LogWarning` を出す。

### 実装例
```csharp
using System;
using System.Collections;
using UnityEngine;

public sealed class FairyDiscoveryAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animation obiAnimation;
    [SerializeField] private SealPhaseController sealPhaseController;
    [SerializeField] private string clipName = "discovery";

    private bool isPlaying;

    public bool TryPlay(Action onCompleted)
    {
        if (isPlaying)
        {
            return false;
        }

        if (obiAnimation == null)
        {
            Debug.LogWarning("ObiRoot の Animation が未設定です。");
            return false;
        }

        AnimationClip clip = obiAnimation.GetClip(clipName);
        if (clip == null)
        {
            Debug.LogWarning($"Animation clip '{clipName}' が見つかりません。");
            return false;
        }

        StartCoroutine(PlayRoutine(clip, onCompleted));
        return true;
    }

    private IEnumerator PlayRoutine(AnimationClip clip, Action onCompleted)
    {
        isPlaying = true;
        sealPhaseController?.SetPeelingLocked(true);

        obiAnimation.Stop();
        obiAnimation.Play(clip.name);

        yield return new WaitForSeconds(clip.length);

        isPlaying = false;
        sealPhaseController?.SetPeelingLocked(false);
        onCompleted?.Invoke();
    }
}
```

## 手順2: SealPhaseController に剥がしロックを追加する
1. [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) を開く。
2. `private bool isPeelingLocked;` を追加する。
3. `ApplyPhase` の `SetTapPeelEnabled` 条件を `phase == SealGamePhase.StickerPeeling && !isPeelingLocked` に変更する。
4. `SetPeelingLocked(bool locked)` を追加し、全アクティブシールへ再適用できるようにする。
5. 配置フェーズに戻る時や `ClearRemainingStickers()` 実行時に、ロック状態が残留しないよう必要なら `isPeelingLocked = false;` を入れる。

### 実装例
```csharp
private bool isPeelingLocked;

public void SetPeelingLocked(bool locked)
{
    isPeelingLocked = locked;

    foreach (PeelSticker3D sticker in StickerRuntimeRegistry.GetActiveStickers())
    {
        sticker.SetTapPeelEnabled(CurrentPhase == SealGamePhase.StickerPeeling && !isPeelingLocked);
    }
}

private void ApplyPhase(SealGamePhase phase)
{
    CurrentPhase = phase;
    tapStickerPlacer.SetPlacementEnabled(phase == SealGamePhase.StickerPlacement);

    foreach (PeelSticker3D sticker in StickerRuntimeRegistry.GetActiveStickers())
    {
        sticker.SetTapPeelEnabled(phase == SealGamePhase.StickerPeeling && !isPeelingLocked);
    }

    eventHub?.NotifyPhaseChanged(CurrentPhase);
}
```

## 手順3: PeelSticker3D に演出待ち破棄を組み込む
1. [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) を開く。
2. `FairyDiscoveryAnimationPlayer` への参照を保持する方法を決める。
3. この作業では、SerializeField 追加か `FindAnyObjectByType<FairyDiscoveryAnimationPlayer>()` のどちらか一方に統一する。
4. `CompletePeel()` の `TryConsumeFairy` 成功後、妖精ありなら `FairyCollectionService.TryRegisterDiscovery` と `FairyDiscoveryLogger.LogDiscovered` を従来どおり先に呼ぶ。
5. その後 `FairyDiscoveryAnimationPlayer.TryPlay(() => Destroy(gameObject))` を呼ぶ。
6. `TryPlay` が `false` の場合は、シール残留防止のため `Destroy(gameObject, 0.5f)` へフォールバックする。
7. 妖精なしの場合は `Destroy(gameObject, 0.5f)` を維持してよい。

### 実装例
```csharp
[SerializeField] private FairyDiscoveryAnimationPlayer fairyDiscoveryAnimationPlayer;

private void CompletePeel()
{
    if (isPeelComplete)
    {
        return;
    }

    isPeelComplete = true;
    isAutoPeeling = false;

    if (!StickerRuntimeRegistry.TryConsumeFairy(this, out StickerFairyAssignment assignment) ||
        assignment == null ||
        !assignment.HasFairy)
    {
        Destroy(gameObject, 0.5f);
        return;
    }

    if (FairyCollectionService.TryRegisterDiscovery(assignment.Fairy, out bool isNewDiscovery))
    {
        FairyDiscoveryLogger.LogDiscovered(assignment.Fairy, isNewDiscovery);
    }

    if (fairyDiscoveryAnimationPlayer == null ||
        !fairyDiscoveryAnimationPlayer.TryPlay(() => Destroy(gameObject)))
    {
        Destroy(gameObject, 0.5f);
    }
}
```

## 手順4: Main.unity の参照と Animation 設定を更新する
1. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を Unity Editor で開く。
2. `ObiRoot` を選択し、`Animation` コンポーネントの `Play Automatically` をオフにする。
3. `SealPhaseSystem` など既存フェーズ制御オブジェクトへ `FairyDiscoveryAnimationPlayer` を追加する。
4. `FairyDiscoveryAnimationPlayer` の `obiAnimation` に `ObiRoot` の `Animation` を割り当てる。
5. `sealPhaseController` に既存の `SealPhaseController` を割り当てる。
6. [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) を SerializeField 参照方式にした場合は、対象 prefab またはシーン上オブジェクト側の設定方法を統一する。

## 手順5: 動作確認
1. Play モードで剥がしフェーズへ入る。
2. 妖精ありシールをめくり、`discovery` が再生されることを確認する。
3. `discovery` 再生中は、別シールをタップしてもめくれないことを確認する。
4. `discovery` 完了後に対象シールが破棄されることを確認する。
5. 演出完了後、他シールの剥がし操作が再開できることを確認する。
6. 妖精なしシールでは `discovery` が再生されず、通常どおり破棄されることを確認する。
7. Console で発見ログが 1 回のみ出ることを確認する。
8. `obiAnimation` 未設定や `discovery` 未登録の状態を一時的に作り、シール残留せずフォールバック破棄されることを確認する。

## 作業時の注意
- `WaitForSeconds` は `AnimationClip.length` を使い、固定秒数を書かない。
- `SetPeelingLocked(false)` を演出完了時にしか呼ばないと、例外時にロック残留する可能性がある。必要なら `try/finally` 相当の後始末を入れる。
- `SealPhaseController` 側で入力再適用するときは、必ず現在フェーズを見て配置フェーズで誤有効化しない。
- `PeelSticker3D` の `isPeelComplete` を維持し、演出待ち中に `CompletePeel()` が再実行されないようにする。
