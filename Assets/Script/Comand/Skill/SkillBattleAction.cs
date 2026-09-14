using System;

public sealed class SkillBattleAction : IBattleAction
{
    public string Id => BattleActionIds.Skill;
    public string DisplayName => "スキル";
    public BattleActionCategory Category =>
        BattleActionCategory.Skill;
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
        // 選択中のスキルに応じた範囲表示は、
        // スキル選択機能を接続するときにここへ追加する。
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

        // TODO: 選択中のスキル効果を適用する。
        context.Slots.TryConsume(ActionSlot.Main);

        // 現在の仕様では主行動後の移動を許可しない。
        context.Slots.Clear(ActionSlot.Movement);

        return BattleActionExecution.Completed;
    }
}
