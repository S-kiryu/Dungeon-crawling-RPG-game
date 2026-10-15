using System;
using System.Collections.Generic;

/// <summary>
/// 戦闘ユニットの可変状態とルールを保持するModel。
/// Unity上の位置や演出は保持しない。
/// </summary>
public sealed class UnitModel
{
    public CharacterData Data { get; }
    public CurrentStatus Status { get; }
    public TeamType Team { get; }
    public ActionRangeData RangeData { get; }
    public IReadOnlyList<SkillData> Skills => _skills;
    public CharacterInstance SourceCharacter { get; }
    public bool IsDead => Status.CurrentHP <= 0;

    public event Action<int> Damaged;
    public event Action Died;

    public int CalculateDamageTaken(int rawDamage)
    {
        if (IsDead)
            return 0;

        return Math.Max(0, rawDamage - Status.Defense);
    }

    public int TakeDamage(int rawDamage)
    {
        if (IsDead)
            return 0;

        int actualDamage = CalculateDamageTaken(rawDamage);

        Status.CurrentHP = Math.Max(
            0,
            Status.CurrentHP - actualDamage);

        SourceCharacter?.ApplyBattleResult(Status);
        Damaged?.Invoke(actualDamage);

        if (IsDead)
        {
            Died?.Invoke();
        }

        return actualDamage;
    }

    public UnitModel(
        CharacterData data,
        CurrentStatus status,
        TeamType team,
        ActionRangeData rangeData,
        IEnumerable<SkillData> skills,
        CharacterInstance sourceCharacter = null)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Team = team;
        RangeData = rangeData;
        SourceCharacter = sourceCharacter;

        if (skills == null)
            return;

        foreach (SkillData skill in skills)
        {
            if (skill != null)
            {
                _skills.Add(skill);
            }
        }
    }

    private readonly List<SkillData> _skills = new();
}
