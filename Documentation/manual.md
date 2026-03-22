# 所持金付きシールショップ機能 作業手順書

## 目的
- 初期所持金を 1000 円で開始する。
- HUD 左上とショップフッターに現在の所持金を表示する。
- シールごとに価格を設定し、ショップカードへ表示する。
- 購入時に所持金を減算し、残高不足のシールはグレーアウトして購入不可にする。
- 購入成功後に所持金表示と所持シール一覧を即時更新する。

## 変更対象
- [Assets/Scripts/Sticker/StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs)
- [Assets/Scripts/Currency/CurrencyBalanceSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Currency/CurrencyBalanceSource.cs)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: StickerDefinition に価格を追加する
1. [StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs) を開く。
2. `stickerPrefab` の下に価格フィールドを追加する。
3. 価格は Inspector から設定できるように `SerializeField` を付ける。
4. 公開プロパティは負値を返さないようにする。

### 変更後コード
```csharp
using UnityEngine;

[System.Serializable]
public sealed class StickerDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private PeelSticker3D stickerPrefab;
    [SerializeField] private int price = 100;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public PeelSticker3D StickerPrefab => stickerPrefab;
    public int Price => Mathf.Max(0, price);
}
```

## 手順2: 所持金データソースを新規作成する
1. `Assets/Scripts` 配下に `Currency` フォルダを作る。
2. [CurrencyBalanceSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Currency/CurrencyBalanceSource.cs) を新規作成する。
3. 初期値 1000 円、現在残高、変更通知イベント、減算 API を持たせる。
4. `TrySpend` は残高不足と負値入力を拒否する。

### 新規ファイルコード
```csharp
using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CurrencyBalanceSource : MonoBehaviour
{
    [SerializeField] private int startingBalance = 1000;

    public event Action<int> BalanceChanged;

    public int CurrentBalance { get; private set; }

    private void Awake()
    {
        CurrentBalance = Mathf.Max(0, startingBalance);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (CurrentBalance < amount)
        {
            return false;
        }

        CurrentBalance -= amount;
        BalanceChanged?.Invoke(CurrentBalance);
        return true;
    }
}
```

## 手順3: HudScreenBinder に所持金表示更新処理を追加する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. `CurrencyBalanceSource` 参照用 `SerializeField` を追加する。
3. `moneyLabel` フィールドを追加する。
4. `OnEnable()` で `money-label` を取得する。
5. `OnEnable()` の末尾で `UpdateMoneyLabels()` を呼ぶ。
6. `OnDisable()` では所持金イベント購読を解除する。

### 追加するフィールド
```csharp
[SerializeField] private CurrencyBalanceSource currencyBalanceSource;

private Label moneyLabel;
```

### OnEnable の追加箇所
```csharp
moneyLabel = root.Q<Label>("money-label");
UpdateMoneyLabels();
SubscribeToCurrencySource();
```

### OnDisable の追加箇所
```csharp
UnsubscribeFromCurrencySource();
```

### 追加メソッド
```csharp
private void SubscribeToCurrencySource()
{
    if (currencyBalanceSource == null)
    {
        Debug.LogError("CurrencyBalanceSource が設定されていません");
        return;
    }

    currencyBalanceSource.BalanceChanged += HandleBalanceChanged;
}

private void UnsubscribeFromCurrencySource()
{
    if (currencyBalanceSource == null)
    {
        return;
    }

    currencyBalanceSource.BalanceChanged -= HandleBalanceChanged;
}

private void HandleBalanceChanged(int _)
{
    UpdateMoneyLabels();

    if (stickerShopOverlay != null && stickerShopOverlay.style.display == DisplayStyle.Flex)
    {
        RefreshStickerShop();
    }
}

private void UpdateMoneyLabels()
{
    int balance = currencyBalanceSource != null ? currencyBalanceSource.CurrentBalance : 0;
    string text = $"お金：{balance}円";

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

## 手順4: ショップカードに価格表示と購入可否判定を追加する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `CreateStickerShopCard()` を差し替える。
2. 名前ラベルの下に価格ラベルを追加する。
3. 現在残高と価格を比較して `canPurchase` を算出する。
4. 購入不可カードには `sticker-shop-card--disabled` クラスを付与し、`SetEnabled(false)` を適用する。

### 差し替えコード
```csharp
private Button CreateStickerShopCard(StickerDefinition item)
{
    Button card = new();
    card.AddToClassList("sticker-shop-card");

    VisualElement image = new();
    image.AddToClassList("sticker-shop-card__image");
    if (item != null && item.Icon != null)
    {
        image.style.backgroundImage = new StyleBackground(item.Icon.texture);
    }

    Label name = new();
    name.AddToClassList("sticker-shop-card__name");
    name.text = string.IsNullOrWhiteSpace(item?.DisplayName) ? "名称未設定" : item.DisplayName;

    Label price = new();
    price.AddToClassList("sticker-shop-card__price");
    price.text = item != null ? $"{item.Price}円" : "0円";

    bool canPurchase =
        item != null &&
        currencyBalanceSource != null &&
        currencyBalanceSource.CurrentBalance >= item.Price;

    card.Add(image);
    card.Add(name);
    card.Add(price);
    card.EnableInClassList("sticker-shop-card--disabled", !canPurchase);
    card.SetEnabled(canPurchase);

    if (canPurchase)
    {
        card.clicked += () => HandleStickerShopItemClicked(item);
    }

    return card;
}
```

## 手順5: 購入処理を所持金減算つきに更新する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `HandleStickerShopItemClicked()` を差し替える。
2. `currencyBalanceSource.TrySpend(item.Price)` 成功時のみ所持シール追加へ進める。
3. ログにはシール名、価格、購入後残高を含める。
4. 購入後にショップ一覧を再描画して、残高不足カードの見た目を更新する。

### 差し替えコード
```csharp
private void HandleStickerShopItemClicked(StickerDefinition item)
{
    if (item == null || inventorySource == null || currencyBalanceSource == null)
    {
        return;
    }

    if (!currencyBalanceSource.TrySpend(item.Price))
    {
        return;
    }

    inventorySource.AddOwnedStickerToFront(item);

    string displayName = string.IsNullOrWhiteSpace(item.DisplayName)
        ? "名称未設定"
        : item.DisplayName;

    Debug.Log($"ショップ購入: {displayName} / {item.Price}円 / 残高 {currencyBalanceSource.CurrentBalance}円");
    RefreshStickerShop();
}
```

## 手順6: StickerShopScreen.uss に価格表示とグレーアウトスタイルを追加する
1. [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を開く。
2. `sticker-shop-card__price` 用スタイルを追加する。
3. `sticker-shop-card--disabled` 用スタイルを追加する。
4. disabled 時は背景色、文字色、画像 opacity を落とす。

### 追加コード
```css
.sticker-shop-card__price {
    margin-top: 12px;
    font-size: 26px;
    color: rgb(50, 50, 50);
    -unity-text-align: middle-center;
}

.sticker-shop-card--disabled {
    background-color: rgb(210, 210, 210);
}

.sticker-shop-card--disabled .sticker-shop-card__image {
    opacity: 0.45;
}

.sticker-shop-card--disabled .sticker-shop-card__name,
.sticker-shop-card--disabled .sticker-shop-card__price {
    color: rgb(120, 120, 120);
}
```

## 手順7: Main.unity の参照と価格を設定する
1. Unity Editor で [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
2. 適切な GameObject に `CurrencyBalanceSource` を追加する。
3. `Starting Balance` を `1000` に設定する。
4. `HubScreenBinder` の `Currency Balance Source` にそのコンポーネントを割り当てる。
5. `StickerShopCatalogSource` が参照する各 `StickerDefinition` の `Price` を設定する。
6. HUD 左上の `money-label` とショップフッターの `sticker-shop-money-label` が既存 UXML 名のままであることを確認する。

## 手順8: 手動確認を行う
1. 起動直後に HUD 左上が `お金：1000円` になることを確認する。
2. `ショップ` を開き、フッターも `お金：1000円` を表示することを確認する。
3. 各販売シールカードに価格が表示されることを確認する。
4. 1000 円より高いシールがグレーアウトし、押せないことを確認する。
5. 300 円のシール購入後に HUD とショップフッターが `お金：700円` になることを確認する。
6. 購入成功時に所持一覧先頭へシールが追加されることを確認する。
7. 残高不足になったカードが、その場でグレーアウトへ切り替わることを確認する。
8. 残高不足カード押下で所持一覧と残高が変わらないことを確認する。
9. 同じシールを複数回購入すると、価格分ずつ残高が減り、所持数が増えることを確認する。
10. `Ready` によるフェーズ遷移と妖精コレクション画面が既存どおり動作することを確認する。

## 注意点
- `StickerDefinition` に価格を持たせるため、販売対象ではないシール定義にも価格欄が表示される。今回は構成簡素化を優先し、この設計を採用する。
- `CurrencyBalanceSource` は永続化を持たないため、シーン再読み込みで初期値 1000 円へ戻る。
- 同一定義のシール重複購入時、選択ハイライトの厳密な 1 枚識別までは今回の作業対象外。
