# UIワイヤー画面 作業手順書

## 目的
- [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) に UI Toolkit の静的 HUD を追加する。
- 今回は UI の見た目だけを構築し、クリック処理、画面遷移、値更新、C# バインドは実装しない。
- この手順書だけで作業できるように、必要なコードと Editor 上の設定手順をすべて記載する。

## 完成イメージ
- 左上に `お金：XXX円` の表示パネルを配置する。
- 右上に `準備完了` ボタンを配置する。
- 右下に `妖精` ボタンと `ショップ` ボタンを縦並びで配置する。
- レイアウト基準は 1920x1080 の 16:9 とする。

## 作成するもの
- `Assets/UI/UXML/HudScreen.uxml`
- `Assets/UI/USS/HudScreen.uss`
- `Assets/UI/Settings/HudPanelSettings.asset`
- `Assets/Main.unity` に追加する `HudDocument` GameObject

## 事前準備
1. Unity 6000.3.3f1 でプロジェクトを開く。
2. シーン [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
3. Game ビューを 1920x1080 または同等の 16:9 に設定する。

## 手順1: フォルダ作成
1. Project ウィンドウで `Assets` を選ぶ。
2. `Assets/UI` フォルダを作成する。
3. `Assets/UI/UXML` フォルダを作成する。
4. `Assets/UI/USS` フォルダを作成する。
5. `Assets/UI/Settings` フォルダを作成する。

## 手順2: USS 作成
1. `Assets/UI/USS` で USS ファイルを新規作成する。
2. ファイル名を `HudScreen.uss` にする。
3. 既存内容をすべて削除し、以下をそのまま貼り付ける。

```css
#root {
    flex-grow: 1;
    width: 100%;
    height: 100%;
    background-color: rgb(255, 255, 255);
}

#top-bar {
    position: absolute;
    left: 32px;
    right: 32px;
    top: 20px;
    flex-direction: row;
    justify-content: space-between;
    align-items: flex-start;
}

#money-panel {
    width: 500px;
    min-height: 140px;
    padding-left: 28px;
    padding-right: 28px;
    padding-top: 20px;
    padding-bottom: 20px;
    background-color: rgb(217, 217, 217);
    border-top-left-radius: 12px;
    border-top-right-radius: 12px;
    border-bottom-left-radius: 12px;
    border-bottom-right-radius: 12px;
    justify-content: center;
}

#money-label {
    -unity-text-align: middle-left;
    font-size: 52px;
    color: rgb(0, 0, 0);
    white-space: nowrap;
}

#ready-button {
    width: 370px;
    min-height: 140px;
    background-color: rgb(217, 217, 217);
    color: rgb(0, 0, 0);
    font-size: 52px;
    border-top-left-radius: 12px;
    border-top-right-radius: 12px;
    border-bottom-left-radius: 12px;
    border-bottom-right-radius: 12px;
    border-left-width: 0;
    border-right-width: 0;
    border-top-width: 0;
    border-bottom-width: 0;
}

#bottom-right-menu {
    position: absolute;
    right: 32px;
    bottom: 36px;
    flex-direction: column;
    align-items: stretch;
}

#fairy-button,
#shop-button {
    width: 370px;
    min-height: 140px;
    margin-top: 0;
    margin-bottom: 24px;
    background-color: rgb(217, 217, 217);
    color: rgb(0, 0, 0);
    font-size: 52px;
    border-top-left-radius: 12px;
    border-top-right-radius: 12px;
    border-bottom-left-radius: 12px;
    border-bottom-right-radius: 12px;
    border-left-width: 0;
    border-right-width: 0;
    border-top-width: 0;
    border-bottom-width: 0;
}

#shop-button {
    margin-bottom: 0;
}
```

## 手順3: UXML 作成
1. `Assets/UI/UXML` で UXML ファイルを新規作成する。
2. ファイル名を `HudScreen.uxml` にする。
3. UI Builder を使う場合も、最終的に Text 表示へ切り替えて内容を確認する。
4. 既存内容をすべて削除し、以下をそのまま貼り付ける。

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/UI/USS/HudScreen.uss?fileID=7433441132597879392&amp;guid=0000000000000000e000000000000000&amp;type=3#HudScreen" />
    <ui:VisualElement name="root">
        <ui:VisualElement name="top-bar">
            <ui:VisualElement name="money-panel">
                <ui:Label name="money-label" text="お金：XXX円" />
            </ui:VisualElement>
            <ui:Button name="ready-button" text="準備完了" />
        </ui:VisualElement>
        <ui:VisualElement name="bottom-right-menu">
            <ui:Button name="fairy-button" text="妖精" />
            <ui:Button name="shop-button" text="ショップ" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

5. UXML 保存後、UI Builder 上で `StyleSheets` に `HudScreen.uss` を追加する。
6. 手順 4 のコード内にある `<Style ... />` の参照は guid が実ファイルと一致しないので、そのままでは使わない。
7. 保存後に UXML の先頭付近を開き、Unity が自動で正しい `HudScreen.uss` 参照へ置き換えたことを確認する。
8. もし自動で参照が入らない場合は、UI Builder の Inspector から `StyleSheets` に `HudScreen.uss` を追加して保存する。

## 手順4: Panel Settings 作成
1. `Assets/UI/Settings` を選ぶ。
2. 右クリックして `Create > UI Toolkit > Panel Settings Asset` を選ぶ。
3. ファイル名を `HudPanelSettings` にする。
4. 特別な要件がなければ初期設定のままでよい。

## 手順5: シーンへ組み込み
1. Hierarchy で右クリックして空 GameObject を作成する。
2. GameObject 名を `HudDocument` にする。
3. Inspector で `Add Component` を押し、`UIDocument` を追加する。
4. `UIDocument` の `Source Asset` に `HudScreen.uxml` を設定する。
5. `UIDocument` の `Panel Settings` に `HudPanelSettings` を設定する。
6. 追加のスクリプトは付けない。

## 手順6: 表示確認
1. Game ビューで左上に `お金：XXX円` が表示されることを確認する。
2. Game ビューで右上に `準備完了` ボタンが表示されることを確認する。
3. Game ビューで右下に `妖精` と `ショップ` ボタンが縦並びで表示されることを確認する。
4. 背景が白、パネルとボタンが薄いグレー、文字が黒になっていることを確認する。
5. 所持金パネルが右上ボタンより横長になっていることを確認する。
6. 3 つのボタンがタップしやすい大きさになっていることを確認する。

## 手順7: 微調整
1. 要素がワイヤーより小さく見える場合は `font-size`、`width`、`min-height` を少しずつ増やす。
2. 端に寄りすぎて見える場合は `#top-bar` の `left` `right` `top` と、`#bottom-right-menu` の `right` `bottom` を調整する。
3. 妖精ボタンとショップボタンの間隔を変える場合は `#fairy-button` の `margin-bottom` を調整する。
4. 所持金パネル内の文字位置を変える場合は `#money-panel` の `padding` と `#money-label` の `-unity-text-align` を調整する。

## 手順8: 動作未実装の確認
1. `準備完了`、`妖精`、`ショップ` をクリックしても何も起きないことを確認する。
2. `お金：XXX円` が固定表示のままであることを確認する。
3. C# スクリプトを追加していないことを確認する。
4. 既存の [PeelStickerDemoBootstrap.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelStickerDemoBootstrap.cs)、[TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs)、[PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) を変更していないことを確認する。

## 手順9: 解像度確認
1. 1920x1080 の 16:9 でワイヤーに近い位置関係になっていることを確認する。
2. 一時的に別の解像度比率へ切り替え、右下メニューが画面外へ出ないことを確認する。
3. 上段 UI が左右端に貼り付きすぎないことを確認する。
4. 文言がはみ出さないことを確認する。

## 完了条件
- `Assets/UI/UXML/HudScreen.uxml` が作成されている。
- `Assets/UI/USS/HudScreen.uss` が作成されている。
- `Assets/UI/Settings/HudPanelSettings.asset` が作成されている。
- [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) に `HudDocument` と `UIDocument` が追加されている。
- 左上、右上、右下の 4 要素がワイヤーに近い位置で表示される。
- ボタン挙動、画面遷移、動的更新は未実装のままである。

## 補足
- UXML の `Style` 参照は Unity が保存時に自動生成するため、コードを貼った直後の記述がそのまま残らない場合がある。重要なのは `HudScreen.uss` が StyleSheets に正しく登録されていること。
- 将来スクリプト接続するため、要素名は `root`、`top-bar`、`money-panel`、`money-label`、`ready-button`、`bottom-right-menu`、`fairy-button`、`shop-button` のまま維持する。
