# 妖精コレクション未発見画像transparent表示 実装計画

## 実装方針
- 対象は [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) と [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) に限定し、妖精コレクション画面の構造や発見状態管理は変更しない。
- 未発見カードの画像欄は、従来の「CSS クラスで伏せ見た目を付けるだけ」の方式から、`Assets/GameResources/Texture/Resources/transparent.png` を読み込んで背景画像として設定する方式へ切り替える。
- `FairyDefinition` や `FairyCollectionService` には手を入れず、カード生成責務を持つ `HudScreenBinder.CreateFairyCard()` 内で発見済みと未発見の画像設定を分岐する。
- `transparent` 素材の取得失敗時は、現行の `fairy-card__image--undiscovered` クラスによる見た目を残してフォールバックできる構成にする。
- ユーザー確認済みの通り、未発見カード下部の補足文は現行実装の `クール/ワイルド` を維持する。

## 変更対象ファイル一覧
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - `transparent` 素材を保持する参照を追加する。
  - 妖精コレクション UI 初期化時、または初回利用時に `Resources.Load<Texture2D>("transparent")` で素材を取得する。
  - `CreateFairyCard()` の未発見分岐で `transparent` 素材を背景画像へ設定し、取得失敗時のみ既存クラスへフォールバックする。
- [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)
  - `fairy-card__image--undiscovered` をフォールバック用スタイルとして残す。
  - `transparent` 素材適用時に不要な色味が強く残らないよう、未発見カード画像欄の背景色・不透明度の役割を整理する。

## データフロー / 処理フロー
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の初期化処理で妖精コレクション UI を準備する。
2. 同クラス内で `transparent` 素材を `Resources` から取得し、後続のカード生成で再利用できるよう保持する。
3. `OpenFairyCollection()` 実行時に `RefreshFairyCollection()` が全妖精一覧を取得する。
4. 各妖精について `FairyCollectionService.IsDiscovered(fairy.Id)` を評価し、`CreateFairyCard(fairy, isDiscovered)` を呼ぶ。
5. 発見済みなら `fairy.Icon.texture` を画像欄に設定する。
6. 未発見なら保持済みの `transparent` 素材を画像欄に設定する。素材未取得時は `fairy-card__image--undiscovered` を付与して従来見た目に戻す。
7. 生成したカードを `fairyCollectionScrollView` に追加し、件数表示は既存どおり `GetDiscoveredCount(fairies)` で更新する。

## 詳細設計

### 1. `transparent` 素材の参照方法
- 既存プロジェクトには [transparent.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Texture/Resources/transparent.png) が `Resources` 配下に存在するため、コードからは `Resources.Load<Texture2D>("transparent")` で取得できる前提を採用する。
- 毎回カード生成時に `Resources.Load` を呼ぶ必要はないため、`HubScreenBinder` に `private Texture2D undiscoveredFairyImageTexture;` のようなキャッシュ用フィールドを追加する。
- 読み込みタイミングは `InitializeFairyCollectionUi()` の末尾、または専用のヘルパーメソッド経由とし、UI 初期化の責務範囲に収める。
- 読み込み失敗時は `null` のまま保持し、例外を出さずに既存クラスフォールバックへ流す。

### 2. `CreateFairyCard()` の分岐設計
- 現状の [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs#L658) では、発見済みだけ `fairy.Icon.texture` を背景画像へ設定し、それ以外は `fairy-card__image--undiscovered` クラスを付与している。
- これを以下の優先順位に変更する。
1. `isDiscovered && fairy != null && fairy.Icon != null` の場合は `fairy.Icon.texture` を設定する。
2. 未発見かつ `undiscoveredFairyImageTexture != null` の場合は `transparent` 素材を設定する。
3. どちらも満たさない場合のみ `fairy-card__image--undiscovered` を付与する。
- 未発見カードに発見済み画像の残像が出ないよう、未発見分岐では必ず明示的に背景画像を上書きする。
- 名前は従来どおり `？？？`、補足文は現行どおり `クール/ワイルド` を維持する。

### 3. USS の整理方針
- `transparent` 素材を直接表示する場合でも、画像フレーム自体のサイズ・余白・角丸は既存 `.fairy-card__image-frame` と `.fairy-card__image` を流用する。
- `.fairy-card__image--undiscovered` は削除せず、素材取得失敗時の後方互換用スタイルとして残す。
- 既存の未発見スタイルが強いグレー背景や高い不透明度を持っている場合、`transparent` 素材表示時の見た目を邪魔しないよう、クラス付与時だけ効く構成かを確認する。
- 実装では「`transparent` 素材を設定したが、同時に未発見クラスを付けて上書きしてしまう」状態を避ける。

### 4. 初期化と責務分離
- `InitializeFairyCollectionUi()` は現在、UI 要素取得と表示初期化のみを担当している。
- 今回はここに素材参照の準備を追加するが、処理が膨らみすぎる場合は `LoadUndiscoveredFairyImageTexture()` のような private メソッドへ切り出す。
- こうしておくと、後続で `transparent` 以外の未発見画像へ差し替える際も修正箇所が `HudScreenBinder` 内に閉じる。

## リスクと対策
- `Resources.Load` のパス指定を誤ると、未発見画像が表示されず静かに `null` になる。
  - 取得先を `transparent` の単一文字列に固定し、設計書と実装コメントに参照元パスを明記する。
- 未発見分岐で背景画像クリアが不十分だと、再利用された `VisualElement` に以前の画像が残る可能性がある。
  - カード生成は毎回新規 `VisualElement` を作っている現状を維持し、未発見時も必ず背景画像設定またはクラス付与のどちらかを実行する。
- `transparent` 素材が完全透明のため、画像欄が「何も出ていない」ように見える可能性がある。
  - 画像フレーム側の背景色と枠線はそのまま残し、未発見でも画像欄の存在が視認できる前提にする。
- USS 側の未発見クラスが強すぎると、将来の見た目調整で `transparent` 素材との責務が衝突する。
  - クラスの役割を「フォールバック専用」と定義し、素材読込成功時は付与しない実装に統一する。

## 検証方針
- 手動確認1: 妖精コレクションを開いたとき、発見済みカードは従来どおり各妖精アイコンが表示される。
- 手動確認2: 未発見カードは画像欄が空ではなく、`transparent` 素材適用状態で表示される。
- 手動確認3: 未発見カードでもカード枠、画像フレーム、名前 `？？？`、補足文 `クール/ワイルド` のレイアウトが崩れない。
- 手動確認4: 件数表示 `X/Y`、スクロール、クローズ操作、ショップとの排他表示に回帰がない。
- 手動確認5: `transparent` 素材を一時的に取得不能にしても、画面更新が停止せず、既存の未発見スタイルで描画される。

## コードスニペット
```csharp
private Texture2D undiscoveredFairyImageTexture;

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
    undiscoveredFairyImageTexture = Resources.Load<Texture2D>("transparent");

    fairyCollectionOverlay.style.display = DisplayStyle.None;
}
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
