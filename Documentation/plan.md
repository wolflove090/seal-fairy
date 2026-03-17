# UIワイヤー画面 実装計画

## 実装方針
- 対象は HUD の静的 UI のみとし、ゲームロジックやイベント接続は行わない。
- UI は UI Toolkit で構成し、`UIDocument` + `Panel Settings` + `UXML` + `USS` の標準構成を採用する。
- 既存の `Assets/Scripts/` 配下のゲームロジックには手を入れず、シーン `Assets/Main.unity` に画面固定 UI を追加する。
- レイアウト基準は 1920x1080 の 16:9 とし、他解像度でも右上・左上・右下固定が崩れない構造にする。
- UI 要素は将来のバインドを想定し、`name` を要件書どおりに付与して後続の C# 実装を容易にする。

## 変更対象ファイル一覧
- 新規想定: `Assets/UI/UXML/HudScreen.uxml`
  - HUD 全体の階層定義を持つ。
- 新規想定: `Assets/UI/USS/HudScreen.uss`
  - パネル色、サイズ、余白、右下縦積み、文字サイズを定義する。
- 新規想定: `Assets/UI/Settings/HudPanelSettings.asset`
  - UI Toolkit の描画設定を保持する。
- 変更想定: `Assets/Main.unity`
  - `UIDocument` を持つ HUD 用 GameObject を追加し、UXML と Panel Settings を参照させる。
- 参照のみ: `Documentation/要件書/UIワイヤー画面要件書.md`
  - 命名、スコープ、受け入れ条件の基準として使う。

## UI構成
- ルート `root`
  - 全画面ストレッチの最上位要素。
- 上段コンテナ `top-bar`
  - 左右端揃えの横並びコンテナ。
- 左上表示 `money-panel`
  - 内部に `money-label` を配置し、`お金：XXX円` を表示する。
- 右上操作 `ready-button`
  - `準備完了` を表示するボタン。
- 右下メニュー `bottom-right-menu`
  - 画面右下に固定する縦並びコンテナ。
- 右下上段 `fairy-button`
  - `妖精` を表示するボタン。
- 右下下段 `shop-button`
  - `ショップ` を表示するボタン。

## レイアウト/スタイル方針
- ルートは `width: 100%` と `height: 100%` 相当の全画面構成にする。
- `top-bar` は上端固定の横並びにし、左右へ余白を持たせる。
- `bottom-right-menu` は絶対配置または同等の固定配置で右下へ寄せる。
- 背景は白、パネルとボタンは薄いグレー、文字色は黒で統一する。
- 角丸は小さめにし、ワイヤーの印象に寄せる。
- 所持金パネルは他要素より横長に設定する。
- `ready-button` と `bottom-right-menu` 配下の 2 ボタンはタップしやすい高さと幅を持たせる。
- ボタン間隔は USS のマージンで調整し、妖精ボタンとショップボタンの見た目を揃える。

## 処理フロー
1. ユーザーが `Assets/UI` 配下に UXML / USS / Settings のフォルダを作成する。
2. UXML で要件書に沿った名前付き要素を定義する。
3. USS で 1920x1080 基準の見た目と固定配置を調整する。
4. Panel Settings アセットを作成する。
5. `Assets/Main.unity` に HUD 用 GameObject を追加する。
6. `UIDocument` を付与し、UXML と Panel Settings を設定する。
7. Game ビューで 1920x1080 を選び、ワイヤーどおりの位置関係か確認する。
8. 追加で縦横比を変えて、右下メニューが画面外に出ないことを確認する。

## 既存コードとの関係
- `Assets/Scripts/PeelStickerDemoBootstrap.cs`
  - カメラとワールドオブジェクト生成のみを担当しており、今回の UI 実装では変更不要。
- `Assets/Scripts/TapStickerPlacer.cs`
  - 配置入力ロジックを持つが、今回はイベント接続しないため変更不要。
- `Assets/Scripts/PeelSticker3D.cs`
  - 剥がし挙動を持つが、今回は UI 表示のみのため変更不要。

## リスクと対策
- `UIDocument` をシーンへ追加しても表示されない可能性がある。
  - `Panel Settings` の未設定、または `Visual Tree Asset` 未割り当てを確認項目に含める。
- 解像度差で余白が極端に崩れる可能性がある。
  - 1920x1080 を基準にしつつ、他比率でも固定端からの距離で保持する USS 構成にする。
- 将来のスクリプト接続時に要素参照しづらくなる可能性がある。
  - `name` を要件書どおり固定し、役割ごとの class を分離する。
- ワイヤーの寸法が厳密でないため見た目差分が出る可能性がある。
  - ピクセル一致ではなく、相対位置と視認性を優先した調整方針を採る。

## 検証方針
- `Assets/Main.unity` を開いた Game ビューで HUD の 4 要素が表示されることを確認する。
- 1920x1080 で左上・右上・右下の配置がワイヤー準拠であることを確認する。
- カメラ移動やワールド表示の影響を受けず、UI が画面固定であることを確認する。
- ボタン押下で何も起きないことを確認し、挙動未実装を維持する。

## コードスニペット
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
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

```css
#root {
    flex-grow: 1;
    background-color: rgb(255, 255, 255);
}

#top-bar {
    position: absolute;
    top: 24px;
    left: 32px;
    right: 32px;
    flex-direction: row;
    justify-content: space-between;
}

#bottom-right-menu {
    position: absolute;
    right: 32px;
    bottom: 24px;
}
```
