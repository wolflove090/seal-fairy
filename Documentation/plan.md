# シール妖精発見エフェクト 実装計画

## 実装方針
- 既存の [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) は「シールを生成し、配置時の初期状態を仕込む」責務を維持し、妖精ありシールに対してのみキラキラエフェクトを子オブジェクトとして追加する。
- 既存の [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) は剥がし進行の制御元として扱い、めくり完了後の即時 `Destroy(gameObject)` をやめて 2 秒遅延破棄へ変更する。
- 妖精有無の保持は既存の [StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs) を継続利用し、配置時に決めた結果と、剥がし完了時のログ出力条件を一致させる。
- キラキラエフェクトはシール裏側に「事前配置されている」扱いにするため、剥がし完了時に新規生成はしない。剥がすことで裏面が露出し、結果としてエフェクトが見える構造にする。
- エフェクト prefab は `Resources.Load<GameObject>()` で取得する前提にし、`TapStickerPlacer` のランタイム自動生成構成を維持したまま仕込み処理を追加する。
- 初期デモ用としてシーンに直接存在するシールは今回の仕込み対象外とし、`TapStickerPlacer` 経由で生成したシールだけにエフェクトを付与する。

## 変更対象ファイル一覧

### 更新予定
- `Assets/Scripts/TapStickerPlacer.cs`
  - 配置時の妖精判定と registry 登録後、妖精ありシールにだけ `KiraKiraEffect.prefab` を子として生成する。
  - エフェクト生成の責務を private helper に切り出し、配置処理の見通しを保つ。
- `Assets/Scripts/PeelSticker3D.cs`
  - めくり完了時の状態を追加し、`Destroy(gameObject)` の即時実行を 2 秒遅延へ置き換える。
  - 重複して破棄待機やログ出力が走らないように、完了後のガード状態を持つ。
- `Assets/Scripts/StickerRuntimeRegistry.cs`
  - 基本 API は維持しつつ、今回必要であれば未使用データ削除の補助 API 追加可否を検討する。
  - ただし最小差分を優先し、`Register` と `TryConsumeFairy` のままで成立するなら変更しない。

### 参照確認対象
- `Assets/Effect/Kirakira/KiraKiraEffect.prefab`
  - `Resources.Load` で取得できる配置へ変更し、生成時の親子付け、Transform 初期化、ParticleSystem の再生前提を確認する。
- `Assets/Scripts/PeelStickerDemoBootstrap.cs`
  - 初期デモシールが今回の対象外で問題ないことを確認する。

## データフロー
1. プレイヤー入力により [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) がシールを生成する。
2. 生成直後に `Random.value < 0.5f` で妖精有無を 1 回だけ決定する。
3. 妖精有無を [StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs) に登録する。
4. 妖精ありの場合のみ `KiraKiraEffect.prefab` を生成し、対象シールの子オブジェクトとしてアタッチする。
5. 生成したエフェクトの localPosition は `Vector3.zero`、localRotation は `Quaternion.identity`、localScale は `Vector3.one` とする。
6. プレイヤーがシールをタップすると [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) が既存どおり自動めくりを進行する。
7. めくり量が完了条件に達した時、registry から妖精有無を取り出し、妖精ありなら既存ログを 1 回だけ出力する。
8. 同タイミングで「破棄待機中」状態へ遷移し、2 秒経過後にシールオブジェクトを破棄する。
9. エフェクトはシールの子なので、2 秒経過後の親破棄と同時に消える。

## 処理フロー

### 配置時
1. `SpawnSticker` が実体シールを生成する。
2. `bool hasFairy = Random.value < 0.5f;` を評価する。
3. `StickerRuntimeRegistry.Register(sticker, hasFairy)` を実行する。
4. `hasFairy == true` の場合だけ `KiraKiraEffect.prefab` を Instantiate し、`sticker.transform` の子にする。
5. エフェクトのローカル Transform はゼロ初期化し、追加の回転補正は行わない。
6. `hasFairy == false` の場合は何も生成しない。

### めくり時
1. 既存どおり `StartAutoPeel()` で自動めくりを開始する。
2. `Update()` で `peelAmount` を `1f` へ進める。
3. 完了到達時、まだ完了処理未実行であることをガードで確認する。
4. `StickerRuntimeRegistry.TryConsumeFairy(this, out bool hasFairy)` を呼ぶ。
5. `hasFairy == true` の場合だけ `Debug.Log("妖精を発見！")` を出す。
6. その場では `Destroy(gameObject)` せず、遅延破棄カウントを開始する。
7. 2 秒経過後に親シールを破棄し、子エフェクトも同時に破棄する。

## 実装詳細

### /Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs
- `SpawnSticker` 内で妖精判定と registry 登録を行っているため、その直後にエフェクト仕込みを差し込む。
- エフェクト生成処理は `AttachFairyEffect(PeelSticker3D sticker)` のような private method に分離する。
- prefab は `Resources.Load<GameObject>()` で取得する。
- `TapStickerPlacer` 側で毎回ロードしないよう、static または private cache で 1 回だけ読み込む構成にする。
- `Resources.Load` 失敗時は `null` チェックでエフェクト生成だけをスキップし、シール配置自体は継続する。
- エフェクトは `sticker.transform` 配下へ生成し、`localPosition = Vector3.zero`、`localRotation = Quaternion.identity`、`localScale = Vector3.one` を設定する。
- 同一シールで二重生成しないよう、配置時のみ helper を呼ぶ。

### /Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs
- `isAutoPeeling` に加えて、例えば `isPeelComplete` のような完了ガード状態を追加する。
- 完了フレームで registry 参照、ログ出力、`Destroy(gameObject, 2f)` による遅延破棄予約を 1 回だけ行う。
- 遅延破棄中は `peelAmount` を固定し、再度ログや破棄開始が走らないようにする。
- 既存の `OnDestroy` はそのまま活かし、親破棄時に子エフェクトもまとめて消える前提とする。

### /Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs
- 現状の `Register` と `TryConsumeFairy` は今回の要件に適合している。
- 追加 API は原則不要とし、未登録シールや初期デモシールでも例外なく通ることを維持する。
- registry 参照はめくり完了 1 回のみとし、重複ログ防止を registry の削除動作で担保する。

## リスクと対策
- `KiraKiraEffect.prefab` の親子付けだけでは未剥がし時に表側へ見えてしまう可能性がある。
  - 既存の front/back メッシュの向きと描画順を確認し、必要なら localPosition の z 微調整を設計レビューで追加判断する。
- `TapStickerPlacer` がシーン参照を持たないため prefab 参照解決が不安定になる可能性がある。
  - `Resources.Load` に統一し、失敗時は `null` チェックで配置処理自体を壊さない。
- `Resources.Load` を使うため prefab 配置先が不正だとロード失敗する可能性がある。
  - `KiraKiraEffect.prefab` を `Assets/Resources/` 配下へ移し、ロードパスを固定文字列で管理する。
- めくり完了後に `Update()` が継続するため、ログ出力や遅延開始が重複する可能性がある。
  - 完了処理専用フラグを設けて 1 回しか通さない。
- 2 秒待機中にシールが残ることで、再タップや重複入力が入る可能性がある。
  - 完了後は入力判定から除外し、追加の `StartAutoPeel()` を受け付けない。
- 初期デモシールは registry 未登録かつエフェクト未仕込みのため、挙動差が混在する可能性がある。
  - 今回の対象は配置シールのみと明記し、手動確認も配置シールで行う。

## 検証方針
- 手動確認1:
  - シールを複数枚配置し、妖精ありシールにのみエフェクト子オブジェクトが付与されることを Hierarchy で確認する。
- 手動確認2:
  - 未剥がし状態で、妖精ありシールでもキラキラが表から見えないことを確認する。
- 手動確認3:
  - 妖精ありシールをめくり、裏面露出後にキラキラが見えることを確認する。
- 手動確認4:
  - 妖精なしシールをめくってもキラキラが見えないことを確認する。
- 手動確認5:
  - めくり完了後、シールが即時には消えず約 2 秒後に破棄されることを確認する。
- 手動確認6:
  - 妖精ありシールでログが 1 回だけ出ること、妖精なしシールで出ないことを確認する。
- 手動確認7:
  - 初期デモシールはエフェクト対象外であり、配置シールの挙動に影響しないことを確認する。

## コードスニペット
```csharp
private void SpawnSticker(Vector3 worldPoint)
{
    PeelSticker3D sticker = Instantiate(templateSticker);
    sticker.name = "Peel Sticker";
    sticker.transform.SetPositionAndRotation(worldPoint, templateSticker.transform.rotation);
    sticker.transform.localScale = templateSticker.transform.localScale;
    sticker.gameObject.SetActive(true);
    sticker.PeelAmount = 0f;

    bool hasFairy = Random.value < 0.5f;
    StickerRuntimeRegistry.Register(sticker, hasFairy);

    if (hasFairy)
    {
        AttachFairyEffect(sticker);
    }
}
```

```csharp
private static GameObject LoadFairyEffectPrefab()
{
    // Effect/Kirakira/Resources/KiraKiraEffect.prefabに配置
    return Resources.Load<GameObject>("KiraKiraEffect");
}
```

```csharp
private void Update()
{
    if (!Application.isPlaying)
    {
        return;
    }

    if (allowTapPeel && !isPeelComplete)
    {
        HandlePointer();
    }

    if (isAutoPeeling && !isPeelComplete)
    {
        float nextAmount = Mathf.MoveTowards(peelAmount, 1f, Time.deltaTime / autoPeelDuration);
        PeelAmount = nextAmount;
        if (nextAmount >= 1f)
        {
            CompletePeel();
        }
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

    if (StickerRuntimeRegistry.TryConsumeFairy(this, out bool hasFairy) && hasFairy)
    {
        Debug.Log("妖精を発見！");
    }

    Destroy(gameObject, 2f);
}
```
