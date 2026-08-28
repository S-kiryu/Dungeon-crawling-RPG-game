/// <summary>
/// 各ターンの状態を管理するクラス
/// </summary>
public enum BattleState
{
    PreparingTurn,
    SelectCommand,
    SelectMoveTarget,
    Moving,
    SelectAttackTarget,
    Attacking,
    SelectSkillTarget,
    UsingSkill,
    EnemyTurn,
    BattleFinished
}
