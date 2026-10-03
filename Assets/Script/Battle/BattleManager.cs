using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バトルのターン進行と、現在選択中のアクションを管理する。
/// </summary>
public class BattleManager : MonoBehaviour, IBattleHudSource
{

    public Unit CurrentTurnUnit { get; private set; }
    public int RoundCount => _turnOrder.RoundCount;

    /// <summary>
    /// 現在、情報表示対象として選択されているユニット。
    /// </summary>
    public Unit InspectedUnit { get; private set; }

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

    public event Action<bool> BattleEnded;

    /// <summary>
    /// アクションの追加・削除、残り枠、状態のいずれかが変化した。
    /// コマンドUIの再描画に使用できる。
    /// </summary>
    public event Action TurnActionsChanged;

    /// <summary>
    /// 現在の行動者、または今後の行動順が変化した。
    /// </summary>
    public event Action TurnOrderChanged;

    /// <summary>
    /// 情報表示対象のユニットが変化した。
    /// </summary>
    public event Action<Unit> InspectedUnitChanged;

    /// <summary>
    /// 現在のユニットのターンを終了する。
    /// </summary>
    public void CompleteCurrentAction()
    {
        _gridManager.ClearBattleSelection();

        ReleaseTurnContext();
        CurrentTurnUnit = null;
        _turnOrder.ClearCurrent();

        AdvanceTurn();
    }

    public void OnCellClicked(GridCell clickedCell)
    {
        if (clickedCell == null)
            return;

        SetInspectedUnit(
            clickedCell.CurrentUnit);

        if (!IsPlayerTurn ||
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

    /// <summary>
    /// 対象選択を取り消してコマンド選択へ戻る。
    /// </summary>
    public void OnCancelSelectedAction()
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

    public void OnWaitButton()
    {
        if (!CanWait)
            return;

        if (CurrentState == BattleState.SelectTarget)
        {
            OnCancelSelectedAction();
        }

        TrySelectAction(BattleActionIds.Wait);
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

    /// <summary>
    /// スキル一覧UIで選択されたスキルを開始する。
    /// </summary>
    public bool TrySelectSkill(
        SkillData skill)
    {
        if (_turnContext == null ||
            CurrentTurnUnit == null ||
            CurrentState != BattleState.SelectCommand ||
            !IsPlayerTurn ||
            skill == null ||
            !HasCurrentUnitSkill(skill))
        {
            return false;
        }

        SkillBattleAction skillAction =
            new SkillBattleAction(skill);

        if (!skillAction
                .GetAvailability(_turnContext)
                .CanExecute)
        {
            return false;
        }

        // Idが"skill"の行動を、選択したスキルで置き換える
        if (!_turnContext.Actions.Add(
                skillAction))
        {
            return false;
        }

        return TrySelectAction(
            BattleActionIds.Skill);
    }

    /// <summary>
    /// 現在選択中の行動が対象へ与えるダメージを取得する。
    /// </summary>
    public bool TryGetSelectedDamagePreview(
        GridCell targetCell,
        out DamagePreview preview)
    {
        preview = default;

        if (_turnContext == null ||
            _selectedAction == null ||
            CurrentState != BattleState.SelectTarget ||
            _selectedAction is not
                IDamagePreviewAction previewAction)
        {
            return false;
        }

        return previewAction
            .TryGetDamagePreview(
                _turnContext,
                targetCell,
                out preview);
    }

    /// <summary>
    /// スキル一覧でカーソルを合わせたスキルの射程だけを表示する。
    /// </summary>
    public void PreviewSkillRange(
        SkillData skill)
    {
        if (_turnContext == null ||
            CurrentTurnUnit == null ||
            CurrentState != BattleState.SelectCommand ||
            !IsPlayerTurn ||
            skill == null ||
            !HasCurrentUnitSkill(skill))
        {
            return;
        }

        _gridManager.ShowSkillRange(
            CurrentTurnUnit,
            skill);
    }

    /// <summary>
    /// スキル一覧の射程プレビューを解除する。
    /// </summary>
    public void ClearSkillRangePreview()
    {
        if (_turnContext == null ||
            CurrentTurnUnit == null ||
            CurrentState != BattleState.SelectCommand)
        {
            return;
        }

        _gridManager.PreparePlayerAction(
            CurrentTurnUnit);
    }

    /// <summary>
    /// 現在の行動者を先頭として、今後行動するユニットを取得する。
    /// 現在ラウンドの末尾へ到達した場合は次ラウンドの先頭から補充する。
    /// </summary>
    public List<Unit> GetUpcomingTurnUnits(
        int maximumCount)
    {
        List<Unit> result = new();

        foreach (UnitModel model in
                 _turnOrder.GetUpcoming(maximumCount))
        {
            Unit unit = FindUnit(model);

            if (unit != null)
            {
                result.Add(unit);
            }
        }

        return result;
    }
    [SerializeField]
    private EnemyTurnController _enemyTurnController;

    [SerializeField]
    private GridManager _gridManager;

    [SerializeField]
    private UnitManager _unitManager;

    private readonly BattleTurnOrderModel _turnOrder = new();
    private bool _battleEnded;

    private BattleTurnContext _turnContext;
    private IBattleAction _selectedAction;
    private ActiveUnitMarker _activeUnitMarker;

    private bool IsPlayerTurn =>
        CurrentTurnUnit != null &&
        CurrentTurnUnit.Team == TeamType.Player;

    private void Awake()
    {
        _activeUnitMarker =
            GetComponent<ActiveUnitMarker>();

        if (_activeUnitMarker == null)
        {
            _activeUnitMarker =
                gameObject.AddComponent<
                    ActiveUnitMarker>();
        }

        _activeUnitMarker.Initialize(this);

        _unitManager.UnitsReady +=
            BeginBattle;

        if (_gridManager != null)
        {
            _gridManager.CellClicked +=
                OnCellClicked;

            _gridManager.HoveredCellChanged +=
                HandleHoveredCellChanged;
        }
    }

    private void OnDestroy()
    {
        if (_unitManager != null)
        {
            _unitManager.UnitsReady -=
                BeginBattle;
        }

        if (_gridManager != null)
        {
            _gridManager.CellClicked -=
                OnCellClicked;

            _gridManager.HoveredCellChanged -=
                HandleHoveredCellChanged;
        }

        ReleaseTurnContext();
    }

    /// <summary>
    /// バトル開始
    /// </summary>
    private void BeginBattle()
    {
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
    /// クリックして表示したユニットのマスから
    /// カーソルが外れたら情報表示を解除する。
    /// </summary>
    private void HandleHoveredCellChanged(
        GridCell hoveredCell)
    {
        if (InspectedUnit == null)
            return;

        // まだ表示中ユニットのマスにいる
        if (hoveredCell != null &&
            hoveredCell.CurrentUnit ==
                InspectedUnit)
        {
            return;
        }

        SetInspectedUnit(null);
    }

    /// <summary>
    /// ターンを進める。現在のターンが終了した場合、次のユニットのターンへ移行する。
    /// </summary>
    private void AdvanceTurn()
    {
        if (IsBattleFinished())
            return;

        UnitModel nextModel = _turnOrder.Advance(
            GetParticipantModels());

        CurrentTurnUnit = FindUnit(nextModel);

        if (CurrentTurnUnit == null)
            return;

        TurnOrderChanged?.Invoke();

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
        actions.Add(new WaitBattleAction());


        if (unit.Skills.Count > 0)
        {
            actions.Add(
                new SkillBattleAction(
                    unit.Skills[0]));
        }

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
        _turnOrder.ClearCurrent();

        TurnOrderChanged?.Invoke();

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
    /// 情報表示対象のユニットを変更する。
    /// </summary>
    private void SetInspectedUnit(Unit unit)
    {
        if (InspectedUnit == unit)
            return;

        InspectedUnit = unit;

        InspectedUnitChanged?.Invoke(
            InspectedUnit);
    }

    private bool CanSelectAction(string actionId)
    {
        return IsPlayerTurn &&
               _turnContext != null &&
               CurrentState == BattleState.SelectCommand &&
               GetActionAvailability(actionId).CanExecute;
    }

    private void ExecuteSelectedAction(GridCell target)
    {
        if (_selectedAction == null || _turnContext == null)
            return;

        if (!GetActionAvailability(
                _selectedAction.Id).CanExecute)
        {
            OnCancelSelectedAction();
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

        TurnOrderChanged?.Invoke();

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
            OnCancelSelectedAction();
            return;
        }

        TurnActionsChanged?.Invoke();
    }

    /// <summary>
    /// 現在のユニットが指定スキルを所持しているか。
    /// </summary>
    private bool HasCurrentUnitSkill(
        SkillData skill)
    {
        if (CurrentTurnUnit == null ||
            skill == null)
        {
            return false;
        }

        foreach (SkillData ownedSkill in
                 CurrentTurnUnit.Skills)
        {
            if (ownedSkill == skill)
            {
                return true;
            }
        }

        return false;
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

    private List<UnitModel> GetParticipantModels()
    {
        List<UnitModel> participants = new();

        foreach (Unit unit in _unitManager.Units)
        {
            if (unit?.Model != null)
            {
                participants.Add(unit.Model);
            }
        }

        return participants;
    }

    private Unit FindUnit(UnitModel model)
    {
        if (model == null)
            return null;

        foreach (Unit unit in _unitManager.Units)
        {
            if (unit != null && unit.Model == model)
            {
                return unit;
            }
        }

        return null;
    }
}
