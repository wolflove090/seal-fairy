# ショップUIと周辺UIブラッシュアップ 実装計画

## 実装方針
- 既存のショップ購入、所持金更新、所持シール一覧更新ロジックは [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を中心に維持し、今回は UI 構造と表示スタイルの刷新を主対象にする。
- HUD とショップの見た目変更は UI Toolkit の UXML / USS へ集約し、座標や色、角丸などのレイアウト定数を C# 側へ持ち込まない。
- 添付モック準拠のため、[HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) と [StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml) の階層を組み替え、`HudScreenBinder` のクエリ名とカード生成先を同期更新する。
- ショップカードと所持シールタイルは、静的 UXML テンプレートではなく現在の動的生成方式を維持する。その代わり、カード内部の子要素構成と USS クラス設計を刷新する。
- 所持金表示はテキスト単独から「アイコン + 数値」構成へ変更するため、UI Toolkit の `VisualElement` 背景画像で新規コインアセットを表示する。
- 既存の妖精コレクション画面は本タスクのスコープ外だが、ショップ画面と同じ右側オーバーレイ構造を共有しているため、ショップ側だけを独立してブラッシュアップしても開閉排他が壊れないことを確認する。

## 変更対象ファイル一覧

### 更新予定
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
  - 所持金パネルを `money-icon` + `money-label` の複合構成へ変更する。
  - 左下の所持シール一覧パネルをヘッダー帯とコンテンツ領域に分離する。
  - 必要に応じて右下メニューや既存ラベルのラッパー要素を追加し、モックに近いレイヤ構造に整理する。
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
  - 所持金パネル、所持シール一覧、右下メニュー、選択中タイルのスタイルを全面更新する。
  - ピンク基調の配色、白縁、角丸、影風表現、スクロール領域の見た目を定義する。
- [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml)
  - タイトル帯、閉じるボタン、ショップ内コンテンツフレーム、ショップ内所持金表示の配置をモック準拠へ組み替える。
  - `ScrollView` を内側フレームへ収め、ヘッダー固定の構造にする。
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
  - 右側大型ショップパネルの色、余白、装飾、グリッド間隔、閉じるボタン見た目を刷新する。
  - ショップカードの画像領域、シール名、価格プレート、無効状態の表現を定義する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - UXML 名称変更に追従したクエリ更新を行う。
  - 所持金ラベル更新処理を、数値主体表示へ合わせて調整する。
  - コインアイコンやショップ内所持金枠など、追加要素があっても既存ロジックが崩れない初期化順へ整理する。
  - 動的生成するショップカードと所持シールタイルの子要素構成と USS クラスを刷新する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - 必要に応じて `UIDocument` の参照先や新規スプライト参照用 SerializedField を確認する。

### 新規追加予定
- [Assets/UI/Common](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/Common)
  - 既存配置に合わせて共通 UI アセット置き場が必要なら作成する。
- `Assets/GameResources/Texture` または UI 用適切な配置先
  - 所持金パネル用のコインアイコン画像を追加する。

## データフロー / 処理フロー
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) が `UIDocument.rootVisualElement` から HUD の要素を取得する。
2. `money-label` の表示文言は `CurrencyBalanceSource.CurrentBalance` をもとに数値主体フォーマットへ変換して反映する。
3. `shop-button` 押下でショップオーバーレイを開き、`RefreshStickerShop()` が販売カードを再生成する。
4. `RefreshStickerShop()` は [StickerShopCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerShop/StickerShopCatalogSource.cs) から販売シール一覧を取得し、各 `StickerDefinition` ごとにカードを生成する。
5. カード生成時、画像、名称、価格ラベルを新しい子要素構成で組み立て、残高不足時は無効クラスと非活性状態を付与する。
6. カード押下時、既存どおり `CurrencyBalanceSource.TrySpend()` と `OwnedStickerInventorySource.AddOwnedStickerToFront()` を実行する。
7. `OwnedStickersChanged` を受けた `BuildStickerList()` が左下の所持シール一覧を新デザインのタイルで再生成する。
8. `money-label` と `sticker-shop-money-label` は同じ残高ソースから再描画され、HUD とショップ内で同期する。

## 処理詳細

### 1. HUD UXML 再構成
- `money-panel` 直下を `money-icon` と `money-label` を持つ横並びレイアウトに変更する。
- `bottom-left-sticker-panel` は、ヘッダー帯と本体コンテナを分けるため、`sticker-list-header` と `sticker-list-body` のようなラッパーを追加する。
- `sticker-scroll-view` は本体コンテナの内側へ配置し、内側余白と白縁表現を USS で扱いやすくする。
- 既存の `preview-count-label` は現状仕様を維持し、今回の見た目変更対象からは外す。

### 2. ショップ UXML 再構成
- `sticker-shop-panel` 直下を「ヘッダー」と「コンテンツフレーム」の 2 層に分ける。
- ヘッダーには `sticker-shop-title` と `sticker-shop-close-button` を置く。
- コンテンツフレーム内に `sticker-shop-money-label`、`sticker-shop-empty-label`、`sticker-shop-scroll-view` を配置し、ショップを開いた瞬間に残高とカード群を確認できる構成にする。
- 背景用 `sticker-shop-backdrop` は左側ゲーム背景がうっすら見える半透明値へ調整する。

### 3. 所持金表示更新の調整
- `UpdateMoneyLabels()` は `お金：{balance}円` ではなく、3 桁区切りの数値文字列を返す形へ変更する。
- 文字列フォーマットは `999,999` のような見た目を基準とし、ショップ内表示も同じルールに揃える。
- ショップ内にラベルのみで残高を置くか、アイコン付き枠にするかは UXML 構成に合わせて `sticker-shop-money-label` または専用コンテナへ反映する。

### 4. 所持シールタイル生成の刷新
- `CreateStickerCell()` の生成物を「画像 + 小さな名前ラベル + 枚数バッジ」の構成へ更新する。
- タイルサイズは現在の 112px 正方形よりやや大きめに見直し、モック相当の 2 行 3 列前後が収まるサイズを基準にする。
- 選択状態は黄色背景ではなく、新デザインに馴染む枠線・明度差・影の組み合わせへ変更する。
- `GetOwnedStickerCount()` の表示は既存どおり `xN` を継続するが、バッジ位置と色を新デザインに合わせる。

### 5. ショップカード生成の刷新
- `CreateStickerShopCard()` はカード全体ボタンの中に `imageFrame`、`nameLabel`、`pricePlate` を組み立てる。
- 画像領域は黄緑背景 + 大きめ画像表示とし、名称は画像領域の下側寄せで白文字表示する。
- 価格は白い独立プレート上にピンク文字で数値のみ表示する。
- 無効状態では `sticker-shop-card--disabled` に加え、画像、名称、価格プレートの子要素にも一貫した減衰クラスを当てられる構成にする。

### 6. コインアイコン追加
- コインアイコンは新規画像をプロジェクトへ追加し、UI Toolkit の背景画像として参照する。
- 参照方法は以下のいずれかに統一する。
  - UXML 上の `VisualElement` に背景画像を直接設定
  - `HudScreenBinder` の SerializedField で `Sprite` を受け、起動時に `StyleBackground` を設定
- 既存コードとの整合と差し替え容易性を優先し、実装時には参照方法を 1 つに固定する。

## リスクと対策
- UXML の階層変更で `root.Q()` が要素取得に失敗する可能性がある。
  - 計画どおり要素名を一覧化し、`OnEnable()` の取得対象をまとめて見直す。
- ショップカードの見た目を強く変えると、無効状態や hover 時の視認性が落ちる可能性がある。
  - 通常 / hover / active / disabled を USS で明示的に個別定義する。
- 数値主体表示へ変更すると、既存の `お金：` 前提ログや UI 文言と不整合が出る可能性がある。
  - UI 表示だけを変更し、ログ文言と内部変数名は変更しない。
- コインアイコンの追加先が散ると後で差し替えしづらい。
  - 追加先ディレクトリを UI 用アセットに寄せ、用途が分かるファイル名にする。
- 左下パネルのタイルサイズを上げると、一覧件数が増えた時の可視件数が減る。
  - 初期表示件数とスクロール操作性のバランスを見て、2 行表示を優先する。

## 検証方針
- 手動確認1: 通常時の HUD 左上にピンク背景 + コインアイコン + 数値主体の所持金表示が出る。
- 手動確認2: 所持金が変化すると HUD 左上とショップ内の表示が同じ値へ即時更新される。
- 手動確認3: 左下の所持シール一覧がピンクヘッダー付きパネルで表示され、所持 0 件時も崩れない。
- 手動確認4: ショップを開くと右側に大型ピンクパネルが表示され、`シールショップ` タイトルと `X` 閉じるボタンが見える。
- 手動確認5: ショップカードが 3 列グリッドで並び、画像、名称、価格プレートがモックに近い配置で表示される。
- 手動確認6: 残高不足カードが無効見た目となり、クリックしても購入されない。
- 手動確認7: 購入すると所持シール一覧が新デザインのまま更新され、先頭へ追加される。
- 手動確認8: ショップと妖精コレクションの排他表示、フェーズ切替、既存ログ出力が退行しない。

## コードスニペット
```csharp
private void UpdateMoneyLabels()
{
    RefreshMoneyLabelReferences();

    int balance = currencyBalanceSource != null ? currencyBalanceSource.CurrentBalance : 0;
    string text = balance.ToString("N0");

    if (moneyLabel != null)
    {
        moneyLabel.text = text;
    }

    if (stickerShopMoneyLabel != null)
    {
        stickerShopMoneyLabel.text = text;
    }
}
```

```csharp
private Button CreateStickerShopCard(StickerDefinition item)
{
    Button card = new();
    card.AddToClassList("sticker-shop-card");

    VisualElement imageFrame = new();
    imageFrame.AddToClassList("sticker-shop-card__image-frame");

    VisualElement image = new();
    image.AddToClassList("sticker-shop-card__image");
    if (item != null && item.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(item.Icon.texture);
    }

    Label name = new();
    name.AddToClassList("sticker-shop-card__name");
    name.text = string.IsNullOrWhiteSpace(item?.DisplayName) ? "名称未設定" : item.DisplayName;

    VisualElement pricePlate = new();
    pricePlate.AddToClassList("sticker-shop-card__price-plate");

    Label price = new();
    price.AddToClassList("sticker-shop-card__price");
    price.text = item != null ? item.Price.ToString("N0") : "0";

    imageFrame.Add(image);
    imageFrame.Add(name);
    pricePlate.Add(price);
    card.Add(imageFrame);
    card.Add(pricePlate);
    return card;
}
```
