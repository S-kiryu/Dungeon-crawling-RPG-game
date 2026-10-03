using System;
using System.Collections.Generic;

/// <summary>
/// スキルを使用する戦闘行動。
/// </summary>
public sealed class SkillBattleAction :
    IBattleAction,
    IDamagePreviewAction
{
    private readonly SkillData _skill;

    public SkillBattleAction(
        SkillData skill)
    {
        _skill = skill;
    }

    public string Id =>
        BattleActionIds.Skill;

    public string DisplayName =>
        _skill != null &&
        !string.IsNullOrWhiteSpace(
            _skill.SkillName)
                ? _skill.SkillName
                : "スキル";

    public BattleActionCategory Category =>
        BattleActionCategory.Skill;

    public bool RequiresTarget => true;

    /// <summary>
    /// スキルを使用できるかどうかを判定
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public ActionAvailability GetAvailability(
        BattleTurnContext context)
    {
        if (context == null ||
            context.Actor == null)
        {
            return ActionAvailability.Unavailable(
                "行動するユニットがいません。");
        }

        if (_skill == null)
        {
            return ActionAvailability.Unavailable(
                "スキルが設定されていません。");
        }

        if (_skill.ActionRangeData == null)
        {
            return ActionAvailability.Unavailable(
                "スキルの射程が設定されていません。");
        }

        if (_skill.Effects == null ||
            !_skill.Effects.Exists(
                effect => effect != null))
        {
            return ActionAvailability.Unavailable(
                "スキル効果が設定されていません。");
        }

        if (!context.Slots.Has(
                ActionSlot.Main))
        {
            return ActionAvailability.Unavailable(
                "主行動枠を使い切っています。");
        }

        return ActionAvailability.Available();
    }

    public void BeginTargetSelection(
        BattleTurnContext context)
    {
        context.Grid.ShowSkillRange(
            context.Actor,
            _skill);
    }

    public BattleActionExecution TryExecute(
        BattleTurnContext context,
        GridCell targetCell,
        Action onCompleted)
    {
        if (!GetAvailability(context).CanExecute ||
            targetCell == null)
        {
            return BattleActionExecution.Rejected;
        }

        if (!context.Grid.IsInRange(
                context.Actor,
                targetCell,
                _skill.ActionRangeData))
        {
            return BattleActionExecution.Rejected;
        }

        if (!context.Grid.IsValidSkillTarget(
                context.Actor,
                targetCell,
                _skill.TargetType))
        {
            return BattleActionExecution.Rejected;
        }

        Unit mainTarget =
            targetCell.CurrentUnit;

        List<Unit> hitUnits = new();

        if (mainTarget != null)
        {
            hitUnits.Add(mainTarget);
        }

        SkillEffectContext effectContext =
            new SkillEffectContext
            {
                Caster = context.Actor,
                MainTarget = mainTarget,

                TargetGrids =
                    new List<GridCell>
                    {
                        targetCell
                    },

                HitUnits = hitUnits
            };

        foreach (SkillEffectData effect in
                 _skill.Effects)
        {
            if (effect != null)
            {
                effect.Apply(effectContext);
            }
        }

        context.Slots.TryConsume(
            ActionSlot.Main);

        // 現在はスキル使用後の移動を不可にする
        context.Slots.Clear(
            ActionSlot.Movement);

        return BattleActionExecution.Completed;
    }

    public bool TryGetDamagePreview(
        BattleTurnContext context,
        GridCell targetCell,
        out DamagePreview preview)
    {
        preview = default;

        if (!GetAvailability(context).CanExecute ||
            targetCell == null ||
            !context.Grid.IsInRange(
                context.Actor,
                targetCell,
                _skill.ActionRangeData) ||
            !context.Grid.IsValidSkillTarget(
                context.Actor,
                targetCell,
                _skill.TargetType))
        {
            return false;
        }

        Unit target = targetCell.CurrentUnit;

        if (target == null || target.IsDead)
            return false;

        SkillEffectContext effectContext =
            new SkillEffectContext
            {
                Caster = context.Actor,
                MainTarget = target,
                TargetGrids =
                    new List<GridCell>
                    {
                        targetCell
                    },
                HitUnits =
                    new List<Unit>
                    {
                        target
                    }
            };

        int totalDamage = 0;
        bool hasDamageEffect = false;

        foreach (SkillEffectData effect in
                 _skill.Effects)
        {
            if (effect != null &&
                effect.TryGetDamagePreview(
                    effectContext,
                    target,
                    out int damage))
            {
                totalDamage += damage;
                hasDamageEffect = true;
            }
        }

        if (!hasDamageEffect)
            return false;

        preview = new DamagePreview(
            target,
            totalDamage);

        return true;
    }
}
