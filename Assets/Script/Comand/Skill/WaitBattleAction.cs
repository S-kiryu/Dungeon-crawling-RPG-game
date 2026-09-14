using System;

public sealed class WaitBattleAction : IBattleAction
{
    public string Id => BattleActionIds.Wait;
    public string DisplayName => "待機";
    public BattleActionCategory Category =>
        BattleActionCategory.Utility;
    public bool RequiresTarget => false;

    public ActionAvailability GetAvailability(
        BattleTurnContext context)
    {
        return context != null && context.Actor != null
            ? ActionAvailability.Available()
            : ActionAvailability.Unavailable(
                "行動するユニットがいません。");
    }

    public void BeginTargetSelection(
        BattleTurnContext context)
    {
    }

    public BattleActionExecution TryExecute(
        BattleTurnContext context,
        GridCell target,
        Action onCompleted)
    {
        return GetAvailability(context).CanExecute
            ? BattleActionExecution.EndTurn
            : BattleActionExecution.Rejected;
    }
}
