using System;
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

    private bool _hasMoved;
    private bool _hasUsedMainAction;
    private bool _battleEnded;

    public Unit CurrentTurnUnit { get; private set; }

    public int RoundCount { get; private set; }

    public BattleState CurrentState { get; private set; }
        = BattleState.PreparingTurn;

    public bool CanMove =>
        IsPlayerTurn &&
        !_hasMoved &&
        !_hasUsedMainAction &&
        CurrentState == BattleState.SelectCommand;

    public bool CanAttack =>
        IsPlayerTurn &&
        !_hasUsedMainAction &&
        CurrentState == BattleState.SelectCommand;

    public bool CanUseSkill =>
        IsPlayerTurn &&
        !_hasUsedMainAction &&
        CurrentState == BattleState.SelectCommand;

    public bool CanWait =>
        IsPlayerTurn &&
        CurrentState != BattleState.Moving &&
        CurrentState != BattleState.Attacking &&
        CurrentState != BattleState.UsingSkill;

    private bool IsPlayerTurn =>
        CurrentTurnUnit != null &&
        CurrentTurnUnit.Team == TeamType.Player;

    public event Action<bool> BattleEnded;

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
        _hasMoved = false;
        _hasUsedMainAction = false;

        _gridManager.PreparePlayerAction(unit);
        ChangeState(BattleState.SelectCommand);
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

        if (_battleEnded)
            return true;

        _battleEnded = true;

        CurrentTurnUnit = null;

        _gridManager.ClearBattleSelection();

        ChangeState(
            BattleState.BattleFinished);

        if (hasPlayer)
            Debug.Log("プレイヤーの勝利");
        else
            Debug.Log("プレイヤーの敗北");

        BattleEnded?.Invoke(hasPlayer);

        return true;
    }

    /// <summary>
    /// 現在の行動を終了する
    /// </summary>
    public void CompleteCurrentAction()
    {
        _gridManager.ClearBattleSelection();
        CurrentTurnUnit = null;

        _hasMoved = false;
        _hasUsedMainAction = false;

        AdvanceTurn();
    }

    /// <summary>
    /// GridManagerからクリックされたマスを受け取る
    /// </summary>
    public void OnCellClicked(GridCell clickedCell)
    {
        if (!IsPlayerTurn || clickedCell == null)
            return;

        switch (CurrentState)
        {
            case BattleState.SelectMoveTarget:
                TryExecuteMove(clickedCell);
                break;

            case BattleState.SelectAttackTarget:
                TryExecuteAttack(clickedCell);
                break;

            case BattleState.SelectSkillTarget:
                TryExecuteSkill(clickedCell);
                break;
        }
    }

    /// <summary>
    /// 移動ボタンが押されたときの処理
    /// </summary>
    public void OnMoveButton()
    {
        if (!CanMove)
            return;

        ChangeState(BattleState.SelectMoveTarget);

        _gridManager.ShowMovementRange(
            CurrentTurnUnit
        );
    }

    /// <summary>
    /// 攻撃ボタンが押されたときの処理
    /// </summary>
    public void OnAttackButton()
    {
        if (!CanAttack)
            return;

        ChangeState(BattleState.SelectAttackTarget);

        _gridManager.ShowAttackRange(
            CurrentTurnUnit
        );
    }

    /// <summary>
    /// スキルボタンが押されたときの処理
    /// </summary>
    public void OnSkillButton()
    {
        if (!CanUseSkill)
            return;

        ChangeState(BattleState.SelectSkillTarget);
    }

    /// <summary>
    /// 待機ボタンが押されたときの処理
    /// </summary>
    public void OnWaitButton()
    {
        if (!CanWait)
            return;

        CompleteCurrentAction();
    }

    /// <summary>
    /// そこに移動できるかどうか判定する
    /// </summary>
    /// <param name="destination"></param>
    private void TryExecuteMove(GridCell destination)
    {
        if (!IsPlayerTurn ||
            _hasMoved ||
            _hasUsedMainAction ||
            CurrentState != BattleState.SelectMoveTarget)
        {
            return;
        }

        Unit movingUnit = CurrentTurnUnit;
        GridCell previousCell = movingUnit.CurrentCell;

        bool startedMoving = _gridManager.TryMoveUnit(
            movingUnit,
            destination.Position,
            () =>
            {
                _hasMoved = true;

                _gridManager.PreparePlayerAction(
                    movingUnit
                );

                ChangeState(BattleState.SelectCommand);
            }
        );

        if (!startedMoving)
            return;

        _gridManager.RestoreCellMaterial(
            previousCell
        );

        ChangeState(BattleState.Moving);
    }

    private void TryExecuteAttack(GridCell clickedCell)
    {
        if (!IsPlayerTurn ||
            _hasUsedMainAction ||
            CurrentState != BattleState.SelectAttackTarget ||
            !clickedCell.IsOccupied)
        {
            return;
        }

        Unit attacker = CurrentTurnUnit;
        Unit target = clickedCell.CurrentUnit;

        if (target == null ||
            target.Team != TeamType.Enemy ||
            !_gridManager.IsInActionRange(
                attacker,
                clickedCell))
        {
            return;
        }

        ChangeState(BattleState.Attacking);

        target.TakeDamage(
    attacker.Status.Attack);

        if (IsBattleFinished())
            return;

        _hasUsedMainAction = true;

        _gridManager.PreparePlayerAction(
            attacker
        );

        ChangeState(BattleState.SelectCommand);
    }

    private void TryExecuteSkill(GridCell clickedCell)
    {
        if (!IsPlayerTurn ||
            _hasUsedMainAction ||
            CurrentState != BattleState.SelectSkillTarget)
        {
            return;
        }

        // TODO: 選択中のスキル効果を適用する。
        _hasUsedMainAction = true;

        _gridManager.PreparePlayerAction(
            CurrentTurnUnit
        );

        ChangeState(BattleState.SelectCommand);
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
