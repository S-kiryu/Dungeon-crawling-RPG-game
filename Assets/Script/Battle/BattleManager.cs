using System.Collections;
using System.Collections.Generic;
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
    [SerializeField]
    private UnitManager _unitManager;

    private readonly List<Unit> _turnOrder = new();

    private int _turnIndex = -1;

    public Unit CurrentTurnUnit { get; private set; }

    public int RoundCount { get; private set; }

    public BattleState CurrentState { get; private set; }
        = BattleState.PreparingTurn;

    private bool _isChangingTurn;

    private void Awake()
    {
        _unitManager.UnitsReady += BeginBattle;
    }

    /// <summary>
    /// バトルを開始する
    /// </summary>
    private void BeginBattle()
    {
        BuildTurnOrder();
        AdvanceTurn();
    }

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
    /// ターンを進める
    /// </summary>
    private void AdvanceTurn()
    {
        if (IsBattleFinished())
            return;

        do
        {
            _turnIndex++;

            if (_turnIndex >= _turnOrder.Count)
            {
                BuildTurnOrder();
                _turnIndex++;
            }

            CurrentTurnUnit = _turnOrder[_turnIndex];
        }
        while (CurrentTurnUnit == null ||
               CurrentTurnUnit.IsDead);

        if (CurrentTurnUnit.Team == TeamType.Player)
        {
            StartPlayerAction(CurrentTurnUnit);
        }
        else
        {
            StartCoroutine(
                EnemyActionRoutine(CurrentTurnUnit)
            );
        }
    }

    /// <summary>
    /// プレイヤーの行動を開始する
    /// </summary>
    /// <param name="unit"></param>
    private void StartPlayerAction(Unit unit)
    {
        _gridManager.PreparePlayerAction(unit);
        ChangeState(BattleState.SetMove);
    }

    /// <summary>
    /// 敵の行動を開始する
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private IEnumerator EnemyActionRoutine(Unit enemy)
    {
        ChangeState(BattleState.EnemyTurn);

        yield return _enemyTurnController.ExecuteAction(
            enemy
        );

        CompleteCurrentAction();
    }

    /// <summary>
    /// バトルが終了しているかどうかを判定する
    /// </summary>
    /// <returns></returns>
    private bool IsBattleFinished()
    {
        bool hasPlayer =
            _unitManager
                .GetLivingUnits(TeamType.Player)
                .Count > 0;

        bool hasEnemy =
            _unitManager
                .GetLivingUnits(TeamType.Enemy)
                .Count > 0;

        if (hasPlayer && hasEnemy)
            return false;

        CurrentTurnUnit = null;
        _gridManager.ClearBattleSelection();

        ChangeState(BattleState.BattleFinished);

        if (hasPlayer)
        {
            Debug.Log("プレイヤーの勝利");
        }
        else
        {
            Debug.Log("プレイヤーの敗北");
        }

        return true;
    }

    /// <summary>
    /// 現在の行動を完了する
    /// </summary>
    public void CompleteCurrentAction()
    {
        _gridManager.ClearBattleSelection();
        CurrentTurnUnit = null;

        AdvanceTurn();
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

        CompleteCurrentAction();
    }

    /// <summary>
    /// ターン順を構築する
    /// </summary>
    private void BuildTurnOrder()
    {
        _turnOrder.Clear();

        foreach (Unit unit in _unitManager.Units)
        {
            if (unit == null ||
                unit.IsDead ||
                unit.Team == TeamType.Neutral)
            {
                continue;
            }

            _turnOrder.Add(unit);
        }

        //素早さの降順でソートする
        _turnOrder.Sort((left, right) =>
        {
            return right.Status.Speed.CompareTo(
                left.Status.Speed
            );
        });

        _turnIndex = -1;
        RoundCount++;
    }
}