using System;

/// <summary>
/// 移動アクションを表すクラス
/// </summary>
public sealed class MoveBattleAction : IBattleAction
{
    public string Id => BattleActionIds.Move;
    public string DisplayName => "移動";
    public BattleActionCategory Category =>
        BattleActionCategory.Movement;
    public bool RequiresTarget => true;


    public ActionAvailability GetAvailability(
        BattleTurnContext context)
    {
        if (context == null || context.Actor == null)
        {
            return ActionAvailability.Unavailable(
                "行動するユニットがいません。");
        }

        if (!context.Slots.Has(ActionSlot.Movement))
        {
            return ActionAvailability.Unavailable(
                "移動枠を使い切っています。");
        }

        return ActionAvailability.Available();
    }

    public void BeginTargetSelection(
        BattleTurnContext context)
    {
        context.Grid.ShowMovementRange(context.Actor);
    }

    public BattleActionExecution TryExecute(
        BattleTurnContext context,
        GridCell target,
        Action onCompleted)
    {
        if (!GetAvailability(context).CanExecute ||
            target == null)
        {
            return BattleActionExecution.Rejected;
        }

        GridCell previousCell = context.Actor.CurrentCell;

        bool started = context.Grid.TryMoveUnit(
            context.Actor,
            target.Position,
            onCompleted);

        if (!started)
            return BattleActionExecution.Rejected;

        context.Slots.TryConsume(ActionSlot.Movement);
        context.Grid.ClearBattleSelection();
        context.Grid.RestoreCellMaterial(previousCell);

        return BattleActionExecution.Started;
    }
}
