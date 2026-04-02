# 妖精ごとの好きなシールとフレーバー個別設定 作業手順書

## 目的
- 妖精ごとに異なる「好きなシール」と「フレーバー」を設定できるようにする。
- 妖精コレクション詳細画面と一覧カードが、固定文言ではなく妖精マスタの値を表示するようにする。
- 未設定時は `*****` を表示し、未発見妖精には個別内容を見せないようにする。

## 変更対象
- [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)

## 手順1: `FairyDefinition` に個別設定項目を追加する
1. [FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) を開く。
2. `icon` の下に、好きなシール用文字列とフレーバー文字列を追加する。
3. フレーバーは Inspector で複数行入力しやすいよう `TextArea(3, 6)` を付ける。
4. 末尾の public property に参照用 getter を追加する。

### 変更例
```csharp
using UnityEngine;

[System.Serializable]
public sealed class FairyDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int weight = 1;
    [SerializeField] private Sprite icon;
    [SerializeField] private string favoriteStickerText;
    [SerializeField, TextArea(3, 6)] private string flavorText;

    public string Id => id;
    public string DisplayName => displayName;
    public int Weight => weight;
    public Sprite Icon => icon;
    public string FavoriteStickerText => favoriteStickerText;
    public string FlavorText => flavorText;
}
```

## 手順2: `HudScreenBinder` の固定文言をフォールバック定数へ置き換える
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. ファイル先頭の以下 2 定数を探す。
3. `FairyDetailFavoriteText` と `FairyDetailFlavorText` は削除し、共通フォールバック定数に置き換える。

### 変更例
```csharp
private const string FairyDetailFallbackText = "*****";
```

## 手順3: 文言解決 helper を追加する
1. `CloseFairyDetail()` の近く、または妖精詳細関連メソッドのまとまりに helper を追加する。
2. `favoriteStickerText` と `flavorText` が null または空文字なら `*****` を返す。
3. `fairy == null` の場合も同じく `*****` を返す。

### 追加例
```csharp
private static string ResolveFavoriteStickerText(FairyDefinition fairy)
{
    if (fairy == null || string.IsNullOrWhiteSpace(fairy.FavoriteStickerText))
    {
        return FairyDetailFallbackText;
    }

    return fairy.FavoriteStickerText;
}

private static string ResolveFlavorText(FairyDefinition fairy)
{
    if (fairy == null || string.IsNullOrWhiteSpace(fairy.FlavorText))
    {
        return FairyDetailFallbackText;
    }

    return fairy.FlavorText;
}
```

## 手順4: 詳細画面の表示処理を個別設定対応に変える
1. `ApplyFairyDetail(FairyDefinition fairy)` を探す。
2. 名前と画像の既存処理はそのまま残す。
3. 好きなシールとフレーバーに固定文言を入れている箇所を helper 呼び出しへ置き換える。

### 変更例
```csharp
private void ApplyFairyDetail(FairyDefinition fairy)
{
    if (fairyDetailNameLabel == null ||
        fairyDetailImage == null ||
        fairyDetailFavoriteValueLabel == null ||
        fairyDetailFlavorValueLabel == null)
    {
        return;
    }

    fairyDetailNameLabel.text = string.IsNullOrWhiteSpace(fairy.DisplayName)
        ? "名称未設定"
        : fairy.DisplayName;

    fairyDetailFavoriteValueLabel.text = ResolveFavoriteStickerText(fairy);
    fairyDetailFlavorValueLabel.text = ResolveFlavorText(fairy);

    if (fairy.Icon != null)
    {
        fairyDetailImage.style.backgroundImage = new StyleBackground(fairy.Icon.texture);
    }
    else
    {
        fairyDetailImage.style.backgroundImage = StyleKeyword.None;
    }
}
```

## 手順5: 一覧カードの好きなシール表示を個別設定へ置き換える
1. `CreateFairyCard(FairyDefinition fairy, bool isDiscovered)` を探す。
2. `detailValue.text = FairyDetailFavoriteText;` となっている箇所を差し替える。
3. 発見済みのときだけ個別値を表示し、未発見時は `*****` を表示する。

### 変更例
```csharp
Label detailValue = new();
detailValue.AddToClassList("fairy-card__detail-value");
detailValue.text = isDiscovered
    ? ResolveFavoriteStickerText(fairy)
    : FairyDetailFallbackText;
```

## 手順6: Unity Editor で妖精ごとの値を入力する
1. [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を Unity Editor で開く。
2. `FairyCatalogSource` を持つオブジェクトを選択する。
3. `fairies` リストの各要素に対して、`Favorite Sticker Text` と `Flavor Text` を入力する。
4. すべての妖精で入力方針を揃える。
5. 未設定のまま残す妖精がある場合は、意図的に `*****` フォールバックになることを確認する。

## 手順7: 動作確認を行う
1. Unity でゲームを起動する。
2. 発見済み妖精を複数用意し、妖精一覧を開く。
3. 発見済みカードごとに異なる好きなシール文言が表示されることを確認する。
4. 詳細モーダルで、好きなシールとフレーバーが妖精ごとに切り替わることを確認する。
5. 好きなシール未設定の妖精で、一覧カードと詳細画面に `*****` が表示されることを確認する。
6. フレーバー未設定の妖精で、詳細画面に `*****` が表示されることを確認する。
7. 未発見妖精カードを確認し、個別設定値が見えないことを確認する。
8. 妖精一覧の開閉、詳細モーダルの開閉、ショップとの排他表示に回帰がないことを確認する。

## 完了条件
- `FairyDefinition` に好きなシール文字列とフレーバー文字列が追加されている。
- 妖精詳細画面が妖精ごとの個別設定値を表示する。
- 妖精一覧カードの好きなシール欄が発見済み妖精ごとに切り替わる。
- 未設定時は `*****`、未発見時は個別内容非表示のルールが守られている。
- `Main.unity` の `FairyCatalogSource` に必要な値が入力されている。
