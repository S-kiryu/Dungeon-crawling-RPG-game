using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクターが所持できるスキルの固定データ。
/// </summary>
[CreateAssetMenu(
    menuName = "Battle/Skill Data",
    fileName = "NewSkillData")]
public class SkillData : ScriptableObject
{
    [Header("基本情報")]
    public string SkillId;
    public string SkillName;

    [TextArea]
    public string Description;

    public Sprite Icon;

    [Header("対象範囲")]
    public ActionRangeData ActionRangeData;

    [Header("スキル効果")]
    public List<SkillEffectData> Effects = new();
}
