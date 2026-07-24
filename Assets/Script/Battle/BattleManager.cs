using UnityEngine;

/// <summary>
/// 戦闘時に各ステートを管理する
/// </summary>
public class BattleManager : MonoBehaviour
{
    public BattleState CurrentState { get; private set; } = BattleState.SelectUnit;

    public void ChangeState(BattleState nextState)
    {
        CurrentState = nextState;
        Debug.Log($"BattleState changed: {CurrentState}");
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void SelectMoveCommand()
    {
        ChangeState(BattleState.SelectMoveTarget);
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    public void SelectAttackCommand()
    {
        ChangeState(BattleState.SelectAttackTarget);
    }

    /// <summary>
    /// 待つ
    /// </summary>
    public void SelectWaitCommand()
    {
        Debug.Log("Wait selected");
        ChangeState(BattleState.EnemyTurn);
    }

    public void BackToCommand()
    {
        ChangeState(BattleState.SelectCommand);
    }
}