using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterPoolEntry
{
    public CharacterData CharacterData;

    [Min(1)]
    public int Weight = 1;
}

[Serializable]
public class SkillPoolEntry
{
    public SkillData SkillData;

    [Min(1)]
    public int Weight = 1;
}

[Serializable]
public class RarityGachaSetting
{
    public CharacterRarity Rarity;

    [Header("排出重み")]
    [Min(1)]
    public int Weight = 1;

    [Header("ステータス倍率")]
    [Min(0f)]
    public float MinimumStatusMultiplier = 1f;

    [Min(0f)]
    public float MaximumStatusMultiplier = 1f;

    [Header("初期スキル数")]
    [Min(0)]
    public int SkillCount = 1;
}

/// <summary>
/// ガチャの排出候補と確率、レアリティごとの性能を管理する。
/// </summary>
[CreateAssetMenu(
    menuName = "Gacha/Gacha Settings",
    fileName = "GachaSettings")]
public class GachaSettings : ScriptableObject
{
    [Header("キャラ排出設定")]
    public List<CharacterPoolEntry> CharacterPool = new();

    [Header("レアリティ設定")]
    public List<RarityGachaSetting> RaritySettings = new();

    [Header("スキル排出設定")]
    public List<SkillPoolEntry> SkillPool = new();
}
