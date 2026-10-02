using UnityEngine;

/// <summary>
/// スキルの効果を表す抽象クラス。具体的な効果はこのクラスを継承して実装する。
/// </summary>
public abstract class SkillEffectData : ScriptableObject
{
    public abstract void Apply(SkillEffectContext context);

    /// <summary>
    /// この効果が対象へ与える実ダメージを取得する。
    /// ダメージを与えない効果はfalseを返す。
    /// </summary>
    public virtual bool TryGetDamagePreview(
        SkillEffectContext context,
        Unit target,
        out int damage)
    {
        damage = 0;
        return false;
    }
}
