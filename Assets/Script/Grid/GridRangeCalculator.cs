using UnityEngine;

/// <summary>
/// 攻撃・スキル範囲と対象条件を判定するModelサービス。
/// </summary>
public sealed class GridRangeCalculator
{
    public bool IsInRange(
        Vector2Int origin,
        Vector2Int target,
        ActionRangeData rangeData)
    {
        if (rangeData == null || rangeData.Offsets == null)
            return false;

        Vector2Int targetOffset = target - origin;

        foreach (Vector2Int offset in rangeData.Offsets)
        {
            if (offset == targetOffset)
                return true;
        }

        return false;
    }

    public bool IsValidTarget(
        Unit caster,
        Unit target,
        SkillTargetType targetType)
    {
        if (caster == null)
            return false;

        switch (targetType)
        {
            case SkillTargetType.Enemy:
                return target != null &&
                       !target.IsDead &&
                       target.Team != caster.Team &&
                       target.Team != TeamType.Neutral;

            case SkillTargetType.Ally:
                return target != null &&
                       !target.IsDead &&
                       target != caster &&
                       target.Team == caster.Team;

            case SkillTargetType.Self:
                return target == caster && !caster.IsDead;

            case SkillTargetType.AnyUnit:
                return target != null && !target.IsDead;

            case SkillTargetType.EmptyCell:
                return target == null;

            default:
                return false;
        }
    }

    public bool IsSupportTarget(SkillTargetType targetType)
    {
        return targetType == SkillTargetType.Ally ||
               targetType == SkillTargetType.Self;
    }
}
