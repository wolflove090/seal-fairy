# 妖精ごとの好きなシールとフレーバー個別設定 実装計画

## 実装方針
- 既存 UI はすでに [FairyCollectionScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/UXML/FairyCollectionScreen.uxml) と [FairyCollectionScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/FairyCollectionScreen/USS/FairyCollectionScreen.uss) に実装済みのため、今回の主変更点は表示データを固定文言から妖精マスタへ移すことに置く。
- 個別設定値は [FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs) に serialized field として追加し、`FairyCatalogSource` が保持する既存 `fairies` リスト経由で Inspector から編集できる構成にする。
- 表示時の文言解決は [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) に集約し、詳細画面と一覧カードの両方が同じデータ解決ロジックを使うようにする。
- 未設定時のフォールバック文言 `*****` は 1 か所の定数として管理し、null・空文字時にのみ適用する。
- シーン上の `FairyCatalogSource` には既存妖精データが [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) で直列化されているため、コード変更後に Inspector で各妖精の新規項目を埋める作業を前提にする。

## 変更対象ファイル一覧
- [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs)
  - 好きなシール自由入力文字列とフレーバーテキストの serialized field、および参照用プロパティを追加する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - 既存の固定文言定数をフォールバック専用へ置き換える。
  - 妖精ごとの表示値を解決する private helper を追加し、詳細画面と一覧カードの両方で利用する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `FairyCatalogSource` が保持する各妖精データに、新規項目の値を設定する。
- [Documentation/要件書/妖精ごとの好きなシールとフレーバー個別設定要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精ごとの好きなシールとフレーバー個別設定要件書.md)
  - 実装中の判断確認用として参照する。

## データフロー / 処理フロー
1. `Main.unity` 上の `FairyCatalogSource` は `fairies` リストとして複数の `FairyDefinition` を保持する。
2. 各 `FairyDefinition` は `displayName`、`icon` に加えて、`favoriteStickerText` と `flavorText` を保持する。
3. プレイヤーが妖精一覧から発見済み妖精を選ぶと、`HudScreenBinder.OpenFairyDetail(fairy)` が呼ばれる。
4. `ApplyFairyDetail(fairy)` は `ResolveFavoriteStickerText(fairy)` と `ResolveFlavorText(fairy)` を通じて表示文言を決定する。
5. `Resolve...()` は対象フィールドが null または空文字ならフォールバック `*****` を返し、値がある場合はそのまま返す。
6. 妖精一覧の `CreateFairyCard(fairy, isDiscovered)` も同じ `ResolveFavoriteStickerText(fairy)` を呼び、発見済みカードだけ個別設定値を表示する。
7. 未発見妖精カードは従来どおり `？？？` と伏せ表示を維持し、個別設定値は描画しない。

## 詳細設計

### 1. `FairyDefinition` のデータ拡張
- 追加フィールド案
  - `[SerializeField] private string favoriteStickerText;`
  - `[SerializeField, TextArea(3, 6)] private string flavorText;`
- 公開プロパティ案
  - `public string FavoriteStickerText => favoriteStickerText;`
  - `public string FlavorText => flavorText;`
- `TextArea` 属性で Inspector 上の複数行入力をしやすくし、フレーバーテキストの運用コストを下げる。

### 2. `HudScreenBinder` の文言解決責務
- 既存の `FairyDetailFavoriteText` と `FairyDetailFlavorText` は固定表示値ではなく、フォールバック定数 `FairyDetailFallbackText` に集約する。
- 文言解決 helper を追加する。
  - `private static string ResolveFavoriteStickerText(FairyDefinition fairy)`
  - `private static string ResolveFlavorText(FairyDefinition fairy)`
- helper は `fairy == null` も吸収し、UI 更新側では null チェックを増やさない。

### 3. 詳細画面反映
- `ApplyFairyDetail(FairyDefinition fairy)` の以下を差し替える。
  - `fairyDetailFavoriteValueLabel.text = ResolveFavoriteStickerText(fairy);`
  - `fairyDetailFlavorValueLabel.text = ResolveFlavorText(fairy);`
- 名前と画像の反映ロジックは現状維持でよい。
- 詳細モーダルの UXML/USS は既存要素名をそのまま使えるため、構造変更は不要とする。

### 4. 一覧カード反映
- `CreateFairyCard(FairyDefinition fairy, bool isDiscovered)` の `detailValue.text` を以下へ変更する。
  - 発見済み: `ResolveFavoriteStickerText(fairy)`
  - 未発見: `*****` または現行の伏せ表示ルールに合わせた伏せ値
- 今回の要件では未発見時に個別内容を出さないことが重要なため、発見済み分岐の内側だけで解決 helper を呼ぶ。

### 5. `Main.unity` の運用反映
- `FairyCatalogSource` の `fairies` 要素ごとに、新規フィールドを Inspector で入力する。
- 既存妖精数分の入力漏れがあるとフォールバック表示になるため、実装作業では全要素確認を完了条件に含める。
- YAML を直接編集するより、Unity Editor 上での更新を前提に手順書へ落とし込む。

## リスクと対策
- `FairyDefinition` の serialized field 追加により、既存シーン上では初期値が空文字のままとなる。
  - 対策: フォールバック `*****` を実装し、Inspector 入力が終わるまで UI が破綻しない状態にする。
- 詳細画面と一覧カードで文言解決ロジックが分散すると、表示差異が出る。
  - 対策: `HudScreenBinder` に helper を 2 つだけ用意し、両方から共通利用する。
- フレーバーテキストの改行量が増えると詳細モーダルの見た目が崩れる可能性がある。
  - 対策: 今回は UI 構造を変えず、既存詳細パネルで収まる文量を Inspector 入力時の運用ルールとして扱う。必要なら次段で USS 調整を切り出す。
- 未発見カードに誤って個別設定値を表示すると、発見前ネタバレになる。
  - 対策: `isDiscovered` 分岐の外では個別値を参照しない構造に固定する。

## 検証方針
- 手動確認1: `Main.unity` の各妖精へ異なる好きなシール文言を設定し、発見済みカードで差分表示されることを確認する。
- 手動確認2: 発見済み妖精の詳細モーダルで、妖精ごとに異なる好きなシールとフレーバーが表示されることを確認する。
- 手動確認3: 好きなシール未設定の妖精で、一覧カードと詳細画面の両方に `*****` が表示されることを確認する。
- 手動確認4: フレーバー未設定の妖精で、詳細画面に `*****` が表示されることを確認する。
- 手動確認5: 未発見妖精カードでは、従来どおり詳細が開かず、個別設定値も見えないことを確認する。
- 手動確認6: 妖精一覧の件数表示、開閉、ショップとの排他表示に回帰がないことを確認する。

## コードスニペット
```csharp
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

```csharp
private const string FairyDetailFallbackText = "*****";

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
}
```
