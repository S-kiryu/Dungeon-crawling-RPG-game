using System;

/// <summary>
/// 戦闘状態をHUD表示用の値へ変換するPresenter。
/// </summary>
public sealed class BattleHudPresenter
{

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _source.TurnActionsChanged += Refresh;
        Refresh();
    }

    public void Deactivate()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _source.TurnActionsChanged -= Refresh;
    }
    public BattleHudPresenter(
        IBattleHudSource source,
        IBattleHudView view)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    private readonly IBattleHudSource _source;
    private readonly IBattleHudView _view;
    private bool _isActive;

    private void Refresh()
    {
        TurnActionSlots slots = _source.CurrentTurnSlots;

        string movementText = slots == null
            ? "移動: -"
            : $"移動: {slots.GetRemaining(ActionSlot.Movement)}";

        string mainActionText = slots == null
            ? "主行動: -"
            : $"主行動: {slots.GetRemaining(ActionSlot.Main)}";

        _view.Render(
            new BattleHudState(
                $"ラウンド {_source.RoundCount}",
                GetCurrentUnitText(),
                GetBattleStateText(),
                movementText,
                mainActionText,
                _source.CanMove,
                _source.CanAttack,
                _source.CanUseSkill,
                _source.CanWait,
                _source.CurrentState == BattleState.SelectTarget));
    }

    private string GetCurrentUnitText()
    {
        Unit currentUnit = _source.CurrentTurnUnit;

        if (currentUnit == null)
            return "行動中: -";

        string unitName =
            currentUnit.Data != null &&
            !string.IsNullOrWhiteSpace(currentUnit.Data.CharacterName)
                ? currentUnit.Data.CharacterName
                : currentUnit.name;

        return $"行動中: {unitName}";
    }

    private string GetBattleStateText()
    {
        return _source.CurrentState switch
        {
            BattleState.PreparingTurn => "ターン準備中",
            BattleState.SelectCommand => "行動を選択してください",
            BattleState.SelectTarget => GetTargetSelectionText(),
            BattleState.ExecutingAction => "行動中",
            BattleState.EnemyTurn => "敵のターン",
            BattleState.BattleFinished => "戦闘終了",
            _ => string.Empty
        };
    }

    private string GetTargetSelectionText()
    {
        IBattleAction action = _source.SelectedAction;

        return action == null
            ? "対象を選択してください"
            : $"{action.DisplayName}の対象を選択";
    }
}
