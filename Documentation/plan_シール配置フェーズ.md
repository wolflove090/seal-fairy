# シール配置フェーズ 実装計画

## 実装方針
- 配置フェーズのコア機能は Unity 非依存のドメイン層へ置き、`MonoBehaviour` は入力取得、座標変換、プレビュー表示、UI 反映のみに使う。
- 残数 10、重なり許可、妖精出現率 50% のルールは `PlacementPhaseDomainService` に閉じ込める。
- マウスとタッチは `PointerInputAdapter` で統一し、WebGL で差分が出ない構成にする。
- 配置フェーズ固有の詳細は [シール配置フェーズ設計.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/設計書/シール配置フェーズ設計.md) に従う。

## 変更対象ファイル一覧

### 新規作成予定
- `Assets/Scripts/Domain/GamePhase.cs`
- `Assets/Scripts/Domain/StickerId.cs`
- `Assets/Scripts/Domain/WorldPoint.cs`
- `Assets/Scripts/Domain/PlacedSticker.cs`
- `Assets/Scripts/Domain/StickerCollection.cs`
- `Assets/Scripts/Domain/PlacementPhaseState.cs`
- `Assets/Scripts/Domain/PlacementPhaseDomainService.cs`
- `Assets/Scripts/Application/GameFlowService.cs`
- `Assets/Scripts/Application/PlacementPhaseUseCase.cs`
- `Assets/Scripts/Application/ViewModels/PlacementViewModel.cs`
- `Assets/Scripts/Infrastructure/Random/UnityRandomService.cs`
- `Assets/Scripts/Presentation/Input/PointerInputAdapter.cs`
- `Assets/Scripts/Presentation/World/WorldPlaneRaycaster.cs`
- `Assets/Scripts/Presentation/PlacementPreviewPresenter.cs`
- `Assets/Scripts/Presentation/StickerSpawner.cs`
- `Assets/Scripts/Presentation/UI/GameHudPresenter.cs`
- `Assets/Scripts/Composition/SealFairyGameController.cs`
- `Assets/UI/GameHud.uxml`
- `Assets/UI/GameHud.uss`

### 更新予定
- `Assets/Main.unity`
- `Assets/Scripts/PeelStickerDemoBootstrap.cs`

## データフロー / 処理フロー
1. `PointerInputAdapter` がマウス / タッチ入力を `PointerSnapshot` に変換する。
2. `WorldPlaneRaycaster` がポインタ位置を配置平面上の `WorldPoint` に変換する。
3. `PlacementPhaseUseCase` が現在フェーズを確認し、`PlacementPhaseDomainService` へプレビュー更新または配置要求を渡す。
4. ドメインが残数確認、妖精判定、`PlacedSticker` 生成、コレクション更新を行う。
5. `StickerSpawner` が配置済みシールの表示オブジェクトを生成する。
6. `PlacementPreviewPresenter` がプレビューシールの追従表示を行う。
7. `GameHudPresenter` が残数表示と完了ボタン文言を更新する。
8. 完了ボタン押下時に `GameFlowService` が `SealPeeling` へ遷移する。

## レイヤ別責務

### ドメイン
- 残数、プレビュー座標、配置済みシール、妖精有無を保持する。
- 配置可能判定、残数減算、配置イベント生成を行う。

### アプリケーション
- ポインタ入力とフェーズ状態をドメイン呼び出しへ変換する。
- ViewModel を組み立てて Presenter に渡す。

### Presentation / Infrastructure
- 入力、ワールド変換、プレビュー表示、シール生成、UI Toolkit 反映を担当する。

## リスクと対策
- UI 上のクリックと配置入力が競合する可能性がある。
  - `PointerInputAdapter` で UI ヒット中の配置要求を抑止する。
- 配置残数 0 のときに挙動が分かりにくくなる可能性がある。
  - 残数表示と完了ボタン文言で状態を明示する。
- 配置済みデータと表示オブジェクトがずれる可能性がある。
  - `StickerId` をキーに一対一対応させる。

## 検証方針
- EditMode テストで、残数減算、残数 0 での配置拒否、妖精割当、重なり許可を確認する。
- 手動確認で、プレビュー追従、クリック配置、10 枚上限、全フェーズ共通のカメラ移動、HUD 更新を確認する。

## コードスニペット
```csharp
public sealed class PlacementPhaseDomainService
{
    private readonly StickerCollection stickers;
    private readonly IStickerRandomService randomService;
    private PlacementPhaseState state;

    public PlacementPhaseResult TryPlace(WorldPoint point)
    {
        if (state.RemainingCount <= 0)
        {
            return PlacementPhaseResult.Rejected(state);
        }

        var sticker = PlacedSticker.Create(point, randomService.ContainsFairy(0.5f));
        stickers.Add(sticker);
        state = state.WithRemainingCount(state.RemainingCount - 1)
                     .WithPreviewPoint(point);
        return PlacementPhaseResult.Created(sticker, state);
    }
}
```
