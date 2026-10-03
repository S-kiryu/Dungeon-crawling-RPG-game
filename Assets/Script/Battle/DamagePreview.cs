/// <summary>
/// 対象へ与える予測ダメージと、攻撃後のHPを表す。
/// </summary>
public readonly struct DamagePreview
{
    public Unit Target { get; }
    public int Damage { get; }
    public int CurrentHP { get; }
    public int RemainingHP { get; }

    public DamagePreview(
        Unit target,
        int damage)
    {
        Target = target;
        Damage = System.Math.Max(0, damage);

        CurrentHP =
            target != null &&
            target.Status != null
                ? target.Status.CurrentHP
                : 0;

        RemainingHP =
            System.Math.Max(
                0,
                CurrentHP - Damage);
    }
}
