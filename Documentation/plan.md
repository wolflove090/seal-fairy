# 配置シール選択機能 実装計画

## 実装方針
- 既存の配置処理は [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs) に集約されているため、今回も配置入口は維持しつつ、固定 `templateSticker` 依存を「現在選択中のシール定義」参照へ置き換える。
- UI は既存の [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml) と [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss) を拡張し、左下のスクロール一覧を追加する。
- フェーズ連携は既存の [SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs) と [HubScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を利用し、`StickerPlacement` のときだけ一覧表示、`StickerPeeling` では非表示にする。
- 所持シール一覧は Inspector の固定配列ではなく、所持品管理データを参照する構造を前提にする。ただし現段階のコードベースには所持品管理クラスが未存在のため、追加するデータ供給コンポーネントの責務を明確にして接続点を先に定義する。
- フェーズ再入時は一覧の選択状態を解除し、未選択時は配置しない。起動直後の初期表示のみ一覧先頭を選択状態にする。
- UI クリックとワールド配置の競合を避けるため、一覧セル押下中は既存の配置入力経路を通さないガードを入れる。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs)
  - 選択中シール定義からプレビュー元と生成 prefab を取得するよう変更する。
  - 未選択時は配置処理を行わないようにする。
  - UI 操作中の入力無効化を追加する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - 左下のスクロール一覧、一覧セル、選択状態表示、フェーズ切替時の表示制御を担当する。
  - `SealPhaseEventHub` の通知を受けて一覧の表示/非表示と選択解除を行う。
- [Assets/UI/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml)
  - 左下の所持シール一覧コンテナとスクロール要素を追加する。
- [Assets/UI/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss)
  - 左下パネル、スクロール、画像セル、選択ハイライトの見た目を追加する。
- [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs)
  - `StickerPlacement` 再入時に一覧選択解除の通知ができるよう、必要なら phase change 以外のイベントまたは初期化呼び出しを追加する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - 所持シールデータ供給コンポーネントや画像参照の接続が必要なら参照設定を追加する。

### 新規作成予定
- `Assets/Scripts/StickerSelection/OwnedStickerDefinition.cs`
  - 一覧表示画像、識別子、配置に使う `PeelSticker3D` prefab を保持する定義データ。
- `Assets/Scripts/StickerSelection/OwnedStickerInventorySource.cs`
  - 所持品管理データから現在の所持シール一覧を取得する窓口。
- `Assets/Scripts/StickerSelection/StickerSelectionState.cs`
  - 現在の一覧、選択中シール、初回選択済みフラグ、未選択状態を管理する。

## データフロー / 処理フロー
1. 起動時に `OwnedStickerInventorySource` が所持品管理データから所持シール一覧を取得する。
2. `HudScreenBinder` は一覧データを使って左下スクロール UI を構築し、起動直後のみ先頭シールを選択状態にする。
3. `HudScreenBinder` が一覧セル押下を検知すると、`StickerSelectionState` の選択中シールを更新し、UI の選択ハイライトを切り替える。
4. `TapStickerPlacer` は `StickerSelectionState` から現在の選択中シール定義を参照する。
5. `TapStickerPlacer` は選択中シールがある場合のみ、選択中 prefab を元にプレビューと配置生成を行う。
6. `TapStickerPlacer` は UI 上のポインタ操作中は配置入力を無視する。
7. `SealPhaseController` が `StickerPlacement -> StickerPeeling` へ遷移すると、`HudScreenBinder` は所持シール一覧を非表示にする。
8. `SealPhaseController` が `StickerPeeling -> StickerPlacement` へ遷移すると、`HudScreenBinder` と `StickerSelectionState` は選択状態を解除し、未選択状態へ戻す。
9. 未選択状態ではプレビューを非表示にし、クリックまたはタップしてもシールは生成しない。

## 処理詳細

### シール定義と所持品データ
- `OwnedStickerDefinition` は `string id`、`Sprite icon` または `Texture2D iconTexture`、`PeelSticker3D stickerPrefab` を持つ。
- 一覧セル表示は画像のみを必須とし、名称ラベルは今回のスコープに含めない。
- `OwnedStickerInventorySource` は将来の所持品管理データ接続を前提に、`IReadOnlyList<OwnedStickerDefinition>` を返す API を持つ。
- 実装初期段階では、所持品管理データの実体が未整備なら、将来差し替え可能な仮実装クラスを 1 箇所に閉じ込める。

### 選択状態
- 起動直後の `StickerPlacement` 開始時のみ、一覧先頭を自動選択する。
- `StickerPeeling` へ遷移した時点では、一覧は非表示にするが選択状態は保持してよい。
- `StickerPlacement` に戻る遷移時に明示的に選択解除し、再開直後は未選択状態とする。
- 未選択状態ではプレビュー非表示、配置不可、一覧上のハイライトなしを一貫させる。
- 同じセルを再タップしても未選択に戻さず、そのまま選択維持とする。

### UI 構造
- `HudScreen.uxml` に `bottom-left-sticker-panel` と `sticker-scroll-view` を追加する。
- スクロール内の一覧セルは `VisualElement` または `Button` をテンプレート化して動的生成する。
- 各セルには画像表示用 `VisualElement` か `Image` を 1 つ配置し、選択中セルには class を追加してハイライトする。
- 既存の `top-bar` と `bottom-right-menu` は維持し、左下一覧だけを追加する。

### 配置処理
- `TapStickerPlacer` は `CacheTemplateSticker()` を固定テンプレート探索から選択中 prefab ベースのキャッシュへ置き換える。
- 選択変更時にプレビュー用インスタンスを差し替える必要があるため、現在のプレビューオブジェクトを破棄して新しい選択中シールから再生成する責務を持たせる。
- 配置時の妖精判定と `StickerRuntimeRegistry.Register` は現在のまま維持し、シール種類が増えても registry の責務は変更しない。
- 配置済みシールをクリックした際の配置抑止ロジック `IsPointerOverSticker` は継続利用する。

### フェーズ連携
- `HubScreenBinder` は `SealPhaseEventHub.PhaseChanged` をすでに購読しているため、一覧表示制御も同クラスへ集約する。
- 既存の `ready-button` 文言更新に加えて、`StickerPlacement` なら一覧表示、`StickerPeeling` なら非表示を更新する。
- `StickerPlacement` 再入時の選択解除は、`HubScreenBinder` から `StickerSelectionState.ClearSelection()` を呼ぶ。

## リスクと対策
- 所持品管理データの実体が未整備のまま UI だけ先行すると、将来の接続点が破綻しやすい。
  - 一覧取得の入口を `OwnedStickerInventorySource` に限定し、UI や配置処理は具体的な保存形式を知らない構造にする。
- `TapStickerPlacer` は現在テンプレート探索と生成を同時に担っているため、選択変更対応を安易に足すと責務が肥大化する。
  - 選択状態参照、プレビュー差し替え、実配置生成をメソッド分割して整理する。
- UI Toolkit のスクロール一覧動的生成で name 依存を増やすと保守しにくい。
  - ルート要素の固定 name は最小限にして、セル見た目は class ベースで制御する。
- フェーズ再入時の未選択化と起動直後の先頭選択が混同されると、挙動がぶれやすい。
  - `初回起動` と `フェーズ再入` を別トリガーで扱い、受け入れ条件に沿って切り分ける。
- UI 上の画像セル押下がワールド配置へ伝播すると誤配置が起きる。
  - `EventSystem` / UI Toolkit の pointer over 判定を使い、UI 操作中は `TapStickerPlacer` が即 return する。

## 検証方針
- 手動確認1:
  - 起動直後の `StickerPlacement` で左下一覧が表示され、先頭シールが選択済みになっていることを確認する。
- 手動確認2:
  - 一覧内の別シールを押すと選択ハイライトが切り替わり、プレビュー見た目が変わることを確認する。
- 手動確認3:
  - 選択後にワールドをクリックまたはタップすると、選択したシール prefab が配置されることを確認する。
- 手動確認4:
  - 一覧セル押下時にワールドへシールが誤配置されないことを確認する。
- 手動確認5:
  - `StickerPeeling` へ遷移すると一覧が非表示になることを確認する。
- 手動確認6:
  - `StickerPlacement` へ戻ると選択が解除され、未選択のままでは配置されないことを確認する。
- 手動確認7:
  - 再度一覧からシールを選択すると配置を再開できることを確認する。
- 手動確認8:
  - 所持シール 0 件でも UI が破綻せず、空状態が判別できることを確認する。

## コードスニペット
```csharp
public sealed class OwnedStickerDefinition
{
    public string Id;
    public Sprite Icon;
    public PeelSticker3D StickerPrefab;
}
```

```csharp
public sealed class StickerSelectionState
{
    public IReadOnlyList<OwnedStickerDefinition> OwnedStickers { get; private set; }
    public OwnedStickerDefinition SelectedSticker { get; private set; }

    public void SelectInitialSticker()
    {
        SelectedSticker = OwnedStickers.Count > 0 ? OwnedStickers[0] : null;
    }

    public void ClearSelection()
    {
        SelectedSticker = null;
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

    if (selectionState.SelectedSticker == null)
    {
        return;
    }

    if (IsPointerOverUi())
    {
        return;
    }

    // 選択中シールのプレビュー更新と配置生成
}
```
