# 妖精コレクション画面 実装計画

## 実装方針
- 既存 HUD は [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml) と [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss) と [HubScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) で構成されているため、HUD 本体は維持しつつ、妖精コレクション部分だけを別 UXML / USS に分離する。
- 新規 UXML は HUD と同じ `UIDocument` 配下へ `VisualTreeAsset` として差し込み、表示制御は既存 `HubScreenBinder` に集約する。これによりシーン上の `UIDocument` 追加やバインダ増殖を避け、既存フェーズ UI との接続点を 1 箇所に保つ。
- 妖精一覧のデータソースは [FairyCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogSource.cs) の全件一覧と [FairyCollectionService.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCollectionService.cs) の獲得状態を組み合わせる。
- 未獲得表示は「画像部分グレーアウト」「名前は `*****`」「補足文言は `未発見`」で固定し、好きなシール文言は獲得済み時のみ固定文言 `好きなシール: ポップ・小さい` を表示する。
- コレクション画面表示中は全画面背景オーバーレイで背面入力を遮断し、閉じるボタンと背景押下の両方で閉じられるようにする。

## 変更対象ファイル一覧

### 更新予定
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - `fairy-button` の押下ハンドラを追加する。
  - 新規コレクション UXML を `rootVisualElement` に差し込み、表示 / 非表示を管理する。
  - 妖精一覧カード生成、発見数表示、背景押下で閉じる処理を追加する。
  - オーバーレイ表示中のフェーズ操作や既存 HUD ボタンとの競合を防ぐ。
- [Assets/Scripts/Fairy/FairyCollectionService.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCollectionService.cs)
  - UI から参照するための読み取り API を追加する。
  - 追加候補は `IsDiscovered(string fairyId)`、`GetDiscoveredCount(IReadOnlyList<FairyDefinition> fairies)`。
- [Assets/UI/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/HudScreen.uxml)
  - `fairy-button` は維持しつつ、新規コレクション UXML をバインダから注入しやすい構成で使い続ける。
  - 必要ならテンプレート参照用の最小追加のみ行う。
- [Assets/UI/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/HudScreen.uss)
  - HUD 本体レイアウトを維持しつつ、コレクション表示中も見た目崩れが出ないようにする。
  - 必要に応じて `fairy-button` の状態制御クラスだけ追加する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `HubScreenBinder` に新規 `VisualTreeAsset`、必要なら `StyleSheet`、`FairyCatalogSource` 参照を割り当てる。

### 新規作成予定
- [Assets/UI/UXML/FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/UXML/FairyCollectionScreen.uxml)
  - 全画面背景オーバーレイ、右側パネル、タイトル、スクロール領域、閉じるボタン、発見数ラベルを持つ。
- [Assets/UI/USS/FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/USS/FairyCollectionScreen.uss)
  - ワイヤー準拠の半透明背景、右側パネル、2 列カードレイアウト、グレーアウト表現を定義する。

## データフロー / 処理フロー
1. シーン開始時、`HubScreenBinder` が HUD の `rootVisualElement` を取得する。
2. `HubScreenBinder` は Inspector で受けた `VisualTreeAsset` から `FairyCollectionScreen.uxml` を複製し、HUD ルートへ追加する。
3. コレクションオーバーレイ内の `close-button`、背景要素、スクロール領域、発見数ラベルを `Q()` で取得する。
4. プレイヤーが `fairy-button` を押すと、`FairyCatalogSource.GetFairies()` から全妖精一覧を取得する。
5. `HubScreenBinder` は各 `FairyDefinition` について `FairyCollectionService.IsDiscovered(fairy.Id)` を呼び、カード表示内容を決める。
6. 獲得済みなら `DisplayName`、`Icon`、固定文言 `好きなシール: ポップ・小さい` を表示する。
7. 未獲得なら名前を `*****`、補足文言を `未発見`、画像領域にグレーアウト用 class を付与する。
8. カードを 2 列グリッドとして `ScrollView` に追加する。
9. `FairyCollectionService.GetDiscoveredCount(fairies)` と `fairies.Count` から `発見した数: X/Y` を更新する。
10. オーバーレイ表示中は背景要素が pointer を受け、背面 HUD / ゲーム入力を遮断する。
11. `閉じる` ボタンまたは背景押下でオーバーレイを非表示にする。

## 処理詳細

### UI 構造
- `FairyCollectionScreen.uxml` は少なくとも以下の要素を持つ。
- `fairy-collection-overlay`
- `fairy-collection-backdrop`
- `fairy-collection-panel`
- `fairy-collection-title`
- `fairy-collection-scroll-view`
- `fairy-collection-empty-label`
- `fairy-collection-close-button`
- `fairy-collection-count-label`
- カード生成は UXML 静的定義ではなく、`HubScreenBinder` から `VisualElement` / `Label` / `Button` を組み立てる形でよい。現状のカード情報量なら専用 item UXML までは不要。

### バインド設計
- `HubScreenBinder` に以下の SerializeField を追加する。
- `VisualTreeAsset fairyCollectionScreenAsset`
- `FairyCatalogSource fairyCatalogSource`
- 既存 `uiDocument` と同じライフサイクルで初期化し、`OnEnable` でオーバーレイ生成、`OnDisable` でイベント解除を行う。
- オーバーレイが未設定でも NullReference で落ちないよう、防御コードを入れる。

### 状態参照 API
- `FairyCollectionService` は登録 API に加えて参照 API を持つ。
- `bool IsDiscovered(string fairyId)`
- `int GetDiscoveredCount(IReadOnlyList<FairyDefinition> fairies)`
- 集計時は null 要素や空 ID を除外し、UI 側が重複した条件分岐を持たないようにする。

### カード見た目
- 獲得済みカードは通常色で、画像は `StyleBackground` に `fairy.Icon.texture` を設定する。
- 未獲得カードはカード全体を消さず、画像枠だけに `fairy-card__image--undiscovered` の class を付ける。
- アイコン未設定時は背景画像なしでも崩れないプレースホルダー見た目にする。
- 補足文言は獲得済み時のみ `好きなシール: ポップ・小さい`、未獲得時は `未発見` を表示する。

### 入力制御
- 背景オーバーレイは `PickingMode.Position` を有効にして最前面でクリックを受ける。
- 背景押下時は閉じるが、パネル本体押下では閉じないようイベント伝播を抑制する。
- コレクション表示中に `ready-button` や `shop-button` が押されないことを前提に、背景が全画面を覆う構造にする。

## リスクと対策
- `HubScreenBinder` に一覧生成まで集約すると責務が増える。
  - 今回は 1 画面のみで関連度が高いため binder 集約で進めるが、将来詳細画面やフィルタが増えたら `FairyCollectionPanelController` へ分離する。
- `FairyCollectionService` が登録専用 API のままだと UI が内部状態へ直接触れたくなる。
  - 参照 API を追加し、UI はサービス経由でのみ獲得状態を知るようにする。
- 背景押下で閉じる実装時にパネル内クリックまで閉じる可能性がある。
  - パネル本体で click / pointer イベントを握りつぶし、背景だけで close を発火させる。
- 一覧件数増加時にカードサイズが崩れる可能性がある。
  - ScrollView の content を wrap レイアウトにし、カード幅を固定して 2 列を維持する。

## 検証方針
- 手動確認1:
  - `妖精` ボタン押下で半透明背景付きのコレクション画面が開くこと。
- 手動確認2:
  - `閉じる` ボタンと背景押下の両方で閉じること。
- 手動確認3:
  - 表示中に `ready-button`、`shop-button`、ゲーム画面の入力が反応しないこと。
- 手動確認4:
  - 獲得済み妖精は名前、画像、固定文言が表示されること。
- 手動確認5:
  - 未獲得妖精は名前が `*****`、補足文言が `未発見`、画像がグレーアウトになること。
- 手動確認6:
  - 妖精件数が増えてもスクロールで最後まで閲覧できること。
- 手動確認7:
  - 妖精 0 件のときに空表示と `発見した数: 0/0` が出ること。

## コードスニペット
```csharp
[SerializeField] private VisualTreeAsset fairyCollectionScreenAsset;
[SerializeField] private FairyCatalogSource fairyCatalogSource;

private VisualElement fairyCollectionOverlay;
private VisualElement fairyCollectionBackdrop;
private VisualElement fairyCollectionPanel;
private ScrollView fairyCollectionScrollView;
private Label fairyCollectionCountLabel;
private Label fairyCollectionEmptyLabel;
private Button fairyButton;
private Button fairyCollectionCloseButton;
```

```csharp
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

```csharp
private void CloseFairyCollection()
{
    if (fairyCollectionOverlay == null)
    {
        return;
    }

    fairyCollectionOverlay.style.display = DisplayStyle.None;
}
```
