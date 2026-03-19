# シール妖精配置と剥がしログ更新 実装計画

## 実装方針
- 既存の [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) は「配置入力を受けてシールを生成する」責務を維持しつつ、生成したシールに対応する管理データを別コンポーネントへ登録する構成にする。
- 既存の [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) は見た目と自動めくり演出の責務に寄せ、妖精有無の保持は持たせない。
- 妖精有無は `MonoBehaviour` 外のランタイム管理データで保持し、めくり完了時にその管理データを参照してログ出力する。
- 今回の乱数取得は `UnityEngine.Random.value` などの直接参照で実装し、抽象化は追加しない。
- 実装は最小差分を優先し、既存の自動生成デモ構成を壊さずに追加する。

## 変更対象ファイル一覧

### 新規作成予定
- `Assets/Scripts/StickerRuntimeRegistry.cs`
  - 生成したシールと妖精有無を対応付けて保持する。
  - 配置時の登録、めくり完了時の照会、削除時の後始末を担当する。

### 更新予定
- `Assets/Scripts/TapStickerPlacer.cs`
  - シール生成直後に 50% 判定を行い、生成シールを `StickerRuntimeRegistry` に登録する。
- `Assets/Scripts/PeelSticker3D.cs`
  - めくり完了直前、または完了確定時に `StickerRuntimeRegistry` へ通知し、妖精がいた場合のみログを出す。
- `Assets/Scripts/PeelStickerDemoBootstrap.cs`
  - 変更不要の可能性が高いが、ランタイム管理コンポーネントの生成方式によっては参照確認対象に含める。

## データ構造
- `StickerRuntimeRegistry`
  - キー: `PeelSticker3D` インスタンス、または `instanceId`
  - 値: 妖精有無を表す `bool`
  - 役割:
    - 配置時に `Register(sticker, hasFairy)` する。
    - めくり完了時に `TryConsumeFairy(sticker, out bool hasFairy)` で参照する。
    - 重複ログ防止のため、参照後は該当データを削除する。
- 補足:
  - `MonoBehaviour` 自体に `hasFairy` を持たせず、外部辞書で対応付ける。
  - シール破棄時に参照が残らないよう、完了時または `OnDestroy` 系の保険処理を検討する。

## 処理フロー
1. プレイヤーが入力すると、[TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) が既存どおり配置座標を決定する。
2. `SpawnSticker` で `PeelSticker3D` を生成した直後に `UnityEngine.Random.value < 0.5f` で妖精有無を決定する。
3. 生成した `PeelSticker3D` と妖精有無を `StickerRuntimeRegistry` へ登録する。
4. プレイヤーがシールをクリックすると、[PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) が既存どおり自動めくりを開始する。
5. めくり量が完了条件に達した時点で、`StickerRuntimeRegistry` から対象シールの妖精有無を取得する。
6. 妖精ありなら `Debug.Log` で発見メッセージを 1 回出力する。
7. 取得後は管理データを削除し、その後に `GameObject` を破棄する。

## ファイル別の実装方針

### /Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs
- `SpawnSticker` の戻り値を `void` から `PeelSticker3D` 返却に変えるか、生成直後に登録処理を差し込める形へ変更する。
- `bool hasFairy = Random.value < 0.5f;` を配置時に 1 回だけ評価する。
- `StickerRuntimeRegistry` のインスタンス取得方法を決める。
  - 最小差分なら static 管理でもよい。
  - シーン上の `MonoBehaviour` として置く場合は自動生成処理が必要になる。

### /Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs
- `Update` 内の自動めくり完了分岐で、`Destroy(gameObject)` の前に registry 参照を追加する。
- ログ出力タイミングは「めくり完了時」に固定する。
- 自動めくり完了時にのみ参照することで、めくり開始時ログを防ぐ。
- 参照結果が未登録でも既存の破棄処理は継続できるようにする。

### /Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/StickerRuntimeRegistry.cs
- API は最小限に絞る。
  - `Register(PeelSticker3D sticker, bool hasFairy)`
  - `TryConsumeFairy(PeelSticker3D sticker, out bool hasFairy)`
  - 必要であれば `Unregister(PeelSticker3D sticker)`
- `TryConsumeFairy` は参照と削除を同時に行い、重複ログ防止を担保する。
- 破棄順や未登録ケースで例外を出さない実装にする。

## リスクと対策
- `PeelSticker3D` のテンプレート個体まで registry に登録してしまう可能性がある。
  - 配置時に `Instantiate` した実体のみ登録し、非アクティブなテンプレートは登録しない。
- めくり完了前に別経路でシールが破棄されると registry にゴミが残る可能性がある。
  - `TryConsumeFairy` で正常系を処理しつつ、必要なら `OnDestroy` 時の保険削除を検討する。
- 同一シールで重複ログが出る可能性がある。
  - `TryConsumeFairy` を参照兼削除 API にして 1 回しか成功しないようにする。
- `PeelSticker3D` がデモ用初期シールとして存在する場合、そのシールは registry 未登録のためログ対象にならない可能性がある。
  - 今回の仕様対象を「配置したシール」に限定し、必要なら初期シールは対象外でよいことを明記する。

## 検証方針
- 手動確認1:
  - シールを 5 枚以上配置し、全てが通常どおり置けることを確認する。
- 手動確認2:
  - 複数のシールを順にめくり、妖精がいたシールでのみ Console にログが出ることを確認する。
- 手動確認3:
  - 妖精がいなかったシールでは追加ログが出ないことを確認する。
- 手動確認4:
  - 同じシールでログが 2 回以上出ないことを確認する。
- 手動確認5:
  - 既存の配置不能条件、既存の自動めくり破棄が壊れていないことを確認する。

## コードスニペット
```csharp
public static class StickerRuntimeRegistry
{
    private static readonly Dictionary<int, bool> fairyByStickerId = new();

    public static void Register(PeelSticker3D sticker, bool hasFairy)
    {
        fairyByStickerId[sticker.GetInstanceID()] = hasFairy;
    }

    public static bool TryConsumeFairy(PeelSticker3D sticker, out bool hasFairy)
    {
        int key = sticker.GetInstanceID();
        if (!fairyByStickerId.TryGetValue(key, out hasFairy))
        {
            return false;
        }

        fairyByStickerId.Remove(key);
        return true;
    }
}
```

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
}
```
