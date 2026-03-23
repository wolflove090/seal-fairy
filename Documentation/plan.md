# 配置シールマウス追従プレビュー 実装計画

## 実装方針
- 既存の配置処理は [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) を中心に維持し、その中へ「選択中シールのプレビュー更新」と「HUD へのプレビュー位置通知」を追加する。
- 選択状態の真実のソースは引き続き [StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) とし、選択変更通知イベントを追加して UI と配置処理の同期を安定化する。
- 残数の真実のソースは [OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs) に残し、残数表示はその値を参照して更新する。
- 残数表示は UI Toolkit の画面固定ラベルとして生成しつつ、表示位置は `TapStickerPlacer` から渡されるスクリーン座標を使ってカーソル近傍へ追従させる。
- 毎フレームの `Instantiate` / `Destroy` は避け、選択 prefab が変化したときのみプレビューインスタンスを差し替える。
- フェーズ切り替え、選択解除、残数 0、配置面ヒット失敗のいずれでも、プレビューシールと残数ラベルが同時に消える一貫した挙動にする。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs)
  - プレビュー用 `PeelSticker3D` の表示更新を追加する。
  - 毎フレームのマウス位置からプレビュー座標を計算する。
  - HUD へスクリーン座標と表示可否を通知するイベントまたは API を追加する。
  - フェーズ無効時や残数 0 時にプレビューを消す。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - プレビュー残数ラベルの参照取得、文言更新、位置更新、表示切り替えを追加する。
  - `StickerSelectionState` の選択変更通知を購読し、選択シール変更時に残数表示を更新する。
  - `OwnedStickerInventorySource` の変更通知とフェーズ変更通知で残数表示を同期する。
- [Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs)
  - 選択変更通知イベントを追加する。
  - `Select` と `ClearSelection` の変更時のみイベントを発火する。
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
  - ルート直下にプレビュー残数表示用 `Label` を追加する。
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
  - プレビュー残数ラベルの見た目と absolute 配置スタイルを追加する。
- [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs)
  - `TapStickerPlacer` のプレビュー非表示制御が不足する場合のみ、フェーズ無効化時の明示通知を追加する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - 必要に応じて `HudScreenBinder` と `TapStickerPlacer` の参照設定を確認する。

## データフロー / 処理フロー
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の一覧セル押下で `StickerSelectionState.Select(sticker)` を実行する。
2. [StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) が `SelectedStickerChanged` を通知する。
3. [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) が選択 prefab を再評価し、必要時のみプレビューインスタンスを再生成する。
4. `Update()` で `Input.mousePosition` を取得し、既存の `TryGetPlaneHitPoint` でワールド座標へ変換する。
5. 配置フェーズ中かつ選択中シールの残数が 1 以上なら、プレビューオブジェクトをそのワールド座標へ移動し、`WorldToScreenPoint` で残数ラベル用座標を計算する。
6. `TapStickerPlacer` が `PreviewScreenPointChanged` を通知し、`HubScreenBinder` が `preview-count-label` の位置を更新する。
7. `HubScreenBinder` は `inventorySource.GetOwnedStickerCount(selectionState.SelectedSticker)` を参照して残数文言を `残り xN` として表示する。
8. プレイヤーがクリックまたはタップすると、既存配置ロジックでシール生成後に `inventorySource.RemoveOwnedSticker(selectedSticker)` が実行される。
9. `OwnedStickersChanged` により一覧再構築と残数再計算が行われ、残数 0 または選択解除ならプレビューと残数ラベルが非表示になる。
10. `SealPeeling` 遷移時は `TapStickerPlacer.SetPlacementEnabled(false)` を契機にプレビュー更新を止め、HUD 側もラベルを非表示にする。

## 処理詳細

### 1. 選択変更通知の追加
- `StickerSelectionState` に `event Action<StickerDefinition> SelectedStickerChanged` を追加する。
- `Select` では選択対象が変わったときのみ通知する。
- `ClearSelection` では `null` 通知を送る。
- `BuildStickerList()` 内の再選択や在庫変動時も、このイベントを通じて `TapStickerPlacer` と `HubScreenBinder` が最新状態を受け取れるようにする。

### 2. プレビューインスタンスの管理
- `TapStickerPlacer` の `templateSticker` をプレビュー実体として再利用する。
- `CacheTemplateSticker()` で選択 prefab が変わった場合のみ旧プレビューを破棄し、新しい prefab から生成する。
- プレビューは `gameObject.SetActive(false)` を基本とし、表示条件を満たしたフレームだけアクティブ化する。
- 既存の配置処理はプレビューインスタンスを複製して本体シールを生成する流れを維持する。

### 3. プレビュー位置更新
- `Update()` の先頭で `UpdatePreview()` を呼び、配置入力判定より前にプレビュー表示だけ更新する。
- `UpdatePreview()` は以下を判定する。
  - `Application.isPlaying`
  - `isPlacementEnabled`
  - `selectionState?.SelectedSticker != null`
  - `inventorySource.GetOwnedStickerCount(selectedSticker) > 0`
  - アクティブカメラ取得成功
  - 配置平面ヒット成功
- 条件を満たした場合のみプレビュー位置を更新し、スクリーン座標通知を送る。
- 条件を満たさない場合は `SetPreviewVisible(false)` と残数ラベル非表示通知を送る。

### 4. 残数表示ラベル
- `HudScreen.uxml` に `Label name="preview-count-label"` を追加する。
- `HubScreenBinder.OnEnable()` で `previewCountLabel = root.Q<Label>("preview-count-label");` を取得する。
- 表示文言は `残り x{count}` に統一する。
- `TapStickerPlacer` から受け取ったスクリーン座標に固定オフセットを加えて `style.left` / `style.top` を更新する。
- 画面外へはみ出す場合は `root.layout.width` と `root.layout.height` を使って clamp する。

### 5. フェーズ連携
- `HubScreenBinder.HandlePhaseChanged()` で既存の一覧表示切り替えに加え、`preview-count-label` の表示可否も更新する。
- `SealPhaseController` 側の `SetPlacementEnabled(false)` により `TapStickerPlacer` 側の `UpdatePreview()` が停止するため、同フレーム内で非表示通知を送る。
- `SealPeeling` から `SealPlacement` に戻った際、既存仕様どおり未選択ならプレビューも残数表示も出さない。

## リスクと対策
- `TapStickerPlacer` が入力・配置・プレビュー通知を持つことで肥大化する。
  - プレビュー位置計算を `UpdatePreview()` と `NotifyPreviewScreenPoint()` に分離し、配置処理と明確に分ける。
- 在庫変更後に選択解除とラベル更新の順序がずれると、一瞬だけ古い残数が表示される可能性がある。
  - `OwnedStickersChanged` を受けた `BuildStickerList()` の最後で残数表示更新を必ず再実行する。
- UI 上にマウスがある間もプレビューだけ動き続けると違和感が出る可能性がある。
  - UI 操作中はプレビュー位置更新も止めるか、最後の有効位置で固定するかを 1 つに統一する。今回は非表示に寄せる。
- 画面端で残数ラベルが見切れる可能性がある。
  - スクリーン座標にオフセットを加えた後で clamp する。
- プレビュー prefab が通常シールと同じ描画設定だと視認しづらい可能性がある。
  - 必要ならマテリアル色や透明度の Inspector 調整ポイントを追加するが、初回実装は最小変更で進める。

## 検証方針
- 手動確認1: `SealPlacement` 中にシール選択後、マウス移動に応じて選択中シールのプレビューが追従する。
- 手動確認2: プレビュー近傍に `残り xN` が表示され、一覧セル内の所持数と一致する。
- 手動確認3: 別シールを選択すると、プレビュー見た目と残数表示が即時に切り替わる。
- 手動確認4: シールを配置すると残数表示が即時に 1 減る。
- 手動確認5: 残数 0 でプレビューと残数表示が消え、未選択状態になる。
- 手動確認6: `SealPeeling` 遷移でプレビューと残数表示が消える。
- 手動確認7: UI 一覧や各ボタン操作中に、意図せずシール配置が発生しない。
- 手動確認8: 既存のシール配置、剥がし、ショップ、妖精コレクションが退行しない。

## コードスニペット
```csharp
public sealed class StickerSelectionState
{
    public event Action<StickerDefinition> SelectedStickerChanged;

    public IReadOnlyList<StickerDefinition> OwnedStickers { get; private set; }
    public StickerDefinition SelectedSticker { get; private set; }

    public void Select(StickerDefinition sticker)
    {
        if (SelectedSticker == sticker)
        {
            return;
        }

        SelectedSticker = sticker;
        SelectedStickerChanged?.Invoke(SelectedSticker);
    }

    public void ClearSelection()
    {
        if (SelectedSticker == null)
        {
            return;
        }

        SelectedSticker = null;
        SelectedStickerChanged?.Invoke(null);
    }
}
```

```csharp
private void UpdatePreview()
{
    StickerDefinition selectedSticker = selectionState?.SelectedSticker;
    int ownedCount = inventorySource != null && selectedSticker != null
        ? inventorySource.GetOwnedStickerCount(selectedSticker)
        : 0;

    if (!isPlacementEnabled || selectedSticker == null || ownedCount <= 0)
    {
        SetPreviewVisible(false);
        NotifyPreviewScreenPoint(Vector2.zero, false);
        return;
    }

    Camera activeCamera = GetActiveCamera();
    if (activeCamera == null)
    {
        SetPreviewVisible(false);
        NotifyPreviewScreenPoint(Vector2.zero, false);
        return;
    }

    CacheTemplateSticker();
    Vector3 screenPoint = Input.mousePosition;
    if (templateSticker == null ||
        !templateSticker.TryGetPlaneHitPoint(activeCamera, screenPoint, out Vector3 worldPoint))
    {
        SetPreviewVisible(false);
        NotifyPreviewScreenPoint(Vector2.zero, false);
        return;
    }

    templateSticker.transform.position = worldPoint;
    templateSticker.gameObject.SetActive(true);
    NotifyPreviewScreenPoint(screenPoint, true);
}
```
