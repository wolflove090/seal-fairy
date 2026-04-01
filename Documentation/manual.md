# 妖精コレクション未発見画像transparent表示 作業手順書

## 目的
- 妖精コレクションにおいて、未発見妖精の画像欄へ `transparent` 素材を表示する。
- 発見済み妖精の画像表示、件数表示 `X/Y`、開閉導線、ショップとの排他表示は維持する。
- 未発見カードの名前は `？？？`、補足文は現行どおり `クール/ワイルド` を維持する。

## 変更対象
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)
- 参照素材: [Assets/GameResources/Texture/Resources/transparent.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Texture/Resources/transparent.png)

## 手順1: 未発見画像用テクスチャの保持フィールドを追加する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. 妖精コレクション関連の private フィールド群の近くに、未発見画像キャッシュ用フィールドを追加する。
3. 型は `Texture2D` を使う。

### 追加例
```csharp
private Texture2D undiscoveredFairyImageTexture;
```

## 手順2: 妖精コレクション UI 初期化時に `transparent` 素材を読み込む
1. 同ファイルの `InitializeFairyCollectionUi(VisualElement root)` を探す。
2. 既存の UI 要素取得処理はそのまま残す。
3. `fairyCollectionCloseButton` 取得後、または UI 初期化完了直前で `Resources.Load<Texture2D>("transparent")` を呼び、`undiscoveredFairyImageTexture` に代入する。
4. パスは `Resources` ルールに従い、拡張子なしの `"transparent"` を使う。
5. 読込失敗時でも例外を投げず、そのまま進める。

### 変更例
```csharp
private void InitializeFairyCollectionUi(VisualElement root)
{
    if (root == null || fairyCollectionScreenAsset == null)
    {
        return;
    }

    fairyCollectionOverlay = root.Q<VisualElement>("fairy-collection-overlay");
    if (fairyCollectionOverlay == null)
    {
        fairyCollectionScreenAsset.CloneTree(root);
        fairyCollectionOverlay = root.Q<VisualElement>("fairy-collection-overlay");
    }

    fairyCollectionBackdrop = root.Q<Button>("fairy-collection-backdrop");
    fairyCollectionPanel = root.Q<VisualElement>("fairy-collection-panel");
    fairyCollectionScrollView = root.Q<ScrollView>("fairy-collection-scroll-view");
    fairyCollectionEmptyLabel = root.Q<Label>("fairy-collection-empty-label");
    fairyCollectionCountLabel = root.Q<Label>("fairy-collection-count-label");
    fairyCollectionCloseButton = root.Q<Button>("fairy-collection-close-button");

    if (fairyCollectionOverlay == null ||
        fairyCollectionBackdrop == null ||
        fairyCollectionPanel == null ||
        fairyCollectionScrollView == null ||
        fairyCollectionEmptyLabel == null ||
        fairyCollectionCountLabel == null ||
        fairyCollectionCloseButton == null)
    {
        Debug.LogError("妖精コレクション UI の初期化に失敗しました");
        return;
    }

    undiscoveredFairyImageTexture = Resources.Load<Texture2D>("transparent");
    fairyCollectionOverlay.style.display = DisplayStyle.None;
}
```

## 手順3: `CreateFairyCard()` の未発見画像分岐を差し替える
1. 同ファイルの `CreateFairyCard(FairyDefinition fairy, bool isDiscovered)` を探す。
2. 発見済みのときは現状どおり `fairy.Icon.texture` を表示する。
3. 未発見のときは、`undiscoveredFairyImageTexture` があればそれを `StyleBackground` で設定する。
4. `undiscoveredFairyImageTexture` が `null` のときだけ、既存の `.fairy-card__image--undiscovered` クラスを付与する。
5. 未発見カードの `nameLabel.text` は `？？？` を維持する。
6. `detailValue.text` は発見済み・未発見ともに `クール/ワイルド` のままにする。

### 変更例
```csharp
private VisualElement CreateFairyCard(FairyDefinition fairy, bool isDiscovered)
{
    VisualElement card = new();
    card.AddToClassList("fairy-card");

    Label nameLabel = new();
    nameLabel.AddToClassList("fairy-card__name");
    nameLabel.text = isDiscovered && fairy != null ? fairy.DisplayName : "？？？";

    VisualElement imageFrame = new();
    imageFrame.AddToClassList("fairy-card__image-frame");

    VisualElement image = new();
    image.AddToClassList("fairy-card__image");

    if (isDiscovered && fairy != null && fairy.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(fairy.Icon.texture);
    }
    else if (undiscoveredFairyImageTexture != null)
    {
        image.style.backgroundImage = new StyleBackground(undiscoveredFairyImageTexture);
    }
    else
    {
        image.AddToClassList("fairy-card__image--undiscovered");
    }

    Label detailLabel = new();
    detailLabel.AddToClassList("fairy-card__detail-label");
    detailLabel.text = "好きなシール：";

    Label detailValue = new();
    detailValue.AddToClassList("fairy-card__detail-value");
    detailValue.text = "クール/ワイルド";

    imageFrame.Add(image);
    card.Add(nameLabel);
    card.Add(imageFrame);
    card.Add(detailLabel);
    card.Add(detailValue);
    return card;
}
```

## 手順4: USS のフォールバックスタイルを確認する
1. [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) を開く。
2. `.fairy-card__image--undiscovered` が存在することを確認する。
3. このクラスは `transparent` 素材読込失敗時のみ使う前提なので、残してよい。
4. ただし、通常の `.fairy-card__image` に対して強い `opacity` や灰色背景を直接指定している場合は、`transparent` 素材表示を邪魔しないか確認する。
5. 今回必須なのはフォールバック維持であり、`transparent` 素材が読めているケースではこのクラスを付けないことを前提にする。

### 確認対象の例
```css
.fairy-card__image--undiscovered {
    opacity: 0.35;
    background-color: rgba(255, 255, 255, 0.45);
}
```

## 手順5: 動作確認を行う
1. Unity でゲームを起動する。
2. HUD の `妖精` ボタンから妖精コレクション画面を開く。
3. 発見済み妖精カードが従来どおり個別アイコンを表示することを確認する。
4. 未発見妖精カードの画像欄に `transparent` 素材が設定されていることを確認する。
5. 未発見妖精カードで名前が `？？？`、補足文が `クール/ワイルド` のままであることを確認する。
6. 件数表示 `X/Y`、スクロール、`X` ボタン、背景押下でのクローズが従来どおり動くことを確認する。
7. ショップ画面を開いた状態から妖精コレクションを開き、ショップが閉じることを確認する。

## 手順6: フォールバック確認を行う
1. `Resources.Load<Texture2D>("transparent")` が `null` になった場合を想定して、一時的に読込名を変更するか、デバッグで `undiscoveredFairyImageTexture == null` の状態を作る。
2. その状態で妖精コレクションを開き、未発見カードが `.fairy-card__image--undiscovered` による見た目で表示されることを確認する。
3. コレクション画面更新時に例外停止や UI 崩れが発生しないことを確認する。
4. 確認後は `Resources.Load<Texture2D>("transparent")` を元に戻す。

## 完了条件
- 未発見妖精カードの画像欄が `transparent` 素材で表示される。
- 発見済み妖精カードの画像表示に回帰がない。
- 未発見カードの `？？？` と `クール/ワイルド` 表示が維持される。
- `transparent` 素材取得失敗時でも、未発見カードがフォールバック表示され、エラー停止しない。
- 妖精コレクションの開閉、件数表示、スクロール、ショップとの排他表示に回帰がない。
