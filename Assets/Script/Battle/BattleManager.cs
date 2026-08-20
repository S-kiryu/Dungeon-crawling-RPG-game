using System.Collections;
using UnityEngine;

/// <summary>
/// バトルの状態を管理するクラス
/// </summary>
public class BattleManager : MonoBehaviour
{
    [SerializeField]
    private EnemyTurnController _enemyTurnController;
    [SerializeField]
    private GridManager _gridManager;

    public BattleState CurrentState { get; private set; }
        = BattleState.SelectUnit;

    private bool _isChangingTurn;

    /// <summary>
    /// バトルの状態を変更する
    /// </summary>
    /// <param name="nextState"></param>
    public void ChangeState(BattleState nextState)
    {
        CurrentState = nextState;

        Debug.Log(
            $"BattleState changed: {CurrentState}"
        );
    }

    /// <summary>
    /// 敵のターンを開始する
    /// </summary>
    public void StartEnemyTurn()
    {
        if (_isChangingTurn)
            return;

        _gridManager.ClearBattleSelection();

        StartCoroutine(EnemyTurnRoutine());
    }

    /// <summary>
    /// 敵のターンを処理するコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemyTurnRoutine()
    {
        _isChangingTurn = true;

        ChangeState(BattleState.EnemyTurn);

        Debug.Log("敵ターン開始");

        if (_enemyTurnController != null)
        {
            yield return _enemyTurnController.ExecuteTurn();
        }
        else
        {
            // EnemyTurnControllerが未完成でも確認できる
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("敵ターン終了");

        ChangeState(BattleState.SelectUnit);

        _isChangingTurn = false;
    }

    /// <summary>
    /// 移動ボタンが押されたときの処理
    /// </summary>
    public void OnMoveButton()
    {
        if (CurrentState != BattleState.SetMove)
        {
            Debug.LogWarning(
                "先にキャラを選択してください"
            );

            return;
        }

        ChangeState(BattleState.SelectMoveTarget);
    }

    /// <summary>
    /// 攻撃ボタンが押されたときの処理
    /// </summary>
    public void OnAttackButton()
    {
        if (CurrentState !=
            BattleState.SelectAfterMoveCommand)
        {
            return;
        }

        ChangeState(BattleState.SelectAttackTarget);
    }

    /// <summary>
    /// 待機ボタンが押されたときの処理
    /// </summary>
    public void OnWaitButton()
    {
        if (CurrentState !=
                BattleState.SelectAfterMoveCommand &&
            CurrentState !=
                BattleState.SelectMoveTarget &&
            CurrentState !=
                BattleState.SelectAttackTarget)
        {
            return;
        }

        StartEnemyTurn();
    }
}