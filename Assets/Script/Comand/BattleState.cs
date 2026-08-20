/// <summary>
/// 各ターンの状態を管理するクラス
/// </summary>
public enum BattleState
{
    SelectUnit,
    SetMove,
    SelectMoveTarget,
    Moving,
    SelectAfterMoveCommand,
    SelectAttackTarget,
    Attacking,
    EnemyTurn
}