/// <summary>
/// 各ターンの状態を管理するクラス
/// </summary>
public enum BattleState
{
    PreparingTurn,
    SelectCommand,
    SelectTarget,
    ExecutingAction,
    EnemyTurn,
    BattleFinished
}
