# 妖精コレクション詳細画面 実装計画

## 実装方針
- 対象は既存の妖精コレクション画面を構成する [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)、[FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml)、[FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) を中心とし、新規画面を別管理するのではなく一覧画面内モーダルとして拡張する。
- 一覧カードは現状 `VisualElement` で動的生成しているため、詳細画面起動のクリック導線を追加するために、カード全体を `Button` ベースへ変更するか、クリック可能なラッパー要素を持たせる。
- 未発見妖精は要件どおり選択不可にするため、カード生成時点で `isDiscovered` に応じたクリック可否と見た目を分離する。
- 詳細画面に表示するデータは既存の `FairyDefinition.DisplayName`、`FairyDefinition.Icon`、固定文言の `クール/ワイルド`、固定フレーバーテキストを利用し、`FairyDefinition` の serialized field は増やさない。
- 配色は既存のピンク系 UI に統一し、添付画像の情報構造のみ参考にして緑系配色は採用しない。

## 変更対象ファイル一覧
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - 詳細画面用 UI 参照、開閉制御、選択中妖精の反映処理を追加する。
  - 一覧カード生成処理を、発見済みのみ選択可能な構造へ変更する。
  - 一覧画面を閉じる際に詳細画面も必ず閉じる制御を追加する。
- [Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml)
  - 既存一覧オーバーレイの中に、詳細モーダル用の背景オーバーレイ、パネル、タイトル、画像枠、詳細文言、閉じるボタンの要素を追加する。
- [Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss)
  - 詳細モーダルのレイアウトとピンク基調スタイルを追加する。
  - 一覧カードの選択可能状態、未発見の非活性状態、詳細モーダル表示中の視認性を調整する。

## データフロー / 処理フロー
1. ユーザーが HUD の `妖精` ボタンを押すと、`OpenFairyCollection()` が呼ばれ、ショップを閉じたうえで `RefreshFairyCollection()` を実行し、一覧オーバーレイを表示する。
2. `RefreshFairyCollection()` は `FairyCatalogSource.GetFairies()` で全妖精を取得し、各要素に対して `FairyCollectionService.IsDiscovered(fairy.Id)` を評価する。
3. `CreateFairyCard(fairy, isDiscovered)` はカードの見た目を組み立てると同時に、発見済みなら `OpenFairyDetail(fairy)` を呼ぶクリックイベントを登録する。未発見なら非活性スタイルのみを付与し、クリックイベントは登録しない。
4. `OpenFairyDetail(fairy)` は選択中妖精を検証し、詳細モーダルの各 UI 要素へ名前、画像、固定好きなシール文言、固定フレーバーテキストを設定して詳細オーバーレイを表示する。
5. 詳細表示中に `X` ボタンまたは背景オーバーレイが押されると `CloseFairyDetail()` を実行し、詳細モーダルだけを閉じて一覧表示へ戻す。
6. 一覧画面自体を `CloseFairyCollection()` で閉じる際は `CloseFairyDetail()` を先に呼び、詳細モーダルが残らない状態にしてから一覧オーバーレイを非表示にする。

## 詳細設計

### 1. UXML 構造拡張
- 既存の `fairy-collection-overlay` 配下に、一覧パネルとは別レイヤーで `fairy-detail-overlay` を追加する。
- `fairy-detail-overlay` は以下の子要素を持つ。
  - `fairy-detail-backdrop`
  - `fairy-detail-panel`
  - `fairy-detail-close-button`
  - `fairy-detail-name`
  - `fairy-detail-image-frame`
  - `fairy-detail-image`
  - `fairy-detail-favorite-label`
  - `fairy-detail-favorite-value`
  - `fairy-detail-flavor-label`
  - `fairy-detail-flavor-value`
- 詳細モーダルは一覧オーバーレイ内で absolute 配置し、一覧カード群より前面に表示する。

### 2. `HudScreenBinder` の状態管理
- 既存の妖精一覧用フィールド群に加えて、詳細モーダル参照を保持する private フィールドを追加する。
- 例:
  - `private VisualElement fairyDetailOverlay;`
  - `private Button fairyDetailBackdrop;`
  - `private VisualElement fairyDetailPanel;`
  - `private Button fairyDetailCloseButton;`
  - `private Label fairyDetailNameLabel;`
  - `private VisualElement fairyDetailImage;`
  - `private Label fairyDetailFavoriteValueLabel;`
  - `private Label fairyDetailFlavorValueLabel;`
- `InitializeFairyCollectionUi()` で上記要素を取得し、欠損時は既存と同様にエラーログを出して return する。
- `OnEnable()` / `OnDisable()` で詳細モーダルの閉じるイベント購読・解除を追加する。

### 3. 一覧カードのクリック構造
- 既存 `CreateFairyCard()` は `VisualElement` を返しているが、選択可能にするため `Button` を返す構造へ変更するのが最小差分である。
- `Button` のデフォルト見た目を抑えるため、USS 側でボタン由来の背景や境界線を打ち消し、現行カードと同じ外観を維持する。
- 発見済みカードには `clickable.clicked += () => OpenFairyDetail(fairy);` 相当の処理を付与する。
- 未発見カードには `SetEnabled(false)` は使わず、閉じる導線や見た目への副作用を避けるため、クリック未登録かつ専用クラス `fairy-card--locked` を付与する方針とする。
- ホバーや押下演出は強くしすぎず、既存 UI のトーンに合わせて軽微な変化に留める。

### 4. 詳細モーダル表示内容
- 名前は `fairy.DisplayName` を `fairy-detail-name` に設定する。
- 画像は `fairy.Icon.texture` があれば `fairy-detail-image` の背景画像に設定し、未設定なら背景画像をクリアして枠のみ表示する。
- 好きなシールは固定で `クール/ワイルド` を設定する。
- フレーバーテキストは固定文言を使用する。実装時は `private const string FairyDetailFlavorText = "ホゲホゲ。フガフガ あいうえお";` のように `HudScreenBinder` 内定数化しておくと追跡しやすい。
- 将来データ化する可能性を考え、詳細反映処理は `ApplyFairyDetail(FairyDefinition fairy)` のような専用メソッドへ切り出す。

### 5. 閉じる制御
- `CloseFairyDetail()` は `fairyDetailOverlay.style.display = DisplayStyle.None;` を担当し、null 安全にする。
- `CloseFairyCollection()` 冒頭で `CloseFairyDetail()` を呼ぶ。
- `OpenFairyCollection()` 側でも一覧表示前に `CloseFairyDetail()` を呼んでおくと、再オープン時の残留を避けられる。

### 6. USS 設計
- 既存一覧はピンク背景と淡い内枠で構成されているため、詳細モーダルも濃いピンクの外枠、淡いピンクの内側、白寄りの画像フレームで統一する。
- 添付画像の緑カード相当の面積は、ピンクからコーラル寄りの色へ置き換える。
- `fairy-detail-overlay` は全面 absolute、`fairy-detail-backdrop` は半透明の黒またはグレー、`fairy-detail-panel` は中央寄せ配置とする。
- `fairy-detail-close-button` はショップと同系統の白ベース角丸ボタンを流用しつつ、詳細パネル右上に重ねる。
- 一覧カードには以下のスタイルを追加する。
  - 選択可能カードのカーソル感と軽い押下表現
  - 未発見カード `fairy-card--locked` の視覚的非活性
  - ボタン化に伴う標準境界線、背景、padding の打ち消し

## リスクと対策
- `Button` 化により既存カードレイアウトが崩れる可能性がある。
  - USS で `background-color: rgba(0,0,0,0)`、border 幅 0、padding 0 を明示し、ボタン固有スタイルを打ち消す。
- 詳細モーダルが一覧パネルのスクロールや背面クリックを貫通する可能性がある。
  - 詳細モーダル専用 backdrop を一覧オーバーレイ全面に敷き、前面レイヤーでクリックを吸収する。
- 未発見カードを無効化する実装方法次第で見た目やイベント伝播が壊れる可能性がある。
  - `SetEnabled(false)` に依存せず、クリック未登録の通常要素として扱い、見た目のみ専用クラスで制御する。
- 詳細文言が固定値のため、後続でデータ化する際に実装箇所が散る可能性がある。
  - 反映ロジックを `ApplyFairyDetail()` にまとめ、定数も 1 箇所へ寄せる。

## 検証方針
- 手動確認1: 発見済み妖精カードを押すと詳細モーダルが開く。
- 手動確認2: 未発見妖精カードを押しても詳細モーダルが開かない。
- 手動確認3: 詳細モーダルに妖精名、画像、`クール/ワイルド`、固定フレーバーテキスト、閉じるボタンが表示される。
- 手動確認4: `X` ボタンと背景タップの両方で詳細モーダルが閉じる。
- 手動確認5: 詳細モーダルを開いた状態で一覧画面を閉じても、再表示時に詳細モーダルが残らない。
- 手動確認6: ショップとの排他表示、件数表示 `X/Y`、一覧スクロールに回帰がない。
- 手動確認7: 1920x1080 基準で、詳細モーダルの配色がピンク基調に統一され、緑系の面が残っていない。

## コードスニペット
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

    fairyDetailImage.style.backgroundImage = fairy != null && fairy.Icon != null
        ? new StyleBackground(fairy.Icon.texture)
        : StyleKeyword.None;
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

    // 既存の name / image / detail 組み立ては継続
    return card;
}
```
