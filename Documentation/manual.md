# 配置シールマウス追従プレビュー 作業手順書

## 目的
- `SealPlacement` フェーズ中、選択中シールをマウス位置へ追従表示する。
- プレビュー付近に、選択中シールの残数を `残り xN` 形式で表示する。
- 選択変更、在庫変動、フェーズ切り替えに応じてプレビューと残数表示が同期するようにする。

## 変更対象
- [Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs)
- [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
- 必要に応じて [Assets/Scripts/Phase/SealPhaseController.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseController.cs)

## 手順1: StickerSelectionState に選択変更イベントを追加する
1. [StickerSelectionState.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/StickerSelectionState.cs) を開く。
2. `using System;` を追加する。
3. `SelectedStickerChanged` イベントを追加する。
4. `Select` と `ClearSelection` で、状態が変わった場合のみ通知する。

### 変更後コード
```csharp
using System;
using System.Collections.Generic;

public sealed class StickerSelectionState
{
    public event Action<StickerDefinition> SelectedStickerChanged;

    public IReadOnlyList<StickerDefinition> OwnedStickers { get; private set; }
    public StickerDefinition SelectedSticker { get; private set; }

    public void SetOwnedStickers(IReadOnlyList<StickerDefinition> ownedStickers)
    {
        OwnedStickers = ownedStickers;
    }

    public void SelectInitialSticker()
    {
        Select(OwnedStickers != null && OwnedStickers.Count > 0 ? OwnedStickers[0] : null);
    }

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

## 手順2: HudScreen.uxml にプレビュー残数ラベルを追加する
1. [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) を開く。
2. `root` 直下にプレビュー残数ラベルを追加する。
3. 初期状態では非表示運用するため、名前だけ定義しておけばよい。

### 追加コード
```xml
<ui:Label name="preview-count-label" text="残り x0" />
```

### 配置例
```xml
<ui:VisualElement name="root">
    <ui:Label name="preview-count-label" text="残り x0" />
    <ui:VisualElement name="top-bar">
        ...
```

## 手順3: HudScreen.uss にプレビュー残数ラベルのスタイルを追加する
1. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) を開く。
2. `#preview-count-label` 用スタイルを追加する。
3. absolute 配置、半透明背景、小型チップ見た目を設定する。

### 追加コード
```css
#preview-count-label {
    position: absolute;
    left: 0;
    top: 0;
    padding-left: 14px;
    padding-right: 14px;
    padding-top: 8px;
    padding-bottom: 8px;
    background-color: rgba(0, 0, 0, 0.72);
    color: rgb(255, 255, 255);
    font-size: 28px;
    -unity-text-align: middle-center;
    border-top-left-radius: 999px;
    border-top-right-radius: 999px;
    border-bottom-left-radius: 999px;
    border-bottom-right-radius: 999px;
    display: none;
}
```

## 手順4: TapStickerPlacer にプレビュー位置通知イベントと補助メソッドを追加する
1. [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) を開く。
2. `using System;` を追加する。
3. `public event Action<Vector2, bool> PreviewScreenPointChanged;` を追加する。
4. `SetPreviewVisible(bool visible)` と `NotifyPreviewScreenPoint(Vector2 point, bool visible)` を追加する。
5. `SetPlacementEnabled(false)` 時にプレビューと通知を消す処理を追加する。

### 追加コード例
```csharp
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class TapStickerPlacer : MonoBehaviour
{
    public event Action<Vector2, bool> PreviewScreenPointChanged;

    private void SetPreviewVisible(bool visible)
    {
        if (templateSticker != null)
        {
            templateSticker.gameObject.SetActive(visible);
        }
    }

    private void NotifyPreviewScreenPoint(Vector2 point, bool visible)
    {
        PreviewScreenPointChanged?.Invoke(point, visible);
    }
}
```

## 手順5: TapStickerPlacer にプレビュー更新処理を追加する
1. `Update()` 冒頭で `UpdatePreview()` を呼ぶ。
2. `UpdatePreview()` を新規追加し、配置フェーズ中かつ選択中シールあり、残数あり、配置面ヒット成功時のみプレビューを表示する。
3. `Input.mousePosition` を使用してスクリーン座標を取り、`templateSticker.TryGetPlaneHitPoint()` でワールド座標へ変換する。
4. 成功時はプレビュー位置更新と `NotifyPreviewScreenPoint(screenPoint, true)` を行う。
5. 失敗時は `SetPreviewVisible(false)` と `NotifyPreviewScreenPoint(Vector2.zero, false)` を行う。

### 追加コード
```csharp
private void Update()
{
    if (!Application.isPlaying)
    {
        return;
    }

    UpdatePreview();

    if (!isPlacementEnabled)
    {
        return;
    }

    if (selectionState?.SelectedSticker == null)
    {
        return;
    }

    if (IsPointerOverUi())
    {
        return;
    }

    if (!TryGetPointerDownPosition(out Vector3 screenPoint))
    {
        return;
    }

    Camera activeCamera = GetActiveCamera();
    if (activeCamera == null)
    {
        return;
    }

    CacheTemplateSticker();
    if (templateSticker == null)
    {
        return;
    }

    if (IsPointerOverSticker(activeCamera, screenPoint))
    {
        return;
    }

    if (!templateSticker.TryGetPlaneHitPoint(activeCamera, screenPoint, out Vector3 worldPoint))
    {
        return;
    }

    StickerDefinition selectedSticker = selectionState.SelectedSticker;
    SpawnSticker(worldPoint);
    inventorySource?.RemoveOwnedSticker(selectedSticker);
}

private void UpdatePreview()
{
    StickerDefinition selectedSticker = selectionState?.SelectedSticker;
    int ownedCount = inventorySource != null && selectedSticker != null
        ? inventorySource.GetOwnedStickerCount(selectedSticker)
        : 0;

    if (!isPlacementEnabled || selectedSticker == null || ownedCount <= 0 || IsPointerOverUi())
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
    if (templateSticker == null)
    {
        NotifyPreviewScreenPoint(Vector2.zero, false);
        return;
    }

    Vector3 screenPoint = Input.mousePosition;
    if (!templateSticker.TryGetPlaneHitPoint(activeCamera, screenPoint, out Vector3 worldPoint))
    {
        SetPreviewVisible(false);
        NotifyPreviewScreenPoint(Vector2.zero, false);
        return;
    }

    templateSticker.transform.SetPositionAndRotation(worldPoint, templateSticker.transform.rotation);
    templateSticker.gameObject.SetActive(true);
    templateSticker.PeelAmount = 0f;
    templateSticker.SetTapPeelEnabled(false);

    NotifyPreviewScreenPoint(screenPoint, true);
}
```

## 手順6: HubScreenBinder にプレビュー残数ラベル管理を追加する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. `previewCountLabel`、`previewLabelOffset`、`tapStickerPlacer` 参照を追加する。
3. `OnEnable()` で `preview-count-label` を取得し、`TapStickerPlacer.PreviewScreenPointChanged` と `StickerSelectionState.SelectedStickerChanged` を購読する。
4. `OnDisable()` で購読解除する。
5. `UpdatePreviewCountLabel()`, `HandlePreviewScreenPointChanged()`, `SetPreviewCountVisible()` を追加する。

### 追加フィールド例
```csharp
[SerializeField] private TapStickerPlacer tapStickerPlacer;
[SerializeField] private Vector2 previewCountOffset = new(28f, -36f);

private Label previewCountLabel;
```

### 追加メソッド例
```csharp
private void HandleSelectedStickerChanged(StickerDefinition _)
{
    UpdatePreviewCountLabel();
}

private void HandlePreviewScreenPointChanged(Vector2 screenPoint, bool visible)
{
    if (previewCountLabel == null)
    {
        return;
    }

    if (!visible || currentPhase != SealGamePhase.StickerPlacement || selectionState.SelectedSticker == null)
    {
        previewCountLabel.style.display = DisplayStyle.None;
        return;
    }

    Vector2 position = screenPoint + previewCountOffset;
    float width = previewCountLabel.resolvedStyle.width;
    float height = previewCountLabel.resolvedStyle.height;
    float maxX = Mathf.Max(0f, uiDocument.rootVisualElement.layout.width - width);
    float maxY = Mathf.Max(0f, uiDocument.rootVisualElement.layout.height - height);

    position.x = Mathf.Clamp(position.x, 0f, maxX);
    position.y = Mathf.Clamp(position.y, 0f, maxY);

    previewCountLabel.style.left = position.x;
    previewCountLabel.style.top = position.y;
    previewCountLabel.style.display = DisplayStyle.Flex;
}

private void UpdatePreviewCountLabel()
{
    if (previewCountLabel == null)
    {
        return;
    }

    StickerDefinition selectedSticker = selectionState.SelectedSticker;
    if (selectedSticker == null || inventorySource == null)
    {
        previewCountLabel.style.display = DisplayStyle.None;
        return;
    }

    int count = inventorySource.GetOwnedStickerCount(selectedSticker);
    previewCountLabel.text = $"残り x{count}";
}
```

## 手順7: BuildStickerList とフェーズ変更時に残数表示を同期する
1. `BuildStickerList()` の最後で `UpdatePreviewCountLabel()` を呼ぶ。
2. 所持シール 0 件時と選択解除時は `previewCountLabel.style.display = DisplayStyle.None;` を行う。
3. `HandlePhaseChanged()` で `SealPeeling` 時は非表示、`SealPlacement` 再入時は未選択なら非表示のままにする。

### 追加コード例
```csharp
private void HandlePhaseChanged(SealGamePhase phase)
{
    bool returnedToPlacement = currentPhase == SealGamePhase.StickerPeeling && phase == SealGamePhase.StickerPlacement;
    currentPhase = phase;

    if (returnedToPlacement)
    {
        selectionState.ClearSelection();
    }

    UpdateReadyButtonLabel();
    UpdateStickerPanelVisibility();
    UpdateAuxiliaryButtonVisibility();
    UpdatePreviewCountLabel();

    if (phase != SealGamePhase.StickerPlacement && previewCountLabel != null)
    {
        previewCountLabel.style.display = DisplayStyle.None;
    }
}
```

## 手順8: Unity 上の参照設定を確認する
1. `HubScreenBinder` に `Tap Sticker Placer` 参照を追加した場合は、[Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で割り当てる。
2. `UIDocument` が `HudScreen.uxml` の最新レイアウトを参照していることを確認する。
3. `TapStickerPlacer` の `templateSticker` がプレビュー生成に使える prefab であることを確認する。

## 手順9: 手動確認を行う
1. `SealPlacement` 中にシールを選択し、マウス位置へプレビューが追従することを確認する。
2. プレビュー近傍に `残り xN` が表示されることを確認する。
3. シールを配置すると `残り xN` が即時に減ることを確認する。
4. 別シール選択時にプレビュー見た目と残数表示が切り替わることを確認する。
5. 残数 0 のシールはプレビューと残数表示が消えることを確認する。
6. `SealPeeling` 遷移時にプレビューと残数表示が消えることを確認する。
7. UI 一覧・ショップ・妖精ボタン操作中に誤配置が起きないことを確認する。
8. 既存の配置、剥がし、ショップ、妖精コレクション機能に退行がないことを確認する。

## 注意点
- `UpdatePreview()` と配置クリック処理の両方で `CacheTemplateSticker()` を使うため、選択変更時だけ再生成される状態を維持する。
- 残数表示の座標通知はスクリーン座標をそのまま渡し、UI 側でオフセットと clamp を担当する。
- PC マウス操作前提のため、モバイル向けの指下オフセット分岐は追加しない。
