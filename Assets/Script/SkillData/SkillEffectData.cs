using UnityEngine;

/// <summary>
/// スキルの効果を表す抽象クラス。具体的な効果はこのクラスを継承して実装する。
/// </summary>
public abstract class SkillEffectData : ScriptableObject
{
    public abstract void Apply(SkillEffectContext context);
}