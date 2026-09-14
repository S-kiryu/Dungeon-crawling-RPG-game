using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バトルのターン進行と、現在選択中のアクションを管理する。
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
    private bool _battleEnded;

    private BattleTurnContext _turnContext;
    private IBattleAction _selectedAction;

    public Unit CurrentTurnUnit { get; private set; }
    public int RoundCount { get; private set; }

    public BattleState CurrentState { get; private set; }
        = BattleState.PreparingTurn;

    public TurnActionSet CurrentTurnActions =>
        _turnContext?.Actions;

    public TurnActionSlots CurrentTurnSlots =>
        _turnContext?.Slots;

    public IBattleAction SelectedAction =>
        _selectedAction;

    public bool CanMove =>
        CanSelectAction(BattleActionIds.Move);

    public bool CanAttack =>
        CanSelectAction(BattleActionIds.Attack);

    public bool CanUseSkill =>
        CanSelectAction(BattleActionIds.Skill);

    public bool CanWait =>
        IsPlayerTurn &&
        _turnContext != null &&
        CurrentState != BattleState.ExecutingAction &&
        CurrentState != BattleState.BattleFinished &&
        GetActionAvailability(
            BattleActionIds.Wait).CanExecute;

    private bool IsPlayerTurn =>
        CurrentTurnUnit != null &&
        CurrentTurnUnit.Team == TeamType.Player;

    public event Action<bool> BattleEnded;

    /// <summary>
    /// アクションの追加・削除、残り枠、状態のいずれかが変化した。
    /// コマンドUIの再描画に使用できる。
    /// </summary>
    public event Action TurnActionsChanged;

    private void Awake()
    {
        _unitManager.UnitsReady += BeginBattle;
    }

    private void OnDestroy()
    {
        if (_unitManager != null)
        {
            _unitManager.UnitsReady -= BeginBattle;
        }

        ReleaseTurnContext();
    }

    /// <summary>
    /// バトル開始
    /// </summary>
    private void BeginBattle()
    {
        BuildTurnOrder();
        AdvanceTurn();
    }

    /// <summary>
    /// バトルの状態を変更する。状態が変化した場合
    /// </summary>
    /// <param name="nextState"></param>
    private void ChangeState(BattleState nextState)
    {
        CurrentState = nextState;

        Debug.Log(
            $"BattleState changed: {CurrentState}");

        TurnActionsChanged?.Invoke();
    }

    /// <summary>
    /// ターンを進める。現在のターンが終了した場合、次のユニットのターンへ移行する。
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
                EnemyActionRoutine(CurrentTurnUnit));
        }
    }

    /// <summary>
    /// プレイヤーターン用の行動枠とアクション一覧を構築する。
    /// </summary>
    private void StartPlayerAction(Unit unit)
    {
        ReleaseTurnContext();

        TurnActionSlots slots = new();
        slots.Set(ActionSlot.Movement, 1);
        slots.Set(ActionSlot.Main, 1);

        TurnActionSet actions = new();
        actions.Add(new MoveBattleAction());
        actions.Add(new AttackBattleAction());
        actions.Add(new SkillBattleAction());
        actions.Add(new WaitBattleAction());

        _turnContext = new BattleTurnContext(
            unit,
            _gridManager,
            slots,
            actions);

        actions.Changed += HandleTurnConfigurationChanged;
        slots.Changed += HandleTurnConfigurationChanged;

        ApplyTurnContributors(unit);

        _gridManager.PreparePlayerAction(unit);
        ChangeState(BattleState.SelectCommand);
    }

    /// <summary>
    /// Unitへ追加された装備・バフ等から、そのターンの構成を変更する。
    /// </summary>
    private void ApplyTurnContributors(Unit unit)
    {
        MonoBehaviour[] components =
            unit.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is ITurnActionContributor contributor)
            {
                contributor.ConfigureTurn(_turnContext);
            }
        }
    }

    /// <summary>
    /// 敵ターンの行動を実行する。
    /// </summary>
    /// <param name="enemy">行動を実行する敵ユニット</param>
    /// <returns>コルーチン</returns>
    private IEnumerator EnemyActionRoutine(Unit enemy)
    {
        ChangeState(BattleState.EnemyTurn);

        yield return _enemyTurnController.ExecuteAction(enemy);

        CompleteCurrentAction();
    }

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

        ReleaseTurnContext();
        CurrentTurnUnit = null;

        _gridManager.ClearBattleSelection();
        ChangeState(BattleState.BattleFinished);

        if (hasPlayer)
            Debug.Log("プレイヤーの勝利");
        else
            Debug.Log("プレイヤーの敗北");

        BattleEnded?.Invoke(hasPlayer);

        return true;
    }

    /// <summary>
    /// 現在のユニットのターンを終了する。
    /// </summary>
    public void CompleteCurrentAction()
    {
        _gridManager.ClearBattleSelection();

        ReleaseTurnContext();
        CurrentTurnUnit = null;

        AdvanceTurn();
    }

    public void OnCellClicked(GridCell clickedCell)
    {
        if (!IsPlayerTurn ||
            clickedCell == null ||
            CurrentState != BattleState.SelectTarget ||
            _selectedAction == null)
        {
            return;
        }

        ExecuteSelectedAction(clickedCell);
    }

    public void OnMoveButton()
    {
        TrySelectAction(BattleActionIds.Move);
    }

    public void OnAttackButton()
    {
        TrySelectAction(BattleActionIds.Attack);
    }

    public void OnSkillButton()
    {
        TrySelectAction(BattleActionIds.Skill);
    }

    public void OnWaitButton()
    {
        if (!CanWait)
            return;

        if (CurrentState == BattleState.SelectTarget)
        {
            CancelSelectedAction();
        }

        TrySelectAction(BattleActionIds.Wait);
    }

    /// <summary>
    /// 対象選択を取り消してコマンド選択へ戻る。
    /// </summary>
    public void CancelSelectedAction()
    {
        if (_turnContext == null ||
            CurrentState != BattleState.SelectTarget)
        {
            return;
        }

        _selectedAction = null;

        _gridManager.PreparePlayerAction(
            _turnContext.Actor);

        ChangeState(BattleState.SelectCommand);
    }

    /// <summary>
    /// 現在のターンへ行動を追加する。既存の同じIdは置き換える。
    /// </summary>
    public bool AddActionToCurrentTurn(IBattleAction action)
    {
        return _turnContext != null &&
               _turnContext.Actions.Add(action);
    }

    public bool RemoveActionFromCurrentTurn(string actionId)
    {
        return _turnContext != null &&
               _turnContext.Actions.Remove(actionId);
    }

    public bool AddRuleToCurrentTurn(IActionRule rule)
    {
        return _turnContext != null &&
               _turnContext.Actions.AddRule(rule);
    }

    public bool RemoveRuleFromCurrentTurn(IActionRule rule)
    {
        return _turnContext != null &&
               _turnContext.Actions.RemoveRule(rule);
    }

    public ActionAvailability GetActionAvailability(
        string actionId)
    {
        if (_turnContext == null)
        {
            return ActionAvailability.Unavailable(
                "プレイヤーのターンではありません。");
        }

        return _turnContext.Actions.GetAvailability(
            actionId,
            _turnContext);
    }

    private bool CanSelectAction(string actionId)
    {
        return IsPlayerTurn &&
               _turnContext != null &&
               CurrentState == BattleState.SelectCommand &&
               GetActionAvailability(actionId).CanExecute;
    }

    /// <summary>
    /// Idで任意のアクションを選択する。
    /// 動的に生成したコマンドUIからも利用できる。
    /// </summary>
    public bool TrySelectAction(string actionId)
    {
        if (!CanSelectAction(actionId) ||
            !_turnContext.Actions.TryGet(
                actionId,
                out IBattleAction action))
        {
            return false;
        }

        _selectedAction = action;

        if (!action.RequiresTarget)
        {
            ExecuteSelectedAction(null);
            return true;
        }

        ChangeState(BattleState.SelectTarget);
        action.BeginTargetSelection(_turnContext);

        return true;
    }

    private void ExecuteSelectedAction(GridCell target)
    {
        if (_selectedAction == null || _turnContext == null)
            return;

        if (!GetActionAvailability(
                _selectedAction.Id).CanExecute)
        {
            CancelSelectedAction();
            return;
        }

        ChangeState(BattleState.ExecutingAction);

        BattleActionExecution result =
            _selectedAction.TryExecute(
                _turnContext,
                target,
                HandleAsyncActionCompleted);

        switch (result)
        {
            case BattleActionExecution.Rejected:
                if (_selectedAction.RequiresTarget)
                {
                    ChangeState(BattleState.SelectTarget);
                }
                else
                {
                    _selectedAction = null;
                    ChangeState(BattleState.SelectCommand);
                }
                break;

            case BattleActionExecution.Started:
                break;

            case BattleActionExecution.Completed:
                CompleteSelectedAction();
                break;

            case BattleActionExecution.EndTurn:
                _selectedAction = null;
                CompleteCurrentAction();
                break;
        }
    }

    private void HandleAsyncActionCompleted()
    {
        if (_turnContext == null ||
            CurrentState != BattleState.ExecutingAction)
        {
            return;
        }

        CompleteSelectedAction();
    }

    private void CompleteSelectedAction()
    {
        Unit actingUnit = _turnContext?.Actor;
        _selectedAction = null;

        if (IsBattleFinished())
            return;

        _gridManager.PreparePlayerAction(actingUnit);
        ChangeState(BattleState.SelectCommand);
    }

    private void HandleTurnConfigurationChanged()
    {
        if (_selectedAction != null &&
            CurrentState == BattleState.SelectTarget &&
            !GetActionAvailability(
                _selectedAction.Id).CanExecute)
        {
            CancelSelectedAction();
            return;
        }

        TurnActionsChanged?.Invoke();
    }

    /// <summary>
    /// 現在のターンコンテキストを破棄する。イベント登録も解除する。
    /// </summary>
    private void ReleaseTurnContext()
    {
        if (_turnContext != null)
        {
            _turnContext.Actions.Changed -=
                HandleTurnConfigurationChanged;

            _turnContext.Slots.Changed -=
                HandleTurnConfigurationChanged;
        }

        _selectedAction = null;
        _turnContext = null;
    }

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

        _turnOrder.Sort((left, right) =>
        {
            return right.Status.Speed.CompareTo(
                left.Status.Speed);
        });

        _turnIndex = -1;
        RoundCount++;
    }
}
