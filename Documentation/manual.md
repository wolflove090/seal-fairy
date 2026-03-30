# 妖精コレクション画面ブラッシュアップ 作業手順書

## 目的
- 既存の妖精コレクション画面を、添付画像に近い「右側固定パネル + 大型タイトル + 3 列カードグリッド」へ更新する。
- 既存の `FairyCatalogSource`、`FairyCollectionService`、`HubScreenBinder` の開閉導線は維持する。
- 発見数は右下表示、補足文言は `クール/ワイルド`、未発見名は `？？？` へ揃える。

## 変更対象
- [Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml)
- [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- 参照元: [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)

## 手順1: UXML を右側固定パネル構造へ組み替える
1. [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml) を開く。
2. 既存の `fairy-collection-overlay` と `fairy-collection-backdrop` は維持しつつ、`fairy-collection-panel` の子構造をヘッダー、コンテンツフレーム、フッターに分ける。
3. `fairy-collection-title` は `妖精コレクション` に変更する。
4. `fairy-collection-close-button` はヘッダー右上に残し、テキストは `X` にする。
5. `fairy-collection-count-label` はフッター右下に置く。

### 変更例
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss?fileID=7433441132597879392&amp;guid=a0288d6b5e9eb4e699aea55b9fc358e4&amp;type=3#FairyCollectionScreen"/>
    <ui:VisualElement name="fairy-collection-overlay">
        <ui:Button name="fairy-collection-backdrop" />
        <ui:VisualElement name="fairy-collection-panel">
            <ui:VisualElement name="fairy-collection-header">
                <ui:Label name="fairy-collection-title" text="妖精コレクション" />
                <ui:Button name="fairy-collection-close-button" text="X" />
            </ui:VisualElement>
            <ui:VisualElement name="fairy-collection-content-frame">
                <ui:Label name="fairy-collection-empty-label" text="妖精が登録されていません" />
                <ui:ScrollView name="fairy-collection-scroll-view" mode="Vertical" />
                <ui:VisualElement name="fairy-collection-footer">
                    <ui:Label name="fairy-collection-count-label" text="0/0" />
                </ui:VisualElement>
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

## 手順2: パネルとカードの USS を全面更新する
1. [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) を開く。
2. `#fairy-collection-backdrop` は全面を覆うまま、見た目は透明寄りにして操作遮断専用にする。
3. `#fairy-collection-panel` を右寄せ固定、濃いピンク背景、上下いっぱいの大型パネルへ変更する。
4. `#fairy-collection-content-frame` に淡いピンクの内側フレーム、角丸、余白を設定する。
5. `#fairy-collection-close-button` は [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) の `#sticker-shop-close-button` を参考に、白ベースの立体ボタンへ寄せる。
6. `#fairy-collection-scroll-view .unity-scroll-view__content-container` は `flex-direction: row; flex-wrap: wrap;` を維持する。
7. `.fairy-card`、`.fairy-card__image-frame`、`.fairy-card__image`、`.fairy-card__detail-label`、`.fairy-card__detail-value` を追加し、3 列で詰まりすぎないサイズへ調整する。

### 変更例
```css
#fairy-collection-panel {
    position: absolute;
    top: 0;
    right: 0;
    width: 1018px;
    bottom: 0;
    padding-top: 18px;
    padding-left: 22px;
    padding-right: 22px;
    padding-bottom: 26px;
    background-color: rgb(255, 92, 122);
}

#fairy-collection-content-frame {
    flex-grow: 1;
    padding: 22px;
    background-color: rgb(247, 206, 218);
    border-top-left-radius: 18px;
    border-top-right-radius: 18px;
    border-bottom-left-radius: 18px;
    border-bottom-right-radius: 18px;
}

.fairy-card {
    width: 292px;
    min-height: 380px;
    margin-right: 26px;
    margin-bottom: 24px;
    padding: 18px;
    background-color: rgb(195, 216, 38);
    border-top-left-radius: 18px;
    border-top-right-radius: 18px;
    border-bottom-left-radius: 18px;
    border-bottom-right-radius: 18px;
}
```

## 手順3: `HudScreenBinder.cs` の一覧更新文言を調整する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. `RefreshFairyCollection()` を探し、空表示時と通常時の `fairyCollectionCountLabel.text` を新仕様へ変える。
3. 発見数ラベルは右下表示に合わせて短い文言へ整理する。`0/0` か `発見数 0/0` のどちらかに揃える。
4. 空表示時は `fairyCollectionEmptyLabel.style.display = DisplayStyle.Flex;` を残し、通常時は `DisplayStyle.None;` にする。

### 変更例
```csharp
private void RefreshFairyCollection()
{
    if (fairyCollectionScrollView == null || fairyCollectionEmptyLabel == null || fairyCollectionCountLabel == null)
    {
        return;
    }

    IReadOnlyList<FairyDefinition> fairies = fairyCatalogSource != null ? fairyCatalogSource.GetFairies() : null;
    fairyCollectionScrollView.Clear();

    if (fairies == null || fairies.Count == 0)
    {
        fairyCollectionEmptyLabel.style.display = DisplayStyle.Flex;
        fairyCollectionCountLabel.text = "0/0";
        return;
    }

    fairyCollectionEmptyLabel.style.display = DisplayStyle.None;

    foreach (FairyDefinition fairy in fairies)
    {
        bool isDiscovered = fairy != null && FairyCollectionService.IsDiscovered(fairy.Id);
        fairyCollectionScrollView.Add(CreateFairyCard(fairy, isDiscovered));
    }

    fairyCollectionCountLabel.text = $"{FairyCollectionService.GetDiscoveredCount(fairies)}/{fairies.Count}";
}
```

## 手順4: `CreateFairyCard()` を画像付き 4 ブロック構造へ変更する
1. 同じく [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateFairyCard(FairyDefinition fairy, bool isDiscovered)` を探す。
2. 名前、画像、補足文を直接積む構造から、画像フレームと補足値を分離した構造へ組み替える。
3. 発見済みなら名前は `fairy.DisplayName`、補足値は `クール/ワイルド` を表示する。
4. 未発見なら名前は `？？？`、補足値は `未発見` を表示し、画像には未発見用クラスを付与する。
5. `fairy.Icon` が null でも例外にならないよう、背景画像設定前に null を確認する。

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
    else
    {
        image.AddToClassList("fairy-card__image--undiscovered");
    }

    Label detailLabel = new();
    detailLabel.AddToClassList("fairy-card__detail-label");
    detailLabel.text = "好きなシール：";

    Label detailValue = new();
    detailValue.AddToClassList("fairy-card__detail-value");
    detailValue.text = isDiscovered ? "クール/ワイルド" : "未発見";

    imageFrame.Add(image);
    card.Add(nameLabel);
    card.Add(imageFrame);
    card.Add(detailLabel);
    card.Add(detailValue);
    return card;
}
```

## 手順5: 未発見見た目を USS で定義する
1. [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) で `.fairy-card__image--undiscovered` を定義する。
2. 半透明、淡いグレー背景、必要なら `opacity` を使って未発見感を出す。
3. 名前や値の色も未発見時に弱めたい場合は、`card.EnableInClassList("fairy-card--undiscovered", !isDiscovered);` を追加して親クラスで切り替える。
4. その場合は C# 側で `fairy-card--undiscovered` の付与も忘れずに行う。

### 変更例
```css
.fairy-card__image--undiscovered {
    opacity: 0.35;
    background-color: rgba(255, 255, 255, 0.45);
}

.fairy-card--undiscovered .fairy-card__name,
.fairy-card--undiscovered .fairy-card__detail-value {
    color: rgb(255, 255, 255);
}
```

## 手順6: 動作確認を行う
1. Unity でゲームを起動し、HUD 右下の `妖精` ボタンから画面を開く。
2. 右側に大型ピンクパネル、上部タイトル `妖精コレクション`、右上 `X` が見えることを確認する。
3. 左側の所持金パネルとシール一覧が見えたまま、背面操作はできないことを確認する。
4. カードが 1920x1080 基準で 3 列表示されることを確認する。
5. 発見済みカードが画像と `クール/ワイルド` を表示し、未発見カードが `？？？` と `未発見` を表示することを確認する。
6. 右下に `X/Y` が出ること、データ 0 件時は `0/0` になることを確認する。
7. ショップ画面を開いた状態から妖精画面を開き、ショップが閉じることを確認する。
8. `X` ボタンと背景押下の両方で閉じられることを確認する。

## 完了条件
- 妖精コレクション画面が添付画像に近い右側固定パネル構成へ更新されている。
- 3 列カード、右上 `X`、右下 `X/Y`、`クール/ワイルド`、`？？？` が要件どおり反映されている。
- `HudScreenBinder.cs` の既存の開閉導線とショップ排他制御に回帰がない。
