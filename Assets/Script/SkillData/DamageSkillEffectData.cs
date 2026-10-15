using UnityEngine;

/// <summary>
/// 使用者の攻撃力を基準にダメージを与える効果。
/// 防御力の計算はUnit.TakeDamage側で行われる。
/// </summary>
[CreateAssetMenu(
    menuName = "Battle/Skill Effect/Damage",
    fileName = "NewDamageSkillEffect")]
public class DamageSkillEffectData : SkillEffectData
{

    public override void Apply(
        SkillEffectContext context)
    {
        if (context == null ||
            context.Caster == null ||
            context.Caster.Status == null ||
            context.HitUnits == null)
        {
            return;
        }

        int rawDamage =
            CalculateRawDamage(
                context.Caster);

        foreach (Unit target in
                 context.HitUnits)
        {
            if (target == null || target.IsDead)
                continue;

            target.TakeDamage(rawDamage);
        }
    }

    public override bool TryGetDamagePreview(
        SkillEffectContext context,
        Unit target,
        out int damage)
    {
        damage = 0;

        if (context == null ||
            context.Caster == null ||
            target == null ||
            target.IsDead)
        {
            return false;
        }

        int rawDamage =
            CalculateRawDamage(
                context.Caster);

        damage =
            target.CalculateDamageTaken(
                rawDamage);

        return true;
    }
    [SerializeField]
    [Min(0f)]
    private float _attackMultiplier = 1f;

    [SerializeField]
    private int _bonusDamage;

    private int CalculateRawDamage(
        Unit caster)
    {
        if (caster == null ||
            caster.Status == null)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                caster.Status.Attack *
                _attackMultiplier) +
            _bonusDamage);
    }
}
