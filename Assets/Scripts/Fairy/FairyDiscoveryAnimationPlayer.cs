using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FairyDiscoveryAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animation obiAnimation;
    [SerializeField] private SealPhaseController sealPhaseController;
    [SerializeField] private string clipName = "discovery";

    private bool isPlaying;

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
        if (isPlaying)
        {
            Debug.LogWarning("FairyDiscoveryAnimationPlayer: 発見演出の多重再生はできません。", this);
            return false;
        }

        if (obiAnimation == null)
        {
            Debug.LogWarning("FairyDiscoveryAnimationPlayer: ObiRoot の Animation が未設定です。", this);
            return false;
        }

        AnimationClip clip = obiAnimation.GetClip(clipName);
        if (clip == null)
        {
            Debug.LogWarning($"FairyDiscoveryAnimationPlayer: Animation clip '{clipName}' が見つかりません。", this);
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
        if (!obiAnimation.Play(clip.name))
        {
            Debug.LogWarning($"FairyDiscoveryAnimationPlayer: Animation clip '{clip.name}' の再生に失敗しました。", this);
            isPlaying = false;
            sealPhaseController?.SetPeelingLocked(false);
            yield break;
        }

        yield return new WaitForSeconds(clip.length);

        isPlaying = false;
        sealPhaseController?.SetPeelingLocked(false);
        onCompleted?.Invoke();
    }

    private void ReleaseLockIfNeeded()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;
        sealPhaseController?.SetPeelingLocked(false);
    }
}
