# 妖精コレクション画面 作業手順書

## 目的
- HUD の `妖精` ボタンから開く妖精コレクション画面を追加する。
- 登録済み全妖精をスクロール一覧で表示し、未獲得妖精は `*****`、`未発見`、画像グレーアウトで表現する。
- コレクション画面は半透明グレーの全画面背景で背面入力を遮断し、閉じるボタンと背景押下で閉じられるようにする。

## 変更対象
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/Scripts/Fairy/FairyCollectionService.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCollectionService.cs)
- [Assets/UI/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml)
- [Assets/UI/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss)
- [Assets/UI/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/FairyCollectionScreen.uxml)
- [Assets/UI/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/FairyCollectionScreen.uss)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: コレクション用 UXML を作成
1. [Assets/UI/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/FairyCollectionScreen.uxml) を新規作成する。
2. ルートは全画面オーバーレイ用 `VisualElement` にする。
3. その直下に以下の要素を置く。
- `fairy-collection-backdrop`
- `fairy-collection-panel`
- `fairy-collection-title`
- `fairy-collection-scroll-view`
- `fairy-collection-empty-label`
- `fairy-collection-close-button`
- `fairy-collection-count-label`
4. 背景はパネルより背面、ただし HUD より前面になるように構成する。
5. `fairy-collection-scroll-view` は縦スクロール、内容は wrap 配置前提にする。

### UXML 例
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/UI/USS/FairyCollectionScreen.uss?fileID=7433441132597879392&amp;guid=REPLACE_ME&amp;type=3#FairyCollectionScreen"/>
    <ui:VisualElement name="fairy-collection-overlay">
        <ui:Button name="fairy-collection-backdrop" />
        <ui:VisualElement name="fairy-collection-panel">
            <ui:Label name="fairy-collection-title" text="妖精一覧" />
            <ui:Label name="fairy-collection-empty-label" text="妖精が登録されていません" />
            <ui:ScrollView name="fairy-collection-scroll-view" mode="Vertical" />
            <ui:VisualElement name="fairy-collection-footer">
                <ui:Button name="fairy-collection-close-button" text="閉じる" />
                <ui:Label name="fairy-collection-count-label" text="発見した数: 0/0" />
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

## 手順2: コレクション用 USS を作成
1. [Assets/UI/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/FairyCollectionScreen.uss) を新規作成する。
2. `fairy-collection-overlay` は全画面ストレッチ、初期状態は非表示にする。
3. `fairy-collection-backdrop` は半透明グレーで全画面を覆う。
4. `fairy-collection-panel` はワイヤーに合わせて画面右側に固定する。
5. スクロールコンテンツは 2 列カードレイアウトになるようにする。
6. 未獲得画像用 class `fairy-card__image--undiscovered` を用意し、グレースケール相当の見た目にする。

### USS 例
```css
#fairy-collection-overlay {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    display: none;
}

#fairy-collection-backdrop {
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    background-color: rgba(128, 128, 128, 0.55);
    border-left-width: 0;
    border-right-width: 0;
    border-top-width: 0;
    border-bottom-width: 0;
}

#fairy-collection-panel {
    position: absolute;
    top: 0;
    right: 0;
    width: 830px;
    bottom: 0;
    padding: 32px;
    background-color: rgb(217, 217, 217);
}

#fairy-collection-scroll-view .unity-scroll-view__content-container {
    flex-direction: row;
    flex-wrap: wrap;
    align-content: flex-start;
}
```

## 手順3: FairyCollectionService に参照 API を追加
1. [Assets/Scripts/Fairy/FairyCollectionService.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCollectionService.cs) を開く。
2. `TryRegisterDiscovery` は維持したまま、以下を追加する。

```csharp
public static bool IsDiscovered(string fairyId)
{
    return state.Contains(fairyId);
}

public static int GetDiscoveredCount(IReadOnlyList<FairyDefinition> fairies)
{
    if (fairies == null)
    {
        return 0;
    }

    int count = 0;
    foreach (FairyDefinition fairy in fairies)
    {
        if (fairy != null && state.Contains(fairy.Id))
        {
            count++;
        }
    }

    return count;
}
```

3. UI 側は `HashSet` を直接触らず、このサービス経由で状態取得する。

## 手順4: HubScreenBinder にコレクション表示制御を追加
1. [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に以下の SerializeField を追加する。

```csharp
[SerializeField] private FairyCatalogSource fairyCatalogSource;
[SerializeField] private VisualTreeAsset fairyCollectionScreenAsset;
```

2. 取得済みの `rootVisualElement` に対して `fairyCollectionScreenAsset.CloneTree()` を追加し、オーバーレイ要素参照を確保する。
3. `fairy-button` の参照を取り、クリックで `OpenFairyCollection()` を呼ぶ。
4. `fairy-collection-close-button` と `fairy-collection-backdrop` に `CloseFairyCollection()` を接続する。
5. `OnDisable` で各クリックイベントを解除する。

## 手順5: 一覧構築処理を実装
1. `HubScreenBinder` に `RefreshFairyCollection()` を追加する。
2. `fairyCatalogSource.GetFairies()` で全妖精を取得する。
3. 件数 0 の場合は空表示ラベルを出し、`発見した数: 0/0` をセットする。
4. 妖精がある場合は `ScrollView.Clear()` のうえでカードを 1 件ずつ追加する。
5. 獲得済み判定は `FairyCollectionService.IsDiscovered(fairy.Id)` を使う。

### 一覧更新例
```csharp
private void OpenFairyCollection()
{
    RefreshFairyCollection();
    fairyCollectionOverlay.style.display = DisplayStyle.Flex;
}

private void RefreshFairyCollection()
{
    IReadOnlyList<FairyDefinition> fairies = fairyCatalogSource != null
        ? fairyCatalogSource.GetFairies()
        : null;

    fairyCollectionScrollView.Clear();

    if (fairies == null || fairies.Count == 0)
    {
        fairyCollectionEmptyLabel.style.display = DisplayStyle.Flex;
        fairyCollectionCountLabel.text = "発見した数: 0/0";
        return;
    }

    fairyCollectionEmptyLabel.style.display = DisplayStyle.None;

    foreach (FairyDefinition fairy in fairies)
    {
        bool isDiscovered = fairy != null && FairyCollectionService.IsDiscovered(fairy.Id);
        fairyCollectionScrollView.Add(CreateFairyCard(fairy, isDiscovered));
    }

    fairyCollectionCountLabel.text =
        $"発見した数: {FairyCollectionService.GetDiscoveredCount(fairies)}/{fairies.Count}";
}
```

## 手順6: カード生成処理を実装
1. `CreateFairyCard(FairyDefinition fairy, bool isDiscovered)` を `HubScreenBinder` に追加する。
2. 獲得済み時は以下を表示する。
- 名前: `fairy.DisplayName`
- 画像: `fairy.Icon`
- 補足: `好きなシール: ポップ・小さい`
3. 未獲得時は以下を表示する。
- 名前: `*****`
- 画像: 背景画像ありでもなしでも、未獲得用 class を付与してグレーアウト
- 補足: `未発見`
4. パネル本体押下で閉じないよう、カードやパネル側でイベントを背景へ流さない構成にする。

### カード生成例
```csharp
private VisualElement CreateFairyCard(FairyDefinition fairy, bool isDiscovered)
{
    VisualElement card = new();
    card.AddToClassList("fairy-card");

    Label nameLabel = new();
    nameLabel.AddToClassList("fairy-card__name");
    nameLabel.text = isDiscovered && fairy != null ? fairy.DisplayName : "*****";

    VisualElement image = new();
    image.AddToClassList("fairy-card__image");
    if (!isDiscovered)
    {
        image.AddToClassList("fairy-card__image--undiscovered");
    }
    else if (fairy != null && fairy.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(fairy.Icon.texture);
    }

    Label detailLabel = new();
    detailLabel.AddToClassList("fairy-card__detail");
    detailLabel.text = isDiscovered ? "好きなシール: ポップ・小さい" : "未発見";

    card.Add(nameLabel);
    card.Add(image);
    card.Add(detailLabel);
    return card;
}
```

## 手順7: 既存 HUD との共存を調整
1. [Assets/UI/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml) の `fairy-button` はそのまま使う。
2. 既存 HUD UXML に大きな追加はせず、オーバーレイは binder から差し込む。
3. [Assets/UI/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss) に必要なら `fairy-button` の見た目調整だけ入れる。

## 手順8: シーン参照を設定
1. [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
2. `HubScreenBinder` の Inspector で `fairyCatalogSource` に既存の `FairyCatalogSource` を割り当てる。
3. `fairyCollectionScreenAsset` に [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/FairyCollectionScreen.uxml) を割り当てる。
4. `UIDocument` の参照が既存どおり接続されていることを確認する。

## 手順9: 動作確認
1. `妖精` ボタンでコレクション画面が開くことを確認する。
2. 背景が半透明グレーで全画面を覆うことを確認する。
3. `閉じる` ボタンと背景押下の両方で閉じることを確認する。
4. コレクション表示中に背面 HUD やゲーム入力が反応しないことを確認する。
5. 獲得済み妖精の名前、画像、固定文言が表示されることを確認する。
6. 未獲得妖精が `*****`、`未発見`、画像グレーアウトで表示されることを確認する。
7. 一覧が複数件でスクロール動作することを確認する。
8. 妖精 0 件時に空表示と `発見した数: 0/0` が出ることを確認する。
