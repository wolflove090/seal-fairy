# シール一覧UIショップ準拠調整 実装計画

## 実装方針
- 対象は HUD 左下の `シール一覧` パネルに限定し、既存の選択ロジック、所持数更新、ショップ機能には手を入れない。
- 実装の主変更点は [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) に集約し、可能な限り UXML 要素名と [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の参照を維持する。
- 見た目の基準は [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) の `#sticker-shop-panel` と `#sticker-shop-content-frame` とし、濃いピンク外側コンテナと薄いピンク内側コンテンツの関係を左下パネルへ移植する。
- `HudScreen.uxml` は現在すでに `bottom-left-sticker-panel` 配下に `sticker-list-header` と `sticker-list-body` を持っているため、基本は構造維持で対応する。必要な場合のみラッパーや余白用要素を追加する。
- `BuildStickerList()` と `CreateStickerCell()` の責務は維持し、今回の変更で C# 側に見た目定数を追加しない。

## 変更対象ファイル一覧
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
  - `bottom-left-sticker-panel`、`sticker-list-header`、`sticker-list-body` の背景構造、余白、角丸を調整する。
  - 必要に応じて `sticker-scroll-view`、空表示ラベル、シールセルの見え方を微調整する。
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
  - USS だけで対応できない場合に限り、内側下敷きのレイアウト安定化用の要素を追加する。
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
  - UXML 構造を変えた場合のみ参照名・取得処理を同期する。USS のみで済むなら変更しない。

## データフロー / 処理フロー
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) の `OnEnable()` が `bottom-left-sticker-panel`、`sticker-scroll-view`、`empty-sticker-list-label` を取得する。
2. `BuildStickerList()` が `OwnedStickerInventorySource.GetOwnedStickers()` を読み、`sticker-scroll-view` に動的セルを並べる。
3. 今回は 1 と 2 の処理は変えず、UI Toolkit のスタイルだけでパネル外観をショップ準拠へ寄せる。
4. シール未所持時は `empty-sticker-list-label` が表示され、薄いピンクの内側領域に収まったまま空状態を示す。
5. シール選択時は既存の `.sticker-cell--selected` が適用され、背景変更後も選択が判別できるよう USS を再調整する。

## 処理フロー詳細

### 1. ショップ配色の参照元整理
- [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss#L23) の `#sticker-shop-panel` を濃いピンク背景の基準とする。
- [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss#L85) の `#sticker-shop-content-frame` を薄いピンク下敷きの基準とする。
- `シール一覧` パネルで使う色は完全一致または近似値のどちらかに揃え、少なくとも同一シリーズに見える組み合わせへ調整する。

### 2. HUD パネル構造の見直し
- [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml#L13) の `bottom-left-sticker-panel` 全体に濃いピンク背景を持たせる。
- `sticker-list-header` はタイトル配置用として残しつつ、ヘッダーだけ別色に見えないよう背景色や余白関係を調整する。
- `sticker-list-body` に薄いピンク背景、内側余白、下側角丸を与え、ショップの `content-frame` と同じ役割にする。
- パネル全体にパディングが必要なら `bottom-left-sticker-panel` に付け、`body` 側の余白と二重で過剰にならないよう整理する。

### 3. スクロール領域と空表示の整合
- `sticker-scroll-view` は `sticker-list-body` の中で `flex-grow: 1;` を維持し、一覧件数増加時も内側下敷きの範囲でスクロールする。
- `empty-sticker-list-label` は薄いピンク下敷き内で中央寄せに見えるよう必要最低限の余白調整を行う。
- `sticker-scroll-view .unity-scroll-view__content-container` の折り返し設定は維持し、背景構造変更でセル配置が崩れないことを確認する。

### 4. シールセルの視認性確保
- 背景変更後に `.sticker-cell` の白タイル、`.sticker-cell__count` の黒系バッジ、`.sticker-cell--selected` の選択色が埋もれないか確認する。
- 必要なら `.sticker-cell` の背景色、枠色、選択色のみ微修正し、セル構造や `CreateStickerCell()` の生成内容は変えない。
- `sticker-cell__image` の表示ルールは維持し、画像未設定時もレイアウトが壊れないことを前提にする。

### 5. Binder 影響確認
- [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs#L59) 付近の要素取得名は、UXML の name を変えない限り変更不要とする。
- もし内側下敷き用の新要素追加で取得対象が増えないなら、C# 修正は行わない。
- UXML を変更した場合のみ、`OnEnable()` の null チェック対象と参照名を同期更新する。

## リスクと対策
- `bottom-left-sticker-panel` に濃いピンク背景を直接付けると、`sticker-list-header` と `sticker-list-body` の角丸が二重に見える可能性がある。
  - 外側コンテナと内側下敷きの角丸役割を分け、どちらが外周を担当するかを先に決める。
- 薄いピンク下敷きの余白が不足すると、スクロール領域が外枠に貼り付きショップらしさが出ない。
  - `sticker-list-body` の padding を基準値として、`StickerShopScreen.uss` の `24px` 前後に寄せて比較する。
- 背景色変更により選択状態が目立たなくなる可能性がある。
  - `.sticker-cell--selected` の背景色または枠色を調整し、通常セルとの差分を維持する。
- UXML を触る場合、`HudScreenBinder` の要素取得失敗で `シール一覧 UI が見つかりません` エラーが出る可能性がある。
  - name 属性は原則維持し、変更が必要な場合は Binder の参照更新を同時に行う。

## 検証方針
- 手動確認1: 通常 HUD 表示時、左下 `シール一覧` パネル全体が濃いピンクの外側コンテナに見える。
- 手動確認2: タイトル下の一覧領域が薄いピンクの下敷きとして表示され、ショップの `content-frame` と同系列に見える。
- 手動確認3: 所持シール 0 件時でも空表示が薄いピンク領域内に収まり、背景構造が崩れない。
- 手動確認4: 所持シール複数件時にスクロールが正常動作し、セルが外枠からはみ出さない。
- 手動確認5: シール選択時のハイライトと所持数バッジが背景に埋もれず視認できる。
- 手動確認6: ショップ開閉、フェーズ切替、所持シール更新後も `シール一覧` パネルが崩れない。

## コードスニペット
```css
#bottom-left-sticker-panel {
    position: absolute;
    left: 0;
    bottom: 0;
    width: 590px;
    height: 504px;
    padding: 18px 18px 18px 18px;
    background-color: rgb(245, 127, 155);
    border-top-right-radius: 18px;
}

#sticker-list-header {
    min-height: 72px;
    padding-left: 12px;
    padding-right: 12px;
    justify-content: center;
    background-color: rgba(0, 0, 0, 0);
}

#sticker-list-body {
    flex-grow: 1;
    padding: 24px;
    background-color: rgb(255, 218, 226);
    border-top-left-radius: 18px;
    border-top-right-radius: 18px;
    border-bottom-left-radius: 18px;
    border-bottom-right-radius: 18px;
}
```

```csharp
private void OnEnable()
{
    root = uiDocument.rootVisualElement;
    stickerPanel = root.Q<VisualElement>("bottom-left-sticker-panel");
    stickerScrollView = root.Q<ScrollView>("sticker-scroll-view");
    emptyStickerListLabel = root.Q<Label>("empty-sticker-list-label");

    if (stickerPanel == null || stickerScrollView == null)
    {
        Debug.LogError("シール一覧 UI が見つかりません");
        return;
    }
}
```
