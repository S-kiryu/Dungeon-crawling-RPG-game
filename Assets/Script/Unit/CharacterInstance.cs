using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットの実態
/// </summary>
[Serializable]
public class CharacterInstance
{
    [Header("キャラ設定")]
    [SerializeField]
    private string _instanceId;

    [SerializeField]
    private CharacterData _characterData;

    [SerializeField]
    private CharacterRarity _rarity;

    [SerializeField]
    private CurrentStatus _status;

    [SerializeField]
    private List<SkillData> _skills = new();

    [SerializeField]
    private bool _isDead;

    public string InstanceId => _instanceId;
    public CharacterData CharacterData => _characterData;
    public CharacterRarity Rarity => _rarity;
    public CurrentStatus Status => _status;
    public IReadOnlyList<SkillData> Skills => _skills;
    public bool IsDead => _isDead;

    public bool CanDeploy =>
        !_isDead &&
        _status != null &&
        _status.CurrentHP > 0;

    public CharacterInstance(
        CharacterData characterData,
        CharacterRarity rarity,
        CurrentStatus generatedStatus,
        IReadOnlyList<SkillData> generatedSkills)
    {
        _instanceId = Guid.NewGuid().ToString("N");
        _characterData = characterData;
        _rarity = rarity;
        _status = new CurrentStatus(generatedStatus);
        _skills = generatedSkills != null
            ? new List<SkillData>(generatedSkills)
            : new List<SkillData>();
        _isDead = false;
    }

    /// <summary>
    /// 死んでるかを戦闘が終了した際に確認する物
    /// </summary>
    /// <param name="battleStatus"></param>
    public void ApplyBattleResult(CurrentStatus battleStatus)
    {
        if (_isDead || battleStatus == null)
            return;

        _status.CurrentHP = Mathf.Clamp(
            battleStatus.CurrentHP,
            0,
            _status.MaxHP);

        if (_status.CurrentHP <= 0)
        {
            _status.CurrentHP = 0;
            _isDead = true;
        }
    }
}
