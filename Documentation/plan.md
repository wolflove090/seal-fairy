# シール配置と剥がし機能 実装計画

## 全体方針
- ゲームのコア機能は Unity 非依存のドメイン層として分離し、`MonoBehaviour` は入力、表示、UI バインドのアダプタに限定する。
- 詳細な実装計画はフェーズ単位に分割し、配置フェーズと剥がしフェーズで別文書として管理する。

## フェーズ別計画
- 配置フェーズ: [plan_シール配置フェーズ.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/plan_シール配置フェーズ.md)
- 剥がしフェーズ: [plan_シール剥がしフェーズ.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/plan_シール剥がしフェーズ.md)

## 関連設計書
- [シール配置フェーズ設計.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/設計書/シール配置フェーズ設計.md)
- [シール剥がしフェーズ設計.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/設計書/シール剥がしフェーズ設計.md)
