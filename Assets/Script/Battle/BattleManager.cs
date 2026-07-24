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
    public void OnMoveButton()
    {
        if (CurrentState != BattleState.SelectBeforeMoveCommand)
            return;

        ChangeState(BattleState.SelectMoveTarget);
    }

    public void OnAttackButton()
    {
        if (CurrentState != BattleState.SelectAfterMoveCommand)
            return;

        ChangeState(BattleState.SelectAttackTarget);
    }

    public void OnWaitButton()
    {
        if (CurrentState != BattleState.SelectAfterMoveCommand)
            return;

        ChangeState(BattleState.EnemyTurn);
    }

}