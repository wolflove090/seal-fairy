# 妖精コレクション詳細画面 作業手順書

## 目的
- 妖精コレクション一覧の発見済み妖精カードを押したときに、個別詳細モーダルを表示できるようにする。
- 詳細モーダルは `X` ボタンと背景タップで閉じられるようにする。
- 配色は既存のピンク基調 UI に統一し、緑系の背景は使わない。
- 未発見妖精カードは一覧には表示するが、詳細モーダルは開けないようにする。

## 変更対象
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml)
- [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)

## 手順1: UXML に詳細モーダル要素を追加する
1. [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml) を開く。
2. 既存の `fairy-collection-panel` はそのまま残す。
3. `fairy-collection-overlay` の直下、一覧パネルの後ろではなく前面レイヤーとして `fairy-detail-overlay` を追加する。
4. `fairy-detail-overlay` 配下に背景ボタン `fairy-detail-backdrop` と、詳細パネル `fairy-detail-panel` を置く。
5. `fairy-detail-panel` 内に、閉じるボタン、妖精名、画像枠、好きなシール表示、フレーバー表示を配置する。

### 追加例
```xml
<ui:VisualElement name="fairy-detail-overlay">
    <ui:Button name="fairy-detail-backdrop" />
    <ui:VisualElement name="fairy-detail-panel">
        <ui:Button name="fairy-detail-close-button" />
        <ui:Label name="fairy-detail-name" text="妖精名" />
        <ui:VisualElement name="fairy-detail-image-frame">
            <ui:VisualElement name="fairy-detail-image" />
        </ui:VisualElement>
        <ui:Label name="fairy-detail-favorite-label" text="好きなシール：" />
        <ui:Label name="fairy-detail-favorite-value" text="クール/ワイルド" />
        <ui:Label name="fairy-detail-flavor-label" text="フレーバー：" />
        <ui:Label name="fairy-detail-flavor-value" text="ホゲホゲ。フガフガ あいうえお" />
    </ui:VisualElement>
</ui:VisualElement>
```

## 手順2: USS に詳細モーダルの見た目を追加する
1. [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) を開く。
2. `#fairy-detail-overlay` を全画面 absolute + `display: none;` で定義する。
3. `#fairy-detail-backdrop` は半透明の暗い色で画面全体を覆う。
4. `#fairy-detail-panel` は中央寄せの固定サイズにし、濃いピンクの外装と淡いピンクの内側配色で構成する。
5. `#fairy-detail-close-button` はショップと同系統の白ベース角丸ボタンにする。
6. `#fairy-detail-image-frame` と `#fairy-detail-image` は、画像が大きく見えるサイズで定義する。
7. カードを `Button` 化する前提で、`.fairy-card` のデフォルトボタン装飾を打ち消すスタイルを加える。
8. 未発見用クラス `.fairy-card--locked` を追加し、ホバーや押下感を抑える。

### 追加例
```css
#fairy-detail-overlay {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    display: none;
}

#fairy-detail-panel {
    position: absolute;
    left: 50%;
    top: 50%;
    width: 640px;
    min-height: 860px;
    translate: -320px -430px;
    padding: 30px;
    background-color: rgb(244, 132, 164);
    border-top-left-radius: 28px;
    border-top-right-radius: 28px;
    border-bottom-left-radius: 28px;
    border-bottom-right-radius: 28px;
}
```

```css
.fairy-card {
    padding: 18px;
    border-left-width: 2px;
    border-right-width: 2px;
    border-top-width: 3px;
    border-bottom-width: 4px;
}

.fairy-card--locked {
    opacity: 0.82;
}
```

## 手順3: `HudScreenBinder` に詳細モーダル参照を追加する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. 妖精コレクション関連フィールドの近くに、詳細モーダル用の private フィールドを追加する。
3. 固定文言の定数も同ファイルに追加する。

### 追加例
```csharp
private const string FairyDetailFavoriteText = "クール/ワイルド";
private const string FairyDetailFlavorText = "ホゲホゲ。フガフガ あいうえお";

private VisualElement fairyDetailOverlay;
private Button fairyDetailBackdrop;
private VisualElement fairyDetailPanel;
private Button fairyDetailCloseButton;
private Label fairyDetailNameLabel;
private VisualElement fairyDetailImage;
private Label fairyDetailFavoriteValueLabel;
private Label fairyDetailFlavorValueLabel;
```

## 手順4: `InitializeFairyCollectionUi()` で詳細モーダルを取得する
1. `InitializeFairyCollectionUi(VisualElement root)` を探す。
2. 既存一覧 UI の取得処理の後に、詳細モーダル要素も `Q<>()` で取得する。
3. 必須要素が 1 つでも欠けていたら `Debug.LogError` を出して `return` する。
4. `fairyDetailOverlay.style.display = DisplayStyle.None;` を設定する。

### 変更例
```csharp
fairyDetailOverlay = root.Q<VisualElement>("fairy-detail-overlay");
fairyDetailBackdrop = root.Q<Button>("fairy-detail-backdrop");
fairyDetailPanel = root.Q<VisualElement>("fairy-detail-panel");
fairyDetailCloseButton = root.Q<Button>("fairy-detail-close-button");
fairyDetailNameLabel = root.Q<Label>("fairy-detail-name");
fairyDetailImage = root.Q<VisualElement>("fairy-detail-image");
fairyDetailFavoriteValueLabel = root.Q<Label>("fairy-detail-favorite-value");
fairyDetailFlavorValueLabel = root.Q<Label>("fairy-detail-flavor-value");

if (fairyDetailOverlay == null ||
    fairyDetailBackdrop == null ||
    fairyDetailPanel == null ||
    fairyDetailCloseButton == null ||
    fairyDetailNameLabel == null ||
    fairyDetailImage == null ||
    fairyDetailFavoriteValueLabel == null ||
    fairyDetailFlavorValueLabel == null)
{
    Debug.LogError("妖精詳細 UI の初期化に失敗しました");
    return;
}

fairyDetailOverlay.style.display = DisplayStyle.None;
```

## 手順5: 開閉イベントを登録する
1. `OnEnable()` に、`fairyDetailCloseButton.clicked += CloseFairyDetail;` を追加する。
2. `fairyDetailBackdrop.clicked += CloseFairyDetail;` も追加する。
3. `OnDisable()` では両方の解除を書く。

### 変更例
```csharp
if (fairyDetailCloseButton != null)
{
    fairyDetailCloseButton.clicked += CloseFairyDetail;
}

if (fairyDetailBackdrop != null)
{
    fairyDetailBackdrop.clicked += CloseFairyDetail;
}
```

## 手順6: 詳細表示メソッドを追加する
1. `OpenFairyCollection()` / `CloseFairyCollection()` の近くに、詳細モーダル制御メソッドを追加する。
2. `OpenFairyDetail(FairyDefinition fairy)` は `ApplyFairyDetail(fairy)` を呼んでから overlay を表示する。
3. `ApplyFairyDetail(FairyDefinition fairy)` は、名前、画像、好きなシール、フレーバーの各 UI を埋める。
4. `CloseFairyDetail()` は overlay を非表示にする。

### 追加例
```csharp
private void OpenFairyDetail(FairyDefinition fairy)
{
    if (fairy == null || fairyDetailOverlay == null)
    {
        return;
    }

    ApplyFairyDetail(fairy);
    fairyDetailOverlay.style.display = DisplayStyle.Flex;
}

private void ApplyFairyDetail(FairyDefinition fairy)
{
    fairyDetailNameLabel.text = string.IsNullOrWhiteSpace(fairy.DisplayName)
        ? "名称未設定"
        : fairy.DisplayName;

    fairyDetailFavoriteValueLabel.text = FairyDetailFavoriteText;
    fairyDetailFlavorValueLabel.text = FairyDetailFlavorText;

    if (fairy != null && fairy.Icon != null)
    {
        fairyDetailImage.style.backgroundImage = new StyleBackground(fairy.Icon.texture);
    }
    else
    {
        fairyDetailImage.style.backgroundImage = StyleKeyword.None;
    }
}

private void CloseFairyDetail()
{
    if (fairyDetailOverlay == null)
    {
        return;
    }

    fairyDetailOverlay.style.display = DisplayStyle.None;
}
```

## 手順7: 一覧カードを発見済みのみ選択可能に変更する
1. `CreateFairyCard(FairyDefinition fairy, bool isDiscovered)` を探す。
2. 戻り値は `VisualElement` のままでよいが、生成実体は `Button` にする。
3. 発見済みかつ `fairy != null` のときだけ `clicked` に `OpenFairyDetail(fairy)` を登録する。
4. 未発見のときは `fairy-card--locked` クラスを追加し、クリックイベントは付けない。
5. 名前や画像の既存表示ロジックは維持する。

### 変更例
```csharp
private VisualElement CreateFairyCard(FairyDefinition fairy, bool isDiscovered)
{
    Button card = new();
    card.AddToClassList("fairy-card");

    if (isDiscovered && fairy != null)
    {
        card.clicked += () => OpenFairyDetail(fairy);
    }
    else
    {
        card.AddToClassList("fairy-card--locked");
    }

    Label nameLabel = new();
    nameLabel.AddToClassList("fairy-card__name");
    nameLabel.text = isDiscovered && fairy != null ? fairy.DisplayName : "？？？";

    // 既存の image / detail 組み立てを続ける
    return card;
}
```

## 手順8: 一覧画面を閉じるときに詳細も閉じる
1. `OpenFairyCollection()` の先頭で `CloseFairyDetail();` を呼ぶ。
2. `CloseFairyCollection()` の先頭でも `CloseFairyDetail();` を呼ぶ。
3. これにより、再オープン時の詳細残留を防ぐ。

### 変更例
```csharp
private void OpenFairyCollection()
{
    if (fairyCollectionOverlay == null || fairyCollectionScrollView == null)
    {
        return;
    }

    CloseStickerShop();
    CloseFairyDetail();
    RefreshFairyCollection();
    fairyCollectionOverlay.style.display = DisplayStyle.Flex;
}

private void CloseFairyCollection()
{
    if (fairyCollectionOverlay == null)
    {
        return;
    }

    CloseFairyDetail();
    fairyCollectionOverlay.style.display = DisplayStyle.None;
}
```

## 手順9: 動作確認を行う
1. Unity でゲームを起動する。
2. HUD の `妖精` ボタンから一覧を開く。
3. 発見済み妖精カードを押し、詳細モーダルが開くことを確認する。
4. 詳細に妖精名、画像、`クール/ワイルド`、固定フレーバーテキスト、`X` ボタンが表示されることを確認する。
5. 背景タップでも詳細が閉じることを確認する。
6. 未発見妖精カードを押しても詳細が開かないことを確認する。
7. 一覧を閉じたあとに再度開き、前回の詳細モーダルが残っていないことを確認する。
8. ショップ画面を開いた状態から妖精一覧を開き、ショップが閉じることを確認する。

## 完了条件
- 発見済み妖精カードから詳細モーダルを開ける。
- 詳細モーダルは `X` ボタンと背景タップで閉じられる。
- 未発見妖精カードでは詳細モーダルが開かない。
- 詳細の配色がピンク基調で統一され、緑系の背景色が使われていない。
- 一覧の件数表示、スクロール、ショップとの排他表示に回帰がない。
