# シール配置からシールめくりへのフェーズ遷移 実装計画

## 実装方針
- 既存の [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) と [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) は大きく作り直さず、入力受付可否だけを共通フェーズ状態から切り替える。
- フェーズ状態、UI ボタン連携、配置済みシールの一括削除は新規の進行管理コンポーネントに集約し、個別コンポーネントへ状態を分散させない。
- UI は既存の [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml) の右上ボタン `ready-button` を流用し、クリックはイベント中継クラス経由で通知する。
- `StickerPeeling` から `StickerPlacement` に戻る際の未めくりシール全削除を成立させるため、配置済みシールの追跡 API を `StickerRuntimeRegistry` か専用管理クラスに追加する。
- `SealPhaseEventHub` は `MonoBehaviour` にせず plain C# class とし、Unity 依存は composition root である `MonoBehaviour` 側へ閉じ込める。
- 今回は最小実装を優先し、残数表示、プレビュー、専用ドメイン層分割までは行わない。

## 変更対象ファイル一覧

### 新規作成予定
- `Assets/Scripts/SealGamePhase.cs`
  - `StickerPlacement` と `StickerPeeling` を表す enum。
- `Assets/Scripts/SealPhaseEventHub.cs`
  - UI からのフェーズ切替要求イベントと、フェーズ変更通知イベントを仲介する plain C# class。
- `Assets/Scripts/SealPhaseController.cs`
  - 現在フェーズ管理、イベント購読、各コンポーネントへの有効 / 無効反映、配置済みシール全削除を担当する。
- `Assets/Scripts/HudScreenBinder.cs`
  - UI Toolkit の `UIDocument` から `ready-button` を取得し、クリック時に `SealPhaseEventHub` をトリガーする。
  - `SealPhaseEventHub` が発行するフェーズ変更通知を受けて文言更新する。
- `Assets/Scripts/SealPhaseBootstrap.cs`
  - `SealPhaseEventHub` を生成し、`HudScreenBinder` と `SealPhaseController` へ同一インスタンスを注入する composition root。

### 更新予定
- `Assets/Scripts/TapStickerPlacer.cs`
  - 配置入力をフェーズ依存で有効 / 無効化できる public API を追加する。
  - 生成したシールをフェーズ管理側が追跡できるよう登録経路を整理する。
- `Assets/Scripts/PeelSticker3D.cs`
  - めくり入力をフェーズ依存で有効 / 無効化できる public API を追加する。
  - めくり中 / めくり完了後の重複入力ガードを維持しつつ、外部から強制削除できるようにする。
- `Assets/Scripts/StickerRuntimeRegistry.cs`
  - 妖精有無に加えて配置済みシール参照を追跡し、全削除や個別解除に使える API を追加する。
- `Assets/UI/UXML/HudScreen.uxml`
  - 必要最小限でフェーズ表示に使う既存ボタン名の確認のみ。構造変更が必要な場合だけ最小差分で修正する。
- `Assets/Main.unity`
  - `UIDocument`、bootstrap、フェーズ制御コンポーネントの配置、参照設定を追加する。

## データフロー / 処理フロー
1. 起動時に `SealPhaseController` が初期フェーズを `StickerPlacement` に設定する。
2. `SealPhaseBootstrap` が `SealPhaseEventHub` を生成し、`HudScreenBinder` と `SealPhaseController` に同じインスタンスを渡す。
3. `SealPhaseController` は `TapStickerPlacer` に配置入力有効、`PeelSticker3D` 群にめくり入力無効を反映する。
4. プレイヤーが画面をクリック / タップすると、`TapStickerPlacer` は有効時のみ既存処理でシールを生成する。
5. 生成したシールは `StickerRuntimeRegistry` に妖精情報と実体参照の両方を登録する。
6. `HudScreenBinder` が `ready-button` クリックを受けると、`SealPhaseEventHub` にフェーズ切替要求イベントを発火する。
7. `SealPhaseController` は `SealPhaseEventHub` のフェーズ切替要求イベントを購読し、`StickerPlacement -> StickerPeeling` 遷移時に配置入力を無効化し、登録済みシールのめくり入力を有効化する。
8. `StickerPeeling` 中は `PeelSticker3D` が既存の自動めくり処理を行い、配置入力は無視される。
9. `StickerPeeling -> StickerPlacement` 遷移時、`SealPhaseController` は registry から未破棄シールを列挙して全削除する。
10. 全削除後に `SealPhaseController` は配置入力を再有効化し、めくり入力を無効化する。
11. `SealPhaseController` はフェーズ確定後に `SealPhaseEventHub` へ変更通知を発火し、`HudScreenBinder` がそれを受けて右上ボタン文言を更新する。

## 処理詳細

### フェーズ状態
- `SealGamePhase` は `StickerPlacement` / `StickerPeeling` の 2 値だけを持つ。
- 現在フェーズは `SealPhaseController.CurrentPhase` で一元管理する。
- フェーズ変更 API は `TogglePhase()` か `SetPhase(SealGamePhase nextPhase)` のどちらかに統一し、UI から直接個別コンポーネントを触らせない。
- UI からの入力は `SealPhaseEventHub.RequestPhaseToggle()` のようなイベント API に限定し、`SealPhaseController` を直接参照させない。
- `SealPhaseEventHub` の生成と共有は `SealPhaseBootstrap` が担当し、`HudScreenBinder` と `SealPhaseController` は new しない。

### 配置入力の制御
- `TapStickerPlacer` に `SetPlacementEnabled(bool enabled)` を追加する。
- `Update()` 冒頭でこのフラグを確認し、無効時は入力判定もシール生成も行わない。
- 配置済みシール生成後、`SealPhaseController` または `StickerRuntimeRegistry` 経由で新規シールへ現在フェーズに応じためくり可否を反映する。

### めくり入力の制御
- `PeelSticker3D` の既存 `allowTapPeel` を Inspector 専用設定として残すか、実行時有効フラグと分離する。
- 実装では `SetTapPeelEnabled(bool enabled)` を追加し、`HandlePointer()` 実行条件を `allowTapPeel && runtimeTapPeelEnabled && !isPeelComplete` に整理する。
- これにより、配置フェーズ中は既存シールが存在してもめくり入力を受け付けない。

### 配置済みシールの追跡と全削除
- `StickerRuntimeRegistry` を `Dictionary<int, StickerRuntimeInfo>` のような構造へ拡張し、妖精有無と `PeelSticker3D` 参照をまとめて管理する。
- `Register` 時にシール参照を保存し、`TryConsumeFairy` 後も実体追跡が必要なら削除責務を分離する。
- `RemoveDestroyedSticker(PeelSticker3D sticker)` と `GetActiveStickers()` を用意し、自然破棄と強制削除の両方で整合を取る。
- `StickerPeeling` から戻る際は、`GetActiveStickers()` の返却対象に対して `Destroy(gameObject)` を実行し、その後 registry を掃除する。

### UI 連携
- `HudScreenBinder` は `UIDocument.rootVisualElement` から `ready-button` を取得する。
- ボタン押下時は `SealPhaseEventHub` のイベント発火だけを行い、フェーズ遷移ロジックは持たない。
- ボタン文言は現在フェーズに応じて以下のどちらかに更新する。
  - `StickerPlacement`: 例 `シールめくりへ`
  - `StickerPeeling`: 例 `シール配置へ`
- 文言の最終確定は実装時に行うが、「現在フェーズ」と「次に行う遷移」が読める文字列にする。
- `HudScreenBinder` は `SealPhaseEventHub` が通知する現在フェーズを受けて文言を更新する。
- `HudScreenBinder` は UI Toolkit の参照取得失敗時に早期ログを出し、ゲーム進行の無反応化を見つけやすくする。

## レイヤ別責務

### 進行管理
- `SealPhaseEventHub`
  - フェーズ切替要求イベントを発火する
  - フェーズ変更完了イベントを発火する
- `SealPhaseBootstrap`
  - `SealPhaseEventHub` を生成する
  - `HudScreenBinder` と `SealPhaseController` に同じインスタンスを注入する
- `SealPhaseController`
  - 現在フェーズ管理
  - フェーズ切替要求イベント購読
  - 配置 / めくり入力切替
  - 戻り時の未めくりシール全削除

### シール配置
- `TapStickerPlacer`
  - 配置可能時のみ既存の位置計算と生成を行う
  - 生成直後の registry 登録を継続する

### シールめくり
- `PeelSticker3D`
  - めくり可能時のみ既存の入力処理を行う
  - めくり完了後のログ出力、破棄、registry 解除を継続する

### UI
- `HudScreenBinder`
  - 右上ボタン参照取得
  - クリックでイベント中継をトリガー
  - フェーズ文言更新

## リスクと対策
- 既存の `PeelSticker3D` は各インスタンスが自己入力を受けるため、フェーズ切替直後に取りこぼしが出る可能性がある。
  - `SealPhaseController` が遷移時に全アクティブシールへ一括で `SetTapPeelEnabled` を反映する。
- 配置済みシールの一覧管理が不十分だと、戻り時の全削除漏れが発生する可能性がある。
  - registry に実体参照追跡 API を追加し、生成・自然破棄・強制削除のすべてで同じ管理経路を通す。
- UI Toolkit のボタン取得名がシーン上の実体と不一致だと、フェーズ切替できなくなる可能性がある。
  - `ready-button` の取得失敗時に `Debug.LogError` を出し、PlayMode 初回で即検知できるようにする。
- イベント購読解除漏れがあると、再生停止や再読込で重複購読が発生する可能性がある。
  - `HudScreenBinder` と `SealPhaseController` の購読登録 / 解除を `OnEnable` / `OnDisable` に揃える。
- `SealPhaseEventHub` の共有インスタンス生成が分散すると、UI と制御側で別インスタンスを握って通信できなくなる可能性がある。
  - 生成箇所を `SealPhaseBootstrap` の 1 箇所に固定する。
- `StickerPeeling` から `StickerPlacement` へ戻る瞬間に、めくり完了待ちシールと強制削除が競合する可能性がある。
  - 強制削除時は完了状態を問わず `Destroy` を優先し、registry 側は null 安全に掃除できる API にする。

## 検証方針
- 手動確認1:
  - 起動直後にシール配置ができ、既存シールをクリックしてもめくれないことを確認する。
- 手動確認2:
  - 右上ボタン押下で `StickerPeeling` に遷移し、新規配置ができなくなることを確認する。
- 手動確認3:
  - `StickerPeeling` 中に配置済みシールだけがめくれることを確認する。
- 手動確認4:
  - `StickerPeeling` 中に未配置領域をクリックしても新規シールが増えないことを確認する。
- 手動確認5:
  - `StickerPeeling` 中に右上ボタンを押すと `StickerPlacement` に戻り、未めくりシールが全削除されることを確認する。
- 手動確認6:
  - フェーズごとに右上ボタン文言が切り替わることを確認する。
- 手動確認7:
  - 妖精ありシールをめくったときの既存ログや挙動がフェーズ導入後も壊れていないことを確認する。

## コードスニペット
```csharp
public enum SealGamePhase
{
    StickerPlacement,
    StickerPeeling
}
```

```csharp
public sealed class SealPhaseEventHub
{
    public event Action PhaseToggleRequested;
    public event Action<SealGamePhase> PhaseChanged;

    public void RequestPhaseToggle() => PhaseToggleRequested?.Invoke();

    public void NotifyPhaseChanged(SealGamePhase phase) => PhaseChanged?.Invoke(phase);
}
```

```csharp
public sealed class SealPhaseBootstrap : MonoBehaviour
{
    [SerializeField] private HudScreenBinder hudScreenBinder;
    [SerializeField] private SealPhaseController sealPhaseController;

    private void Awake()
    {
        var eventHub = new SealPhaseEventHub();
        hudScreenBinder.Initialize(eventHub);
        sealPhaseController.Initialize(eventHub);
    }
}
```

```csharp
public sealed class SealPhaseController : MonoBehaviour
{
    public SealGamePhase CurrentPhase { get; private set; } = SealGamePhase.StickerPlacement;
    private SealPhaseEventHub eventHub;

    public void Initialize(SealPhaseEventHub eventHub)
    {
        this.eventHub = eventHub;
    }

    private void OnEnable()
    {
        eventHub.PhaseToggleRequested += HandlePhaseToggleRequested;
    }

    private void OnDisable()
    {
        eventHub.PhaseToggleRequested -= HandlePhaseToggleRequested;
    }

    private void HandlePhaseToggleRequested()
    {
        if (CurrentPhase == SealGamePhase.StickerPlacement)
        {
            SetPhase(SealGamePhase.StickerPeeling);
            return;
        }

        ClearRemainingStickers();
        SetPhase(SealGamePhase.StickerPlacement);
    }
}
```

```csharp
private void Update()
{
    if (!Application.isPlaying || !isPlacementEnabled)
    {
        return;
    }

    // 既存の配置入力処理
}
```

```csharp
public void SetTapPeelEnabled(bool enabled)
{
    runtimeTapPeelEnabled = enabled;
}
```
