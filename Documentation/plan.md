# 妖精データJSONロード 実装計画

## 実装方針
- 現在の妖精マスタは [FairyCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogSource.cs) の Inspector 配列に保持され、利用側の [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) と [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) が `GetFairies()` を参照している。利用側 API を維持したまま、データ供給元のみ JSON 起動時ロードへ置き換える。
- `FairyCatalogSource` 自体に JSON 解析責務を寄せず、別ローダー/リポジトリを追加して責務を分離する。`FairyCatalogSource` はロード済み一覧の提供だけを担う薄いアダプタにする。
- JSON と画像はどちらも `Resources` 配下に配置し、妖精定義 JSON には `Resources.Load<Sprite>()` に渡す `iconResourcePath` を持たせる。
- 現在の妖精画像は [Assets/GameResources/Fairy/05.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/05.png)、[Assets/GameResources/Fairy/07.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/07.png)、[Assets/GameResources/Fairy/13.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Fairy/13.png) にあり `Resources` 参照できないため、JSON 化に合わせて `Resources` 配下へ移設または複製する。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity#L2806) に直列化されている既存 3 件の妖精データ (`バラちゃん`、`ウルフちゃん`、`もっさん`) は、初期 JSON として転記し、シーンから手入力マスタを除去する。

## 変更対象ファイル一覧
- [Assets/Scripts/Fairy/FairyCatalogSource.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogSource.cs)
  - Inspector 配列を廃止し、ロード済み妖精一覧の取得窓口へ変更する。
- [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs)
  - JSON DTO から組み立て可能なランタイム定義へ更新する。
- `Assets/Scripts/Fairy/FairyCatalogLoader.cs`
  - `Resources` から `TextAsset` を読み込み、JSON 解析と DTO 検証を行う新規ローダー。
- `Assets/Scripts/Fairy/FairyCatalogRepository.cs`
  - ロード済み `FairyDefinition` 一覧をキャッシュし、起動時初期化と参照を提供する新規リポジトリ。
- `Assets/Scripts/Fairy/FairyCatalogDto.cs`
  - `JsonUtility` 用のラッパー DTO と妖精1件分 DTO を定義する新規ファイル。
- `Assets/GameResources/Resources/Fairy/fairy_catalog.json`
  - 初期妖精マスタ JSON。
- `Assets/GameResources/Resources/Fairy/05.png`
- `Assets/GameResources/Resources/Fairy/07.png`
- `Assets/GameResources/Resources/Fairy/13.png`
  - `Resources.Load<Sprite>()` で解決できる配置へ移した妖精画像。
- [Assets/Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity)
  - `FairyCatalogSource` の旧 `fairies` 直列化データを除去し、新構成に合わせる。
- [Documentation/要件書/妖精データJSONロード要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精データJSONロード要件書.md)
  - 実装判断の基準として参照する。

## データフロー / 処理フロー
1. Unity 起動時に `FairyCatalogRepository.Initialize()` を実行する。
2. `FairyCatalogLoader` が `Resources.Load<TextAsset>("Fairy/fairy_catalog")` で JSON を取得する。
3. `JsonUtility.FromJson<FairyCatalogDto>()` で DTO を復元する。
4. DTO の各レコードを検証し、`id` 欠落や `weight <= 0` などの不正レコードはログを出してスキップする。
5. 有効レコードごとに `iconResourcePath` を使って `Resources.Load<Sprite>()` を実行し、`FairyDefinition` を組み立てる。
6. 完成した `FairyDefinition` 一覧を `FairyCatalogRepository` にキャッシュする。
7. `FairyCatalogSource.GetFairies()` はキャッシュ済み一覧を返す。
8. [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs#L244) の抽選処理と [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs#L764) の一覧表示は、従来どおり `GetFairies()` を通じてロード済み妖精を参照する。

## 詳細設計

### 1. `FairyDefinition` の責務見直し
- 現状の `FairyDefinition` は Inspector 用 serialized field のみを持つ POCO であり、JSON からの生成に向いた生成経路がない。
- 実装では以下のいずれかに統一する。
  - private field を constructor で初期化できる純粋なランタイムモデルへ変更する。
  - もしくは factory メソッド `Create(...)` を追加する。
- 利用側は `Id`、`DisplayName`、`Weight`、`Icon`、`FavoriteStickerText`、`FlavorText` の getter を継続利用できる形にする。
- `favoriteStickerText` と `flavorText` は null 許容入力を受け取り、表示側の既存フォールバック処理と整合させる。

### 2. JSON DTO と loader
- `JsonUtility` は配列直列化の都合でラッパー型が必要なため、以下の DTO を追加する。
  - `FairyCatalogDto { FairyRecordDto[] fairies; }`
  - `FairyRecordDto { string id; string displayName; int weight; string iconResourcePath; string favoriteStickerText; string flavorText; }`
- `FairyCatalogLoader` は以下を責務に持つ。
  - JSON `TextAsset` の取得
  - JSON 解析
  - DTO 検証
  - `Sprite` 解決
  - `FairyDefinition` への変換
- ローダー内部で `Debug.LogError` / `Debug.LogWarning` を使い、次のログ粒度を揃える。
  - JSON ファイル欠落
  - JSON 解析失敗
  - 個別レコードの必須値不足
  - `iconResourcePath` 解決失敗

### 3. Repository と起動時初期化
- `FairyCatalogRepository` は static 管理とし、起動時に 1 度だけロードして `IReadOnlyList<FairyDefinition>` を保持する。
- 初期化は `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` を優先し、シーン内 `MonoBehaviour` の `Awake` より前にロード完了を狙う。
- `SubsystemRegistration` でキャッシュ初期化を入れ、Play Mode 再実行時の状態汚染を防ぐ。
- `FairyCatalogSource` は `GetFairies()` で repository を返すだけに簡略化する。必要であれば Inspector 参照削除後もコンポーネント自体は残し、シーン参照互換を保つ。

### 4. `Resources` 配置
- JSON は `Assets/GameResources/Resources/Fairy/fairy_catalog.json` に配置する。
- 画像は `Assets/GameResources/Resources/Fairy/` 配下へ置き、JSON の `iconResourcePath` は拡張子抜きの `Fairy/13` 形式で記述する。
- 現在 UI で未発見妖精画像に使っている `transparent` は [Assets/GameResources/Texture/Resources/transparent.png](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Texture/Resources/transparent.png) から読めているため、同じ `Resources` ルールに寄せる。

### 5. `Main.unity` から JSON への移行
- 現在のシーン直列化値を JSON へ転記する。
  - `id: 1`, `displayName: バラちゃん`, `icon: 13.png`
  - `id: 2`, `displayName: ウルフちゃん`, `icon: 05.png`
  - `id: 3`, `displayName: もっさん`, `icon: 07.png`
- 転記対象には既存の `weight`、`favoriteStickerText`、`flavorText` も含める。
- 転記後、`Main.unity` 側の `fairies` 配列は削除し、シーンデータを二重管理しない。

## リスクと対策
- `Resources` 非配置のまま画像パスだけ JSON 化すると `Sprite` が解決できない。
  - 対策: 実装タスクに画像移設を含め、JSON の `iconResourcePath` と 1 対 1 に対応づける。
- `RuntimeInitializeOnLoadMethod` の順序次第で `TapStickerPlacer` や `HudScreenBinder` より初期化が遅れる可能性がある。
  - 対策: `BeforeSceneLoad` を使い、さらに repository 側で未初期化時の遅延初期化を許容する。
- 不正 JSON で全件空になると妖精発見導線が見えづらくなる。
  - 対策: ロード失敗時は明確な `Debug.LogError` を出し、個別レコード不正はスキップ対象を識別できるログを残す。
- `FairyDefinition` の生成方法変更で既存コードがコンパイルエラーになる可能性がある。
  - 対策: getter API は維持し、利用側変更を `FairyCatalogSource` 周辺に局所化する。
- シーン直列化データと JSON が並存すると、どちらが正か不明になる。
  - 対策: 移行完了時に `Main.unity` から旧妖精配列を除去する。

## 検証方針
- 手動確認1: 起動直後に `FairyCatalogSource.GetFairies()` が 3 件返すことを確認する。
- 手動確認2: シール配置時の妖精抽選が従来どおり動き、発見ログに `id` / `displayName` の整合があることを確認する。
- 手動確認3: 妖精コレクション一覧と詳細画面で、JSON の表示名・画像・好きなシール・フレーバーが表示されることを確認する。
- 手動確認4: JSON の 1 レコードだけ `id` や `weight` を壊し、そのレコードのみスキップされログに対象が出ることを確認する。
- 手動確認5: JSON ファイル名または配置を壊し、`Debug.LogError` が出たうえでゲームが停止せず継続することを確認する。
- 手動確認6: `iconResourcePath` を壊した妖精で、画像だけ null になっても一覧・詳細・抽選が致命的エラーにならないことを確認する。

## コードスニペット
```csharp
[System.Serializable]
public sealed class FairyRecordDto
{
    public string id;
    public string displayName;
    public int weight;
    public string iconResourcePath;
    public string favoriteStickerText;
    public string flavorText;
}

[System.Serializable]
public sealed class FairyCatalogDto
{
    public FairyRecordDto[] fairies;
}
```

```csharp
public static class FairyCatalogRepository
{
    private static readonly List<FairyDefinition> fairies = new();
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        fairies.Clear();
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        Initialize();
    }

    public static IReadOnlyList<FairyDefinition> GetFairies()
    {
        if (!initialized)
        {
            Initialize();
        }

        return fairies;
    }

    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        fairies.Clear();
        fairies.AddRange(FairyCatalogLoader.Load());
    }
}
```

```csharp
public sealed class FairyCatalogSource : MonoBehaviour
{
    public IReadOnlyList<FairyDefinition> GetFairies()
    {
        return FairyCatalogRepository.GetFairies();
    }
}
```
