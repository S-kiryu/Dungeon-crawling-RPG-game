/// <summary>
/// 各ターンの状態を管理するクラス
/// </summary>
public enum BattleState
{
    PreparingTurn,
    SetMove,
    SelectMoveTarget,
    Moving,
    SelectAfterMoveCommand,
    SelectAttackTarget,
    Attacking,
    EnemyTurn,
    BattleFinished
}