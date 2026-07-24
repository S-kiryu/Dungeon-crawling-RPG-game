using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public BattleState CurrentState { get; private set; } = BattleState.SelectUnit;

    public void ChangeState(BattleState nextState)
    {
        CurrentState = nextState;
        Debug.Log($"BattleState changed: {CurrentState}");
    }

    public void StartMoveSelect()
    {
        ChangeState(BattleState.SelectMoveTarget);
    }

    /// <summary>
    /// 味方を選んだ後に使うボタン
    /// </summary>
    public void OnMoveButton()
    {
        if (CurrentState != BattleState.SelectUnit)
            return;

        ChangeState(BattleState.SelectMoveTarget);
    }

    /// <summary>
    /// 攻撃したらターン終了
    /// </summary>
    public void OnAttackButton()
    {
        if (CurrentState != BattleState.SelectAfterMoveCommand)
            return;

        ChangeState(BattleState.SelectAttackTarget);
    }

    /// <summary>
    /// ターンを終了させたいとき
    /// </summary>
    public void OnWaitButton()
    {
        if (CurrentState != BattleState.SelectAfterMoveCommand && CurrentState != BattleState.SelectMoveTarget && CurrentState != BattleState.SelectAttackTarget)
            return;
        Debug.Log("敵のターンに移行");
        ChangeState(BattleState.EnemyTurn);
    }

}