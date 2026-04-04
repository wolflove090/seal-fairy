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

    private void Awake()
    {
        if (obiAnimation == null)
        {
            Debug.LogWarning("FairyDiscoveryAnimationPlayer: ObiRoot の Animation が未設定です。", this);
        }
    }

    private void OnDisable()
    {
        ReleaseLockIfNeeded();
    }

    private void OnDestroy()
    {
        ReleaseLockIfNeeded();
    }

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

        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }

        pendingCompletion = null;
        playingCoroutine = null;
        playbackState = DiscoveryPlaybackState.Idle;
        sealPhaseController?.SetPeelingLocked(false);
    }
}
