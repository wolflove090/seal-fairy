# 所持金付きシールショップ機能 実装計画

## 実装方針
- シール価格は [StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs) に `price` フィールドを追加して管理する。ショップ専用データを増やさず、販売一覧・購入判定・ログ出力で同一定義を参照する。
- 所持金は新規のランタイムデータソースへ切り出す。`HubScreenBinder` に金額を直接持たせず、専用コンポーネントが「現在残高」「減算 API」「変更通知」を持つ構成にする。
- HUD 左上の `money-label` とショップフッターの `sticker-shop-money-label` は同じ所持金ソースを購読して表示更新する。
- ショップ購入可否は `所持金 >= StickerDefinition.Price` で判定し、購入不可カードは `SetEnabled(false)` と USS クラス付与でグレーアウトする。
- 既存の所持シール更新責務は [OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs) に残し、購入成功時のみ「所持金減算」と「所持シール追加」を同一ハンドラで連続実行する。
- 既存のショップ UI 資産は流用し、カード内に価格ラベルを追加する最小変更に留める。
- 既存のフェーズ制御 [SealPhaseBoostrap.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Phase/SealPhaseBootstrap.cs) とシール配置処理は壊さず、`HubScreenBinder` の公開 API は現状維持を優先する。

## 変更対象ファイル一覧

### 新規追加予定
- [Assets/Scripts/Currency/CurrencyBalanceSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Currency/CurrencyBalanceSource.cs)
  - 初期所持金、現在残高、減算 API、変更通知イベントを持つコンポーネント。
  - 初期値 1000 円を Inspector から変更可能にしつつ、デフォルト値を 1000 にする。

### 更新予定
- [Assets/Scripts/Sticker/StickerDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerDefinition.cs)
  - シール価格のシリアライズフィールドと公開プロパティを追加する。
  - 0 未満を避けるため `Mathf.Max(0, price)` で公開するか、実装側で負値を防ぐ。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - `money-label` の参照取得と表示更新を追加する。
  - 所持金ソース購読処理、ショップカードの価格表示、購入可否判定、購入成功時の減算処理を追加する。
  - 購入後に所持金表示・ショップ一覧・所持シール一覧を再描画する。
- [Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerSelection/OwnedStickerInventorySource.cs)
  - 既存の先頭追加 API をそのまま利用し、必要ならイベント発火タイミングだけ確認する。
- [Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/UXML/StickerShopScreen.uxml)
  - ショップカードに価格表示を入れる前提で、必要ならテンプレート構造に合わせた要素名を整理する。
- [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)
  - 価格ラベルと購入不可グレーアウト表示のスタイルを追加する。
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
  - `money-label` は既存利用で足りるため、必要なら初期文言のみ調整する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `CurrencyBalanceSource` をシーンへ配置し、`HubScreenBinder` へ参照を割り当てる。
  - 販売対象 `StickerDefinition` の価格を Inspector で設定する。
- [Documentation/todo.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/todo.md)
- [Documentation/manual.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/manual.md)

## データフロー / 処理フロー
1. 起動時、`CurrencyBalanceSource` が初期所持金 1000 円を保持する。
2. [HubScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `OnEnable()` で `money-label` と `sticker-shop-money-label` を取得し、現在残高を表示する。
3. プレイヤーが `ショップ` ボタンを押すと `OpenStickerShop()` が呼ばれ、`RefreshStickerShop()` 実行時に各 `StickerDefinition.Price` と現在残高を比較してカードを生成する。
4. カード生成時、購入可能なら通常表示・押下可能、購入不可ならグレーアウト・押下不可とする。
5. プレイヤーが購入可能カードを押すと `HandleStickerShopItemClicked(StickerDefinition item)` を呼ぶ。
6. ハンドラ内で `currencyBalanceSource.TrySpend(item.Price)` を実行し、成功時のみ `inventorySource.AddOwnedStickerToFront(item)` を実行する。
7. `CurrencyBalanceSource` は残高更新後に変更通知を発火し、`HubScreenBinder` が HUD とショップ内の金額表示を更新する。
8. 続けて `RefreshStickerShop()` を再実行し、残高不足になったカードをグレーアウトへ切り替える。
9. `OwnedStickerInventorySource` の変更通知で所持一覧 UI が再構築され、新規購入シールが先頭に表示される。
10. ショップ画面は開いたまま維持し、背景または `閉じる` のみで閉じる。

## 処理詳細

### 1. 所持金ソースの分離
- `CurrencyBalanceSource` は `CurrentBalance` と `TrySpend(int amount)` を公開する。
- `TrySpend` は `amount < 0` と `CurrentBalance < amount` を拒否し、成功時のみ残高を減らして `BalanceChanged` を発火する。
- 初期値は `[SerializeField] private int startingBalance = 1000;` を持たせる。
- 残高の直接書き換えは避け、減算は API 経由に限定する。

### 2. シール価格の保持
- `StickerDefinition` に `[SerializeField] private int price = 100;` のような価格フィールドを追加する。
- 公開プロパティは `public int Price => Mathf.Max(0, price);` とし、負値設定の事故を吸収する。
- 価格の表示文言は `"{price}円"` に統一する。

### 3. HUD / ショップ金額表示
- `HubScreenBinder` に `private Label moneyLabel;` を追加する。
- `OnEnable()` で `root.Q<Label>("money-label")` を取得する。
- `UpdateMoneyLabels()` を追加し、HUD とショップフッターの 2 ラベルへ同じ文言を設定する。
- ショップを開く前後に関係なく `BalanceChanged` 購読中は常に最新値を反映する。

### 4. ショップカード生成
- `CreateStickerShopCard(StickerDefinition item)` に価格ラベル生成を追加する。
- カード生成時に `bool canPurchase = currencyBalanceSource != null && currencyBalanceSource.CurrentBalance >= item.Price;` を算出する。
- `card.SetEnabled(canPurchase);` に加えて `card.EnableInClassList("sticker-shop-card--disabled", !canPurchase);` を付与する。
- 無効カードは hover / active 変化を抑え、文字色と画像の不透明度も落とす。

### 5. 購入処理
- `HandleStickerShopItemClicked` は以下の順で処理する。
  1. `item`、`inventorySource`、`currencyBalanceSource` の null チェック
  2. `currencyBalanceSource.TrySpend(item.Price)` 実行
  3. 失敗時は return
  4. `inventorySource.AddOwnedStickerToFront(item)` 実行
  5. 購入ログ出力
  6. `RefreshStickerShop()` 実行
- これにより、減算失敗時に所持シールだけ増える不整合を防ぐ。

### 6. 所持一覧への影響
- 所持シール一覧の再構築は現状の `OwnedStickersChanged` 購読に乗せる。
- 選択状態維持ロジックは現状の `BuildStickerList()` を踏襲し、購入しても別シール選択が崩れないようにする。
- 同一定義の重複購入時に複数セルが同時選択表示になる既知挙動は、今回のスコープでは容認するか、補正不要として文書に残す。

## リスクと対策
- `HubScreenBinder` の責務がさらに肥大化し、UI 更新ロジックが散らばる可能性がある。
  - 金額表示更新を `UpdateMoneyLabels()`、ショップ再描画を `RefreshStickerShop()` に寄せ、処理単位を明確に分ける。
- `StickerDefinition` に価格追加後、既存シーン・Prefab の価格未設定で 0 円購入が発生する可能性がある。
  - [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) 内の販売シール設定確認を検証項目へ含める。
- 購入不可カードを `SetEnabled(false)` のみにすると見た目の差が弱い可能性がある。
  - USS クラスで背景色、文字色、画像 opacity を落として明示する。
- 残高変更時にショップ一覧を更新しないと、購入可否表示が古いまま残る。
  - 購入成功直後に必ず `RefreshStickerShop()` を実行し、必要なら `BalanceChanged` 時にもショップオーバーレイ表示中のみ再評価する。
- 新規 `CurrencyBalanceSource` 参照がシーンで未設定だと購入処理が無効になる。
  - `HubScreenBinder` 初期化時の null ガードと `Debug.LogError` を入れ、シーン設定漏れを早期発見する。

## 検証方針
- 手動確認1: 起動直後、HUD 左上とショップフッターに `お金：1000円` が表示される。
- 手動確認2: 各販売シールカードに価格が表示される。
- 手動確認3: 1000 円より高い価格のシールはグレーアウトされ、押せない。
- 手動確認4: 300 円のシールを購入すると、HUD とショップフッターの両方が `お金：700円` に更新される。
- 手動確認5: 購入成功時に対象シールが所持一覧先頭へ追加される。
- 手動確認6: 残高低下で購入不可になった他カードが、その場でグレーアウトへ切り替わる。
- 手動確認7: 残高不足カード押下で所持金も所持シールも変化しない。
- 手動確認8: 同じシールを複数回購入すると、購入回数分だけ残高が減り、所持数が増える。
- 手動確認9: ショップ画面は購入後も開いたままで、`閉じる` または背景押下で閉じる。
- 手動確認10: シール配置フェーズ、フェーズ遷移、妖精コレクション画面が既存どおり動作する。

## コードスニペット
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
        if (amount < 0 || CurrentBalance < amount)
        {
            return false;
        }

        CurrentBalance -= amount;
        BalanceChanged?.Invoke(CurrentBalance);
        return true;
    }
}
```

```csharp
[System.Serializable]
public sealed class StickerDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private PeelSticker3D stickerPrefab;
    [SerializeField] private int price = 100;

    public int Price => Mathf.Max(0, price);
}
```

```csharp
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
    Debug.Log($"ショップ購入: {item.DisplayName} / {item.Price}円 / 残高 {currencyBalanceSource.CurrentBalance}円");
    RefreshStickerShop();
}
```
