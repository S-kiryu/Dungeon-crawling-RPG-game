using UnityEngine;

public abstract class SkillEffectData : ScriptableObject
{
    public abstract void Apply(SkillEffectContext context);
}