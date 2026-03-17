# シール配置と剥がし機能 ToDo

## フェーズ別 ToDo
- 配置フェーズ: [todo_シール配置フェーズ.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/todo_シール配置フェーズ.md)
- 剥がしフェーズ: [todo_シール剥がしフェーズ.md](/Users/tatsuki/Projects/Unity/SealFairy/Documentation/todo_シール剥がしフェーズ.md)

## 共通前提
- ゲームコアはドメイン層へ置き、Unity 依存は Presenter / Adapter へ隔離する。
- フェーズ間の遷移制御は `GameFlowService` と `SealFairyGameController` に集約する。
