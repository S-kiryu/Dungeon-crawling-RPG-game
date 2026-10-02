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
    [SerializeField]
    [Min(0f)]
    private float _attackMultiplier = 1f;

    [SerializeField]
    private int _bonusDamage;

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

        int damage = Mathf.Max(
            0,
            Mathf.RoundToInt(
                context.Caster.Status.Attack *
                _attackMultiplier) +
            _bonusDamage);

        foreach (Unit target in
                 context.HitUnits)
        {
            if (target == null || target.IsDead)
                continue;

            target.TakeDamage(damage);
        }
    }
}