# 妖精発見演出再生 実装計画

## 実装方針
- 既存の妖精発見判定は [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) 内の `CompletePeel()` で完結しているため、妖精判定の責務は維持しつつ、「発見演出再生」と「演出中入力ロック」は別コンポーネントへ分離する。
- `ObiRoot` は [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) 上で `Animation` を持っているため、実装では名前検索を使わず、`Animation` 参照を Inspector から注入する。
- レガシー `Animation` の `discovery` 再生は、専用の `FairyDiscoveryAnimationPlayer` が担当する。`PeelSticker3D` からは「妖精発見時に演出を再生して完了通知を受ける」だけにし、`AnimationState.length` ベースで待機する。
- 演出中の剥がし禁止は [SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) を制御の単一点にする。発見演出開始時に全アクティブシールの `SetTapPeelEnabled(false)` を再適用し、完了後に現在フェーズが `StickerPeeling` の場合のみ復帰する。
- `ObiRoot.Animation` のオート再生は無効化し、`discovery` はスクリプト起点でのみ再生される状態にする。
- 再生失敗時はシール残留を避けるため、警告ログを出しつつ演出待ちをスキップして通常破棄へフォールバックする。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs)
  - `CompletePeel()` を即時破棄から「通常破棄」または「発見演出完了後破棄」へ分岐させる。
  - 重複完了防止は維持しつつ、妖精あり時に `FairyDiscoveryAnimationPlayer` へ再生要求する。
  - 演出中に自身の追加入力が再開しないよう状態を明示する。
- [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs)
  - 発見演出のロック開始 / 解除 API を追加する。
  - 現在フェーズと演出ロック状態の両方を見て `SetTapPeelEnabled` を制御する。
  - 配置フェーズ復帰時や全削除時にロック状態を安全に解除する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `ObiRoot` の `Animation` で `Play Automatically` を無効化する。
  - 新規 `FairyDiscoveryAnimationPlayer` を適切な GameObject に追加し、`ObiRoot` の `Animation` と `SealPhaseController` を割り当てる。

### 新規作成予定
- [Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDiscoveryAnimationPlayer.cs)
  - `Animation` と `SealPhaseController` の参照を保持する。
  - `PlayDiscoveryAndNotify(Action onCompleted)` または同等 API で `discovery` を再生し、クリップ長ぶん待機して完了コールバックを返す。
  - 再生中フラグを持ち、重複再生要求を拒否または無視する。

## データフロー / 処理フロー
1. プレイヤーが剥がしフェーズ中のシールをタップする。
2. [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) が自動めくりを完了し、`StickerRuntimeRegistry.TryConsumeFairy` で妖精割り当てを確定取得する。
3. 妖精なしなら従来どおり完了処理を終え、短時間後にシールを破棄する。
4. 妖精ありなら `FairyCollectionService.TryRegisterDiscovery` と `FairyDiscoveryLogger.LogDiscovered` を先に 1 回だけ実行する。
5. その後 `FairyDiscoveryAnimationPlayer` に `discovery` 再生を依頼する。
6. `FairyDiscoveryAnimationPlayer` は `SealPhaseController` に演出ロック開始を通知し、全シールの剥がし入力を停止する。
7. `FairyDiscoveryAnimationPlayer` は `ObiRoot.Animation.Play("discovery")` を実行し、該当クリップ長を基準に待機する。
8. 待機完了後、`PeelSticker3D` 側へ完了通知を返し、対象シールを破棄する。
9. `FairyDiscoveryAnimationPlayer` は `SealPhaseController` に演出ロック解除を通知し、現在フェーズが `StickerPeeling` の場合のみ剥がし入力を再開する。
10. `Animation` 未設定や `discovery` 未登録なら、警告を出してロックなしで破棄へフォールバックする。

## 処理詳細

### 発見演出再生コンポーネント
- `FairyDiscoveryAnimationPlayer` は `MonoBehaviour` とし、以下の SerializeField を持つ。
- `Animation obiAnimation`
- `SealPhaseController sealPhaseController`
- `string clipName = "discovery"`
- `bool isPlaying`
- API は `bool TryPlay(System.Action onCompleted)` のような単純な形にする。
- `TryPlay` 内で前提チェックを行い、失敗時は `false` を返す。
- 成功時は coroutine を開始し、完了後に `onCompleted` を実行する。

### 剥がし完了処理の変更
- `PeelSticker3D.CompletePeel()` は以下の順序に整理する。
- `isPeelComplete = true`
- `StickerRuntimeRegistry.TryConsumeFairy(...)`
- 妖精登録とログ出力
- 妖精ありなら `FairyDiscoveryAnimationPlayer.TryPlay(...)`
- 再生開始できた場合はコールバック内で `Destroy(gameObject)` を実行
- 再生できなかった場合は即フォールバック破棄
- 妖精なしは既存どおり短時間後の破棄でよい

### 入力ロック制御
- `SealPhaseController` に `SetPeelingLocked(bool locked)` または同等 API を追加する。
- `ApplyPhase` は `CurrentPhase == StickerPeeling && !isPeelingLocked` のときだけアクティブシールへ `SetTapPeelEnabled(true)` を適用する。
- 演出中に新規生成されるシールはない前提だが、復帰時は `StickerRuntimeRegistry.GetActiveStickers()` を再列挙して一括再適用する。
- 配置フェーズでは常に剥がし入力は無効のため、ロック解除時に誤って有効化しないよう現在フェーズ判定を通す。

### シーン設定
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) の `ObiRoot.Animation` で `m_PlayAutomatically: 0` に変更する。
- `FairyDiscoveryAnimationPlayer` は `SealPhaseSystem` または既存のフェーズ関連 GameObject へ追加し、Inspector で `ObiRoot.Animation` と `SealPhaseController` を接続する。
- `PeelSticker3D` から参照するため、`FairyDiscoveryAnimationPlayer` はシーン上の単一インスタンスとして扱う。参照方法は `SealPhaseController` 経由または Inspector 注入先を増やす形で統一する。

## リスクと対策
- `PeelSticker3D` からシーン上の演出プレイヤーへ直接依存すると参照設定漏れで再生失敗しやすい。
  - `FairyDiscoveryAnimationPlayer` は Inspector 必須参照を持たせ、`Awake` で不足時に明示ログを出す。
- 演出中に `SealPhaseController` が別経路でフェーズ切替されると、入力復帰条件が崩れる可能性がある。
  - ロック解除時に常に `CurrentPhase` を参照して `StickerPeeling` 時のみ再有効化する。
- `Animation.Play("discovery")` が失敗した場合にシールが残留する可能性がある。
  - `TryPlay` が失敗したら待機せず破棄へ進む。
- `AnimationState.length` の取得に失敗すると待機時間が不正になる可能性がある。
  - クリップ取得失敗は再生失敗扱いにしてフォールバックする。

## 検証方針
- 手動確認1:
  - 妖精ありシールをめくると `ObiRoot` の `discovery` が 1 回再生されること。
- 手動確認2:
  - `discovery` 再生完了前は対象シールが残り、完了後に破棄されること。
- 手動確認3:
  - 妖精なしシールをめくっても `discovery` は再生されず、通常どおり破棄されること。
- 手動確認4:
  - `discovery` 再生中は他シールをタップしてもめくり開始しないこと。
- 手動確認5:
  - 演出完了後、剥がしフェーズ中なら他シールの剥がし操作が再開できること。
- 手動確認6:
  - `ObiRoot` 参照未設定または `discovery` 未登録時でも、シールが残留せず破棄されること。
- 手動確認7:
  - 妖精登録と発見ログが 1 回のみ実行され、重複しないこと。

## コードスニペット
```csharp
public sealed class FairyDiscoveryAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animation obiAnimation;
    [SerializeField] private SealPhaseController sealPhaseController;
    [SerializeField] private string clipName = "discovery";

    private bool isPlaying;

    public bool TryPlay(System.Action onCompleted)
    {
        if (isPlaying || obiAnimation == null || obiAnimation.GetClip(clipName) == null)
        {
            return false;
        }

        StartCoroutine(PlayRoutine(onCompleted));
        return true;
    }
}
```

```csharp
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

    FairyCollectionService.TryRegisterDiscovery(assignment.Fairy, out bool isNewDiscovery);
    FairyDiscoveryLogger.LogDiscovered(assignment.Fairy, isNewDiscovery);

    if (!fairyDiscoveryAnimationPlayer.TryPlay(() => Destroy(gameObject)))
    {
        Destroy(gameObject, 0.5f);
    }
}
```

```csharp
public void SetPeelingLocked(bool locked)
{
    isPeelingLocked = locked;

    foreach (PeelSticker3D sticker in StickerRuntimeRegistry.GetActiveStickers())
    {
        sticker.SetTapPeelEnabled(CurrentPhase == SealGamePhase.StickerPeeling && !isPeelingLocked);
    }
}
```
