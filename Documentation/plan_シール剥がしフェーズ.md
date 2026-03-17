# シール剥がしフェーズ 実装計画

## 実装方針
- 剥がしフェーズのコア機能は `PeelingPhaseDomainService` と関連ドメインモデルに集約し、Unity 依存はヒットテスト、演出、ログ出力、オブジェクト削除へ限定する。
- 既存の [PeelSticker3D.cs](/Users/tatsuki/Projects/Unity/SealFairy/Assets/Scripts/PeelSticker3D.cs) は見た目表現の参考とし、入力やゲーム状態は新規のフェーズ実装へ分離する。
- フェーズ完了時の未剥がしシール全削除は `GameFlowService` の遷移処理で一括実施する。
- 剥がしフェーズ固有の詳細は [シール剥がしフェーズ設計.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/設計書/シール剥がしフェーズ設計.md) に従う。

## 変更対象ファイル一覧

### 新規作成予定
- `Assets/Scripts/Domain/StickerStatus.cs`
- `Assets/Scripts/Domain/PeelingPhaseDomainService.cs`
- `Assets/Scripts/Domain/PeelingPhaseResult.cs`
- `Assets/Scripts/Domain/StickerLogMessageFactory.cs`
- `Assets/Scripts/Application/PeelingPhaseUseCase.cs`
- `Assets/Scripts/Application/ViewModels/PeelingViewModel.cs`
- `Assets/Scripts/Application/StickerRegistry.cs`
- `Assets/Scripts/Presentation/Interaction/StickerHitTestAdapter.cs`
- `Assets/Scripts/Presentation/Animation/StickerPeelPresenter.cs`
- `Assets/Scripts/Presentation/StickerCleanupPresenter.cs`
- `Assets/Scripts/Presentation/Logging/UnityDebugLogger.cs`
- `Assets/Scripts/Composition/SealFairyGameController.cs`

### 更新予定
- `Assets/Main.unity`
- `Assets/Scripts/PeelSticker3D.cs`
- `Assets/UI/GameHud.uxml`
- `Assets/UI/GameHud.uss`

## データフロー / 処理フロー
1. `StickerHitTestAdapter` がクリック対象シールの `StickerId` を返す。
2. `PeelingPhaseUseCase` が `PeelingPhaseDomainService.TryStartPeeling` を呼び出す。
3. ドメインが未処理シールなら `Peeling` 状態へ変更し、剥がし開始イベントを返す。
4. `StickerPeelPresenter` が対象シールの剥がし演出を再生する。
5. 演出完了時に `PeelingPhaseUseCase.CompletePeel` が呼ばれ、ドメインが `Removed` に確定する。
6. 妖精ありなら `StickerLogMessageFactory` がログ文言を作り、`UnityDebugLogger` が出力する。
7. `StickerCleanupPresenter` が表示オブジェクトを削除する。
8. 完了ボタン押下時に `GameFlowService` が残存シールを全削除し、配置フェーズへ戻す。

## レイヤ別責務

### ドメイン
- 剥がし開始可否判定、重複操作防止、削除確定、妖精発見ログ生成、一括削除対象抽出を行う。

### アプリケーション
- ヒットテスト結果と演出完了通知をドメイン処理へ変換する。
- ログ通知や表示削除要求を Presenter へ仲介する。

### Presentation / Infrastructure
- シール選択、剥がし演出、ログ出力、表示オブジェクト削除を担当する。

## リスクと対策
- 演出中の再クリックで状態不整合が起きる可能性がある。
  - `Peeling` 状態の再操作をドメインで拒否する。
- 剥がし演出完了と削除確定のタイミングがずれる可能性がある。
  - 演出完了コールバックのみを削除確定トリガーにする。
- フェーズ終了時に未剥がしシールが残る可能性がある。
  - `StickerCollection` から残存シールを列挙して一括削除する。

## 検証方針
- EditMode テストで、剥がし開始、重複剥がし拒否、妖精ログ生成、残存シール全削除を確認する。
- 手動確認で、クリックによる剥がし演出、剥がし後の消滅、妖精入りシールのみログ出力、完了ボタンによる全消去を確認する。

## コードスニペット
```csharp
public sealed class GameFlowService
{
    public GamePhase CurrentPhase { get; private set; } = GamePhase.SealPlacement;

    public TransitionResult CompleteCurrentPhase()
    {
        if (CurrentPhase == GamePhase.SealPlacement)
        {
            CurrentPhase = GamePhase.SealPeeling;
            return TransitionResult.ToPeeling();
        }

        var removedIds = stickerCollection.RemoveAllRemaining();
        CurrentPhase = GamePhase.SealPlacement;
        placementService.Reset(remainingCount: 10);
        return TransitionResult.ToPlacement(removedIds);
    }
}
```
