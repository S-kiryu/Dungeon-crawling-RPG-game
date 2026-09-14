using System;

/// <summary>
/// 攻撃アクションを表すクラス
/// </summary>
public sealed class AttackBattleAction : IBattleAction
{
    public string Id => BattleActionIds.Attack;
    public string DisplayName => "攻撃";
    public BattleActionCategory Category =>
        BattleActionCategory.Attack;
    public bool RequiresTarget => true;

    public ActionAvailability GetAvailability(
        BattleTurnContext context)
    {
        if (context == null || context.Actor == null)
        {
            return ActionAvailability.Unavailable(
                "行動するユニットがいません。");
        }

        if (!context.Slots.Has(ActionSlot.Main))
        {
            return ActionAvailability.Unavailable(
                "主行動枠を使い切っています。");
        }

        return ActionAvailability.Available();
    }

    public void BeginTargetSelection(
        BattleTurnContext context)
    {
        context.Grid.ShowAttackRange(context.Actor);
    }

    public BattleActionExecution TryExecute(
        BattleTurnContext context,
        GridCell targetCell,
        Action onCompleted)
    {
        if (!GetAvailability(context).CanExecute ||
            targetCell == null ||
            !targetCell.IsOccupied)
        {
            return BattleActionExecution.Rejected;
        }

        Unit target = targetCell.CurrentUnit;

        if (target == null ||
            target.Team != TeamType.Enemy ||
            !context.Grid.IsInActionRange(
                context.Actor,
                targetCell))
        {
            return BattleActionExecution.Rejected;
        }

        context.Slots.TryConsume(ActionSlot.Main);

        // 現在の仕様では主行動後の移動を許可しない。
        context.Slots.Clear(ActionSlot.Movement);

        target.TakeDamage(context.Actor.Status.Attack);

        return BattleActionExecution.Completed;
    }
}
