# 妖精ごとのシール排出テーブル 実装計画

## 実装方針
- 現在の妖精抽選は [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs) から [FairyWeightedRandomSelector.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs) を呼ぶ全体重み抽選であり、配置したシール種別を考慮していない。これを「ゲーム開始時に `StickerDefinition.Id` ごとの排出テーブルを構築し、配置時はそのキャッシュを使って抽選する」方式へ置き換える。
- 妖精の好み情報は JSON マスタに保持し、`FairyDefinition` にランタイムモデルとして展開する。`StickerDefinition` 側には既存の `Id` があるため、新たなマスタを追加せず `stickerId + weight` の組み合わせを妖精定義側へ持たせる。
- 既存の `FairyDefinition.Weight` は今回の仕様では不要なため削除する。既存 JSON、DTO、ローダー、ランタイムモデル、抽選ロジックから一貫して外す。
- 配置時の乱数処理は 3 段階に分ける。
  1. `50%` で妖精入りシールか判定する。
  2. 妖精入りなら、対象シールの排出テーブルを使って重み付き一次抽選する。
  3. 一次抽選が候補なし、または発見済み妖精当選で空振りした場合のみ、`50%` で未発見のその他妖精から等確率救済抽選する。
- 抽選ロジックを `TapStickerPlacer` に直接埋め込まず、ゲーム開始時の排出テーブル構築と配置時の抽選責務を新規セレクタ/リポジトリへ分離する。`TapStickerPlacer` は「配置したシールを渡して妖精割り当て結果を受け取る」だけに留める。

## 変更対象ファイル一覧
- [Assets/Scripts/Fairy/FairyDefinition.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyDefinition.cs)
  - `Weight` を削除し、好みシール一覧を保持できるランタイムモデルへ変更する。
- [Assets/Scripts/Fairy/FairyCatalogDto.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogDto.cs)
  - `weight` を廃止し、`preferredStickers` 配列 DTO を追加する。
- [Assets/Scripts/Fairy/FairyCatalogLoader.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogLoader.cs)
  - 新 DTO を検証し、`FairyDefinition` を好みシール込みで構築する。
- [Assets/GameResources/Resources/Fairy/fairy_catalog.json](/Users/tatsuki/Projects/Unity/SealFairy/Assets/GameResources/Resources/Fairy/fairy_catalog.json)
  - 各妖精に `preferredStickers` を定義し、旧 `weight` を削除する。
- [Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs)
  - 廃止または互換用途の整理対象。今回の抽選用途から外す。
- `Assets/Scripts/Fairy/FairyStickerPreference.cs`
  - 妖精 1 体が持つ `stickerId + weight` を表す新規ランタイムモデル。
- `Assets/Scripts/Fairy/StickerFairySelector.cs`
  - 対象シール ID に対応するキャッシュ済み排出テーブルを参照し、一次抽選と救済抽選で最終妖精を決定する新規クラス。
- `Assets/Scripts/Fairy/StickerFairyTableRepository.cs`
  - ゲーム開始時にシール別排出テーブルを構築・保持する新規リポジトリ。
- [Assets/Scripts/Sticker/TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs)
  - 既存の単純抽選呼び出しを、新セレクタ経由の割り当てへ差し替える。
- [Documentation/要件書/妖精ごとのシール排出テーブル要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/妖精ごとのシール排出テーブル要件書.md)
  - 実装判断の基準として参照する。

## データフロー / 処理フロー
1. 起動時に [FairyCatalogRepository.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogRepository.cs) が JSON から妖精一覧をロードする。
2. [FairyCatalogLoader.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyCatalogLoader.cs) が各妖精レコードの `preferredStickers` を読み取り、`FairyStickerPreference` 一覧へ変換する。
3. `StickerFairyTableRepository` が、全妖精の好み設定を走査し、`stickerId -> (fairy, weight)` の排出テーブルをシールごとに構築してキャッシュする。
4. 配置時に [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs#L233) が、選択中の `StickerDefinition` を取得する。
5. `TapStickerPlacer` は `50%` 判定で妖精入りでない場合、従来どおり妖精なしで [StickerRuntimeRegistry.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/StickerRuntimeRegistry.cs) へ登録する。
6. 妖精入りの場合、`StickerFairySelector` が対象シール ID の排出テーブルから一次抽選を行う。
7. 一次抽選で未発見妖精が当たればそのまま返し、発見済み妖精当選または候補なしなら空振り扱いにする。
8. 空振り時のみ、対象シールを好まない未発見妖精を救済候補として集め、`50%` で等確率抽選する。
9. 最終的に返った妖精を `StickerFairyAssignment` に詰めて登録し、剥がし時は既存どおり割り当て済み結果だけを消費する。

## 詳細設計

### 1. 妖精の好みデータ構造
- JSON の妖精 1 件は、既存の `id`、`displayName`、`iconResourcePath`、`favoriteStickerText`、`flavorText` に加えて `preferredStickers` 配列を持つ。
- `preferredStickers` の 1 件は以下の最小構造とする。
  - `stickerId`
  - `weight`
- ランタイムでは `FairyStickerPreference` を新規追加し、`StickerId` と `Weight` を不変値として保持する。
- `FairyDefinition` は `IReadOnlyList<FairyStickerPreference>` を公開し、UI からは従来どおり表示名、画像、好きなシール文言、フレーバーだけを参照できるようにする。
- 同一妖精に同一 `stickerId` が複数定義されていた場合は、ローダー段階で合算済みの 1 件へ正規化する。

### 2. DTO / ローダーの再設計
- `FairyRecordDto.weight` は削除する。
- 代わりに以下の DTO を追加する。
  - `PreferredStickerDto { string stickerId; int weight; }`
- `FairyCatalogLoader` は次の検証を担う。
  - `id` が空でないこと
  - `preferredStickers` の各要素が null でないこと
  - `stickerId` が空でないこと
  - `weight > 0` であること
- 妥当でない好み設定はその要素だけをスキップし、妖精レコード全体は可能な限り生かす。
- 好み設定を 1 件も持たない妖精もロードは許可する。該当妖精は通常抽選では出ず、救済抽選候補にのみ入る。

### 3. 排出テーブルリポジトリ
- `StickerFairyTableRepository` は static 管理とし、妖精カタログロード後にシール別排出テーブルを 1 度だけ構築する。
- テーブルは `Dictionary<string, List<StickerFairyTableEntry>>` のような形で保持し、各エントリは `FairyDefinition` と重みを持つ。
- 同一 `stickerId` に対して複数妖精の重みを蓄積し、シール単位の総重み計算を容易にする。
- `SubsystemRegistration` でキャッシュをリセットし、Play Mode 再実行時の状態汚染を防ぐ。

### 4. 抽選セレクタ
- `StickerFairySelector` は static class か軽量 service とし、少なくとも次の責務を持つ。
  - 対象シールのキャッシュ済み排出テーブル取得
  - 重み付き乱数抽選
  - 発見済み当選時の空振り判定
  - 救済候補の構築
  - `50%` 救済抽選
- 一次抽選では発見済み妖精も除外せず、排出テーブル全体を使って抽選する。
- 一次抽選で当選した妖精が発見済みなら `FairyCollectionService.IsDiscovered(fairy.Id)` により空振り扱いにする。
- 救済候補は「対象シールを好まない未発見妖精」とする。
- 乱数レンジは要件書どおり `1..totalWeight` を意識した実装にする。Unity の `Random.Range(int, int)` を使う場合は `0..totalWeight-1` との対応をコメントで明示する。

### 5. `TapStickerPlacer` の責務整理
- 現状の `SpawnSticker()` 内には「シール生成」「50% 判定」「妖精抽選」「演出付与」が混在している。
- 実装では、妖精割り当て部分を `ResolveFairyAssignment(StickerDefinition selectedSticker)` のような private メソッドへ切り出し、その中で `StickerFairySelector` を呼ぶ。
- `selectedSticker` が null、`selectedSticker.Id` が空、`fairyCatalogSource` が未設定などの異常時は、妖精なしで安全継続する。
- 妖精が選ばれたときだけ既存の `AttachFairyEffect()` を呼ぶ。

### 6. 既存 `Weight` の整理
- `FairyDefinition.Weight` は今回の排出仕様では使わないため、モデル・DTO・JSON から削除する。
- [FairyWeightedRandomSelector.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Fairy/FairyWeightedRandomSelector.cs) は参照箇所が [TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/Sticker/TapStickerPlacer.cs#L244) のみであるため、差し替え後に不要ファイルとして削除候補にする。
- 既存コードで `Weight` 参照が残っていないことを `rg "Weight"` で確認する前提で進める。

## リスクと対策
- JSON 構造変更により既存 `fairy_catalog.json` がローダー非互換になる。
  - 対策: DTO と JSON を同時更新し、`weight` 依存の検証を削除する。
- 妖精が好み設定を持たない場合、通常抽選で一切出なくなる。
  - 対策: 要件どおり救済抽選候補へ入る挙動を明文化し、ローダーでレコード自体は生かす。
- 発見済み妖精を一次抽選に含めるため、未発見妖精の体感排出率が下がる。
  - 対策: これは仕様として受け入れ、空振り時だけ救済抽選を入れて偏りを緩和する。
- 同一妖精の同一シール重複設定を放置すると、抽選レンジとデータ表示が一致しなくなる。
  - 対策: ローダーで `stickerId` 単位に合算して正規化する。
- `TapStickerPlacer` にロジックを足し込みすぎると配置責務と抽選責務が再び混ざる。
  - 対策: セレクタクラスを追加し、`TapStickerPlacer` では入力とシール生成に責務を限定する。
- 救済抽選の発火条件を「外れた時」ではなく「候補なし時」にしないと、要件との差異が出る。
  - 対策: 一次抽選が存在する場合は必ず誰かが当たる設計であり、救済抽選は候補なし時のみに限定する。

## 検証方針
- 手動確認1: ゲーム開始時にシール別排出テーブルが構築されることをログまたはデバッガで確認する。
- 手動確認2: 例としてジョウロに `A=7`, `B=4` を設定した場合、一次抽選総重みが 11 として扱われることを確認する。
- 手動確認3: 妖精Aを発見済みにした状態でジョウロを置いたとき、A のレンジに当たると空振りになることを確認する。
- 手動確認4: 一次抽選候補なし、または発見済み妖精当選による空振り時だけ、救済抽選が `50%` で発火することを確認する。
- 手動確認5: 救済抽選では、対象シールを好まない未発見妖精から等確率で選ばれることを確認する。
- 手動確認6: 妖精が割り当てられなかったシールでも、配置と剥がしが正常に継続することを確認する。
- 手動確認7: めくり時に再抽選が起きず、配置時に確定した妖精だけが消費されることを確認する。

## コードスニペット
```csharp
[System.Serializable]
public sealed class PreferredStickerDto
{
    public string stickerId;
    public int weight;
}

[System.Serializable]
public sealed class FairyRecordDto
{
    public string id;
    public string displayName;
    public string iconResourcePath;
    public string favoriteStickerText;
    public string flavorText;
    public PreferredStickerDto[] preferredStickers;
}
```

```csharp
public sealed class FairyStickerPreference
{
    public string StickerId { get; }
    public int Weight { get; }

    public FairyStickerPreference(string stickerId, int weight)
    {
        StickerId = stickerId;
        Weight = weight;
    }
}
```

```csharp
private StickerFairyAssignment ResolveFairyAssignment(StickerDefinition selectedSticker)
{
    if (selectedSticker == null || string.IsNullOrWhiteSpace(selectedSticker.Id))
    {
        return null;
    }

    if (UnityEngine.Random.value >= 0.5f)
    {
        return null;
    }

    FairyDefinition fairy = StickerFairySelector.Select(
        selectedSticker.Id,
        fairyCatalogSource.GetFairies());

    return fairy != null ? new StickerFairyAssignment(fairy) : null;
}
```
