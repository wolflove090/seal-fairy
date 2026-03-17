# UIワイヤー画面 作業手順書

## 目的
- 添付ワイヤーをもとに、`Assets/Main.unity` へ UI Toolkit の静的 HUD を追加する。
- 今回は見た目だけを作成し、クリック処理、画面遷移、値更新は実装しない。

## 完成イメージ
- 左上に `お金：XXX円` の表示パネルを置く。
- 右上に `準備完了` ボタンを置く。
- 右下に `妖精` ボタンと `ショップ` ボタンを縦並びで置く。
- 画面比率の基準は 1920x1080 の 16:9 とする。

## 事前準備
1. Unity 6000.3.3f1 でプロジェクトを開く。
2. シーン [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) を開く。
3. Game ビューの解像度を 1920x1080 相当の 16:9 に設定する。
4. 要件書 [UIワイヤー画面要件書.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/要件書/UIワイヤー画面要件書.md) を参照できる状態にする。

## 手順1: フォルダ作成
1. Project ウィンドウで `Assets` を開く。
2. `Assets/UI` フォルダを作成する。
3. `Assets/UI/UXML` フォルダを作成する。
4. `Assets/UI/USS` フォルダを作成する。
5. `Assets/UI/Settings` フォルダを作成する。

## 手順2: UXML 作成
1. `Assets/UI/UXML` で UI Document 用の UXML を新規作成する。
2. ファイル名は `HudScreen.uxml` とする。
3. UXML のルートに `root` を作成する。
4. `root` の直下に `top-bar` を作成する。
5. `top-bar` の左側に `money-panel` を作成する。
6. `money-panel` の中に `money-label` を作成し、文言を `お金：XXX円` にする。
7. `top-bar` の右側に `ready-button` を作成し、文言を `準備完了` にする。
8. `root` の直下に `bottom-right-menu` を作成する。
9. `bottom-right-menu` の中に `fairy-button` を作成し、文言を `妖精` にする。
10. `bottom-right-menu` の中に `shop-button` を作成し、文言を `ショップ` にする。
11. UXML に USS を参照させる設定を追加する。

## 手順3: USS 作成
1. `Assets/UI/USS` で USS を新規作成する。
2. ファイル名は `HudScreen.uss` とする。
3. `#root` に全画面表示用の設定を入れる。
4. 背景色を白に設定する。
5. `#top-bar` を上端固定の横並びにする。
6. `#top-bar` には左余白、右余白、上余白を設定する。
7. `#money-panel` は薄いグレー背景、横長サイズ、小さめ角丸にする。
8. `#money-label` は黒文字、大きめフォント、中央寄せ寄りの見た目に調整する。
9. `#ready-button` は右上固定で、薄いグレー背景、小さめ角丸、大きめ文字にする。
10. `#bottom-right-menu` は右下固定の縦並びにする。
11. `#fairy-button` と `#shop-button` は同じ幅と高さにする。
12. `#fairy-button` と `#shop-button` に薄いグレー背景、小さめ角丸、黒文字を設定する。
13. `#fairy-button` と `#shop-button` の間に十分な縦間隔を設定する。
14. 1920x1080 の見え方を基準に、右下メニューの余白を調整する。

## 手順4: Panel Settings 作成
1. `Assets/UI/Settings` で Panel Settings アセットを新規作成する。
2. ファイル名は `HudPanelSettings` とする。
3. 特別な描画要件がなければ標準設定のままでよい。

## 手順5: シーンへ組み込み
1. Hierarchy で HUD 表示用の空 GameObject を作成する。
2. GameObject 名は `HudDocument` とする。
3. `HudDocument` に `UIDocument` コンポーネントを追加する。
4. `UIDocument` の `Source Asset` に `HudScreen.uxml` を設定する。
5. `UIDocument` の `Panel Settings` に `HudPanelSettings` を設定する。
6. Play モードへ入らなくても Game ビューに HUD が表示されるか確認する。

## 手順6: レイアウト調整
1. 1920x1080 の Game ビューで、左上の所持金パネルがワイヤーどおり横長に見えるか確認する。
2. 右上の `準備完了` ボタンが上端に寄りすぎていないか確認する。
3. 右下の `妖精` と `ショップ` ボタンが同じ幅で縦並びになっているか確認する。
4. 各要素の背景色が薄いグレー、文字色が黒になっているか確認する。
5. 角丸が強すぎず、ワイヤーの矩形に近い見た目になっているか確認する。
6. 必要に応じて USS の余白、幅、高さ、フォントサイズを調整する。

## 手順7: 動作未実装の確認
1. `準備完了`、`妖精`、`ショップ` をクリックしても何も起きないことを確認する。
2. 金額表示が固定文字列のままであることを確認する。
3. C# スクリプトを追加していないことを確認する。
4. 既存の [PeelStickerDemoBootstrap.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelStickerDemoBootstrap.cs)、[TapStickerPlacer.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/TapStickerPlacer.cs)、[PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) に変更を入れていないことを確認する。

## 手順8: 追加確認
1. Game ビューの比率を一時的に変え、右下メニューが画面外へ出ないことを確認する。
2. 上段 UI が左右端に密着しすぎないことを確認する。
3. 文言がボタンやパネルからはみ出していないことを確認する。
4. カメラやワールドオブジェクトを動かしても UI が画面固定のままであることを確認する。

## 完了条件
- `Assets/UI/UXML/HudScreen.uxml` が存在する。
- `Assets/UI/USS/HudScreen.uss` が存在する。
- `Assets/UI/Settings/HudPanelSettings.asset` が存在する。
- [Main.unity](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Main.unity) に `UIDocument` が組み込まれている。
- 左上、右上、右下の 4 要素がワイヤーに近い位置で表示される。
- ボタン挙動、動的文言更新、画面遷移は未実装である。

## 補足
- 厳密なピクセル一致より、ワイヤーに対する相対位置と視認性を優先する。
- 将来スクリプト接続するため、要素名は `root`、`top-bar`、`money-panel`、`money-label`、`ready-button`、`bottom-right-menu`、`fairy-button`、`shop-button` を維持する。
