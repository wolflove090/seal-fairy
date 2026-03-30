# 妖精コレクション画面ブラッシュアップ 実装計画

## 実装方針
- 対象は [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml)、[FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)、[HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に限定し、妖精発見状態の保持ロジックやカタログデータ構造は変更しない。
- 既存の開閉導線 `OpenFairyCollection()` / `CloseFairyCollection()` / `RefreshFairyCollection()` を維持し、右側固定パネルの見た目とカード構造だけを差し替える。
- 画面全体はオーバーレイのままにしつつ、左側 HUD を視認可能にするため、全面グレー幕ではなく透明な遮断レイヤーと右側大型パネルの組み合わせへ変更する。
- カードは 3 列グリッドを成立させるため、UXML 側はスクロールビューとヘッダー/フッターの骨組みを定義し、カード内部は `HudScreenBinder` で動的生成する。
- 文言仕様は要件書に合わせて固定し、発見数は右下、補足文言 2 行目は `クール/ワイルド`、未発見名は `？？？` を採用する。

## 変更対象ファイル一覧
- [Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml)
  - タイトル帯、右上 `X` ボタン、内側フレーム、スクロール領域、右下発見数ラベルを含む新レイアウトへ更新する。
  - 背面操作を遮断するための全面ボタンは残しつつ、見た目は透明に寄せる。
- [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)
  - 右側固定ピンクパネル、内側淡ピンクフレーム、白い閉じるボタン、3 列カードグリッド、黄緑カードの見た目を定義する。
  - 未発見カードの半透明・伏せ表示、右下発見数ラベル、空表示の見た目も定義する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - 新しい UXML 構造に合わせた要素取得名を維持または最小変更で追従する。
  - `CreateFairyCard()` を画像フレームと 2 行補足文付きカードへ組み替える。
  - 発見数表示文言と空表示時の見せ方を新レイアウトに合わせて更新する。

## データフロー / 処理フロー
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `OnEnable()` で `fairy-collection-overlay` 配下の要素を取得する。
2. `fairy-button.clicked` で `OpenFairyCollection()` が呼ばれ、先に `CloseStickerShop()` を実行して排他表示を保証する。
3. `RefreshFairyCollection()` が `FairyCatalogSource.GetFairies()` で全妖精一覧を取得する。
4. 各妖精について `FairyCollectionService.IsDiscovered(fairy.Id)` を評価し、`CreateFairyCard(fairy, isDiscovered)` でカードを動的生成する。
5. 生成したカードを `fairyCollectionScrollView` に追加し、最後に `FairyCollectionService.GetDiscoveredCount(fairies)` で右下の発見数表示を更新する。
6. `fairyCollectionCloseButton.clicked` または透明な背面ボタン押下で `CloseFairyCollection()` を呼び、オーバーレイを非表示に戻す。

## 処理フロー詳細

### 1. UXML の再構成
- 現状の [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml#L1) は、タイトル、空表示、スクロール、フッターだけの単純構造で、画像のヘッダー帯や右上クローズの位置関係を表現しづらい。
- そのため、オーバーレイ直下に透明な `fairy-collection-backdrop` を置き、その上に `fairy-collection-panel` を右寄せ配置する。
- `fairy-collection-panel` の中は `fairy-collection-header`、`fairy-collection-content-frame`、`fairy-collection-footer` に分割する。
- `fairy-collection-count-label` は要件に合わせてフッター右下へ固定し、`fairy-collection-close-button` はヘッダー右上へ移動する。

### 2. USS のブラッシュアップ
- 現状の [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss#L1) は灰色の背面幕と単色の右パネルのみで、添付画像の二重フレームやカード密度を再現できていない。
- `#fairy-collection-overlay` は全面を覆うが、`#fairy-collection-backdrop` は透明に近い見た目にして操作遮断専用にする。
- `#fairy-collection-panel` は濃いピンク、`#fairy-collection-content-frame` は淡いピンク、`#fairy-collection-close-button` は既存ショップ閉じるボタンに近い白プレート表現へ寄せる。
- スクロールコンテナは `flex-wrap` のまま使い、カード幅と右余白を調整して 1920x1080 で 3 列になる基準値を作る。
- カードは `fairy-card`、`fairy-card__image-frame`、`fairy-card__image`、`fairy-card__name`、`fairy-card__detail-label`、`fairy-card__detail-value` などへ分割する。

### 3. カード生成ロジックの更新
- 現状の [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs#L657) 付近の `CreateFairyCard()` は、カード直下に名前、画像、2 行テキストをそのまま積んでおり、画像のフレームや下部補足文の分離がない。
- ここを「カード本体 > 名前 > 画像フレーム > 画像 > 補足ラベル > 補足値」の構造へ変更する。
- 発見済みなら `fairy.DisplayName` と `fairy.Icon` を表示し、未発見なら名前を `？？？`、補足値を `未発見`、画像を半透明クラス付きの空フレームとして描画する。
- `FairyDefinition.Icon` が未設定でも、画像フレーム自体は表示して例外を避ける。

### 4. カウント表示と空表示
- 既存実装では `fairyCollectionCountLabel.text = "発見した数: X/Y"` としていたが、要件書では右下表示だけを必須にしている。
- 文言は `X/Y` のみ、または `発見数 X/Y` の短い形式に整理し、右下に収まるよう CSS を設計する。
- 妖精 0 件時は空ラベルを表示しつつ、発見数は `0/0` を出す。
- データがある場合は空ラベルを非表示にし、スクロールビューのみを見せる。

### 5. 既存ロジックとの整合確認
- `InitializeFairyCollectionUi()` は既存の要素名を参照しているため、`fairy-collection-overlay`、`fairy-collection-backdrop`、`fairy-collection-scroll-view`、`fairy-collection-empty-label`、`fairy-collection-count-label`、`fairy-collection-close-button` は維持する。
- 要素をラップする中間 `VisualElement` を増やしても、上記の `name` が変わらなければ C# 側の取得コードは大きく変えずに済む。
- ショップとの排他制御はすでに `OpenFairyCollection()` 内にあるため、今回は崩さない。

## リスクと対策
- 3 列前提でカードを大型化しすぎると、スクロール領域で 2 列に落ちて画像イメージとの差が大きくなる。
  - パネル幅、左右パディング、カード幅、カード間余白をセットで調整し、1920x1080 基準で 3 列が成立する数値を優先する。
- `fairy-collection-backdrop` を完全透明にすると、押下可能領域の存在に気づきにくい一方、見た目は画像に近い。
  - 閉じる主導線は右上 `X` とし、背景押下閉じは補助導線として維持する。
- 未発見画像を空フレームだけにすると発見済みとの差が大きくなりすぎる可能性がある。
  - フレームは同じにし、画像エリアへ半透明オーバーレイまたは薄いダミー背景色を残して未発見状態を表現する。
- 発見数ラベルを右下へ移した際に、スクロールバーやカード最下段と重なる可能性がある。
  - フッター領域を独立させ、スクロールビューとは別のレイアウト領域で表示する。

## 検証方針
- 手動確認1: `妖精` ボタン押下で右側固定の大型パネルが開き、左側 HUD は見えたままになる。
- 手動確認2: 右上に白ベースの `X` ボタンが表示され、押下で閉じられる。
- 手動確認3: 1920x1080 基準でカードが 3 列に並び、件数超過時は縦スクロールで全件見られる。
- 手動確認4: 発見済み妖精は名前・画像・`クール/ワイルド` が表示される。
- 手動確認5: 未発見妖精は `？？？` と `未発見` が表示され、同一レイアウトのまま区別できる。
- 手動確認6: 右下の発見数表示が `X/Y` で更新される。
- 手動確認7: 妖精画面表示中にショップ画面は同時表示されず、閉じたあと HUD レイアウトが崩れない。

## コードスニペット
```xml
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
```

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
