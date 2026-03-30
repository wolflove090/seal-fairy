# シール一覧UIショップ準拠調整 作業手順書

## 目的
- HUD 左下の `シール一覧` パネルを、`シールショップ` と同系統の「濃いピンク背景 + 薄いピンク下敷き」構成へ変更する。
- 既存のシール一覧ロジック、スクロール、空表示、選択表示は維持する。
- 変更の主戦場を USS に限定し、必要な場合のみ UXML / Binder を追従修正する。

## 変更対象
- [Assets/UI/HubScreen/USS/HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss)
- [Assets/UI/HubScreen/UXML/HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml)
- [Assets/Scripts/HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs)
- 参照元: [Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss)

## 手順1: 参照元の見た目基準を確認する
1. [StickerShopScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/StickerShopScreen/USS/StickerShopScreen.uss) を開く。
2. `#sticker-shop-panel` の濃いピンク背景色、パディング、角丸感を確認する。
3. `#sticker-shop-content-frame` の薄いピンク背景色、パディング、角丸を確認する。
4. この 2 層構造を `シール一覧` へ移植する前提で値をメモする。

## 手順2: HudScreen.uxml の既存構造を確認する
1. [HudScreen.uxml](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/UXML/HudScreen.uxml) を開く。
2. `bottom-left-sticker-panel` 配下に `sticker-list-header` と `sticker-list-body` があることを確認する。
3. USS だけで対応できるかを先に判断する。
4. 追加要素が不要なら UXML は変更しない。

### 現状構造
```xml
<ui:VisualElement name="bottom-left-sticker-panel">
    <ui:VisualElement name="sticker-list-header">
        <ui:Label name="sticker-list-title" text="シール一覧" />
    </ui:VisualElement>
    <ui:VisualElement name="sticker-list-body">
        <ui:Label name="empty-sticker-list-label" text="所持シールがありません" />
        <ui:ScrollView name="sticker-scroll-view" mode="Vertical" />
    </ui:VisualElement>
</ui:VisualElement>
```

## 手順3: HudScreen.uss で外側コンテナを調整する
1. [HudScreen.uss](/Users/tatsuki/Projects/Unity/SealFairy/Assets/UI/HubScreen/USS/HudScreen.uss) の `#bottom-left-sticker-panel` を編集する。
2. パネル全体に濃いピンク背景を設定する。
3. ショップパネルと近い余白感になるよう、必要なら内側パディングを追加する。
4. 左下固定の位置とサイズは維持し、他 UI との当たりを変えない。

### 変更例
```css
#bottom-left-sticker-panel {
    position: absolute;
    left: 0;
    bottom: 0;
    width: 590px;
    height: 504px;
    padding: 18px;
    background-color: rgb(245, 127, 155);
    border-top-right-radius: 18px;
}
```

## 手順4: ヘッダーと下敷きの関係を調整する
1. `#sticker-list-header` はタイトル配置用として残す。
2. ヘッダーだけ別色の帯に見えないよう、背景色は透明または外側コンテナに馴染む色へ調整する。
3. `#sticker-list-title` は白文字を維持し、上下左右余白をショップ見出しに近づける。
4. `#sticker-list-body` に薄いピンク背景と十分な内側余白を設定する。

### 変更例
```css
#sticker-list-header {
    min-height: 72px;
    padding-left: 12px;
    padding-right: 12px;
    justify-content: center;
    background-color: rgba(0, 0, 0, 0);
}

#sticker-list-title {
    font-size: 48px;
    color: rgb(255, 255, 255);
    -unity-text-align: middle-left;
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

## 手順5: スクロール領域と空表示を整える
1. `#sticker-scroll-view` の `flex-grow: 1;` を維持する。
2. `#sticker-scroll-view .unity-scroll-view__content-container` の折り返し設定は維持する。
3. `#empty-sticker-list-label` は薄いピンク領域内で自然に見えるよう、色や余白を必要最小限だけ調整する。
4. 多件数時にセルが内側下敷きからはみ出さないことを Unity 上で確認する。

### 維持対象コード
```css
#sticker-scroll-view {
    flex-grow: 1;
    background-color: rgba(255, 255, 255, 0);
}

#sticker-scroll-view .unity-scroll-view__content-container {
    flex-direction: row;
    flex-wrap: wrap;
    align-content: flex-start;
}
```

## 手順6: 必要な場合だけセル色を微調整する
1. 背景変更後、`.sticker-cell` の白タイルと `.sticker-cell__count` の黒系バッジが見づらければ最小限だけ調整する。
2. `.sticker-cell--selected` は背景と埋もれない色差を確保する。
3. `CreateStickerCell()` の構造自体は変えない。

### 調整候補
```css
.sticker-cell {
    background-color: rgb(255, 255, 255);
}

.sticker-cell--selected {
    background-color: rgb(255, 244, 204);
    border-left-color: rgb(255, 140, 170);
    border-right-color: rgb(255, 140, 170);
    border-top-color: rgb(255, 140, 170);
    border-bottom-color: rgb(255, 140, 170);
}
```

## 手順7: UXML を変えた場合のみ Binder を同期する
1. [HudScreenBinder.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/HudScreenBinder.cs) を開く。
2. `bottom-left-sticker-panel`、`sticker-scroll-view`、`empty-sticker-list-label` の name を変えた場合のみクエリを更新する。
3. name を維持したなら C# は変更しない。

### 確認箇所
```csharp
stickerPanel = root.Q<VisualElement>("bottom-left-sticker-panel");
stickerScrollView = root.Q<ScrollView>("sticker-scroll-view");
emptyStickerListLabel = root.Q<Label>("empty-sticker-list-label");
```

## 手順8: 動作確認を行う
1. 通常 HUD で `シール一覧` 全体が濃いピンク外枠に見えることを確認する。
2. タイトル下に薄いピンクの下敷き領域が見えることを確認する。
3. 所持シール 0 件時に空表示が崩れないことを確認する。
4. 所持シール複数件時にスクロールできることを確認する。
5. シール選択時のハイライトが判別できることを確認する。
6. ショップ開閉とフェーズ切替後もパネルが崩れないことを確認する。

## 完了条件
- `シール一覧` が `シールショップ` と同系列の「濃いピンク背景 + 薄いピンク下敷き」構成になっている。
- 既存のスクロール、空表示、選択表示、所持数表示が維持されている。
- UXML を変更した場合でも `HudScreenBinder` の要素取得エラーが発生しない。
