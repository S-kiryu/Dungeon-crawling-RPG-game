using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GachaSettingsを使ってキャラクター個体を生成する。
/// 抽選、能力生成、スキル生成を個別の関数へ分離している。
/// </summary>
public class CharacterGenerator
{
    private readonly GachaSettings _settings;

    public CharacterGenerator(GachaSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// ユニットを渡す関数
    /// </summary>
    /// <returns></returns>
    public CharacterInstance Generate()
    {
        if (!TryGenerate(out CharacterInstance character))
            return null;

        return character;
    }

    /// <summary>
    /// ユニットを作る関数
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public bool TryGenerate(out CharacterInstance character)
    {
        character = null;

        if (!ValidateSettings())
            return false;

        if (!TrySelectCharacterData(
                out CharacterData characterData))
        {
            Debug.LogError(
                "抽選可能なCharacterDataがありません。");
            return false;
        }

        if (!TrySelectRaritySetting(
                out RarityGachaSetting raritySetting))
        {
            Debug.LogError(
                "抽選可能なレアリティ設定がありません。");
            return false;
        }

        CurrentStatus generatedStatus =
            GenerateStatus(
                characterData.Status,
                raritySetting);

        List<SkillData> generatedSkills =
            GenerateSkills(
                raritySetting.SkillCount);

        character = CreateCharacterInstance(
            characterData,
            raritySetting.Rarity,
            generatedStatus,
            generatedSkills);

        return character != null;
    }

    private bool ValidateSettings()
    {
        if (_settings != null)
            return true;

        Debug.LogError(
            "GachaSettingsが設定されていません。");
        return false;
    }

    /// <summary>
    /// キャラクターデータを選ぶ
    /// </summary>
    /// <param name="characterData"></param>
    /// <returns></returns>
    private bool TrySelectCharacterData(
        out CharacterData characterData)
    {
        characterData = null;

        List<CharacterPoolEntry> candidates =
            GetValidCharacterEntries();

        if (!TrySelectWeighted(
                candidates,
                entry => entry.Weight,
                out CharacterPoolEntry selectedEntry))
        {
            return false;
        }

        characterData = selectedEntry.CharacterData;
        return characterData != null;
    }

    /// <summary>
    /// レアリティーを決める
    /// </summary>
    /// <param name="raritySetting"></param>
    /// <returns></returns>
    private bool TrySelectRaritySetting(
        out RarityGachaSetting raritySetting)
    {
        List<RarityGachaSetting> candidates =
            GetValidRaritySettings();

        return TrySelectWeighted(
            candidates,
            setting => setting.Weight,
            out raritySetting);
    }

    private List<CharacterPoolEntry>
        GetValidCharacterEntries()
    {
        List<CharacterPoolEntry> candidates = new();

        foreach (CharacterPoolEntry entry
                 in _settings.CharacterPool)
        {
            if (entry == null ||
                entry.CharacterData == null ||
                entry.CharacterData.Status == null)
            {
                continue;
            }

            candidates.Add(entry);
        }

        return candidates;
    }

    private List<RarityGachaSetting>
        GetValidRaritySettings()
    {
        List<RarityGachaSetting> candidates = new();

        foreach (RarityGachaSetting setting
                 in _settings.RaritySettings)
        {
            if (setting == null)
                continue;

            candidates.Add(setting);
        }

        return candidates;
    }

    /// <summary>
    /// キャラクターのステータスを決める関数
    /// </summary>
    /// <param name="baseStatus"></param>
    /// <param name="raritySetting"></param>
    /// <returns></returns>
    private CurrentStatus GenerateStatus(
        StatusBase baseStatus,
        RarityGachaSetting raritySetting)
    {
        CurrentStatus status =
            CreateBaseStatus(baseStatus);

        GetOrderedMultiplierRange(
            raritySetting,
            out float minimumMultiplier,
            out float maximumMultiplier);

        ApplyRandomCoreStats(
            status,
            baseStatus,
            minimumMultiplier,
            maximumMultiplier);

        ApplyFixedStats(
            status,
            baseStatus);

        return status;
    }

    private CurrentStatus CreateBaseStatus(
        StatusBase baseStatus)
    {
        return new CurrentStatus(baseStatus);
    }

    private void GetOrderedMultiplierRange(
        RarityGachaSetting raritySetting,
        out float minimumMultiplier,
        out float maximumMultiplier)
    {
        minimumMultiplier = Mathf.Min(
            raritySetting.MinimumStatusMultiplier,
            raritySetting.MaximumStatusMultiplier);

        maximumMultiplier = Mathf.Max(
            raritySetting.MinimumStatusMultiplier,
            raritySetting.MaximumStatusMultiplier);
    }

    private void ApplyRandomCoreStats(
        CurrentStatus targetStatus,
        StatusBase baseStatus,
        float minimumMultiplier,
        float maximumMultiplier)
    {
        targetStatus.MaxHP = GenerateStat(
            baseStatus.HP,
            minimumMultiplier,
            maximumMultiplier,
            1);

        targetStatus.CurrentHP =
            targetStatus.MaxHP;

        targetStatus.Attack = GenerateStat(
            baseStatus.Attack,
            minimumMultiplier,
            maximumMultiplier,
            0);

        targetStatus.Defense = GenerateStat(
            baseStatus.Deffens,
            minimumMultiplier,
            maximumMultiplier,
            0);

        targetStatus.Speed = GenerateStat(
            baseStatus.Speed,
            minimumMultiplier,
            maximumMultiplier,
            1);

        targetStatus.Weight = GenerateStat(
            baseStatus.Weight,
            minimumMultiplier,
            maximumMultiplier,
            0);
    }

    private void ApplyFixedStats(
        CurrentStatus targetStatus,
        StatusBase baseStatus)
    {
        targetStatus.Level =
            Mathf.Max(1, baseStatus.Level);

        // 移動力は個体値で変動させない。
        targetStatus.MoveLength =
            baseStatus.MoveLength;
    }

    private int GenerateStat(
        int baseValue,
        float minimumMultiplier,
        float maximumMultiplier,
        int minimumValue)
    {
        float multiplier = UnityEngine.Random.Range(
            minimumMultiplier,
            maximumMultiplier);

        int generatedValue = Mathf.RoundToInt(
            baseValue * multiplier);

        return Mathf.Max(
            minimumValue,
            generatedValue);
    }

    /// <summary>
    /// スキルを指定分選択する関数
    /// </summary>
    /// <param name="requestedCount"></param>
    /// <returns></returns>
    private List<SkillData> GenerateSkills(
        int requestedCount)
    {
        List<SkillPoolEntry> candidates =
            GetValidSkillEntries();

        int skillCount = Mathf.Clamp(
            requestedCount,
            0,
            candidates.Count);

        List<SkillData> selectedSkills = new();

        for (int index = 0;
             index < skillCount;
             index++)
        {
            if (!TrySelectSkill(
                    candidates,
                    out SkillPoolEntry selectedEntry))
            {
                break;
            }

            selectedSkills.Add(
                selectedEntry.SkillData);

            // 選ばれたスキルを候補から外して重複を防ぐ。
            candidates.Remove(selectedEntry);
        }

        return selectedSkills;
    }

    private List<SkillPoolEntry> GetValidSkillEntries()
    {
        List<SkillPoolEntry> candidates = new();

        foreach (SkillPoolEntry entry
                 in _settings.SkillPool)
        {
            if (entry == null ||
                entry.SkillData == null)
            {
                continue;
            }

            // 同じSkillDataが複数登録されていても一度だけ候補にする。
            bool alreadyAdded = candidates.Exists(
                candidate =>
                    candidate.SkillData == entry.SkillData);

            if (!alreadyAdded)
                candidates.Add(entry);
        }

        return candidates;
    }

    private bool TrySelectSkill(
        List<SkillPoolEntry> candidates,
        out SkillPoolEntry selectedEntry)
    {
        return TrySelectWeighted(
            candidates,
            entry => entry.Weight,
            out selectedEntry);
    }

    /// <summary>
    /// 作ったものをまとめる
    /// </summary>
    /// <param name="characterData"></param>
    /// <param name="rarity"></param>
    /// <param name="status"></param>
    /// <param name="skills"></param>
    /// <returns></returns>
    private CharacterInstance CreateCharacterInstance(
        CharacterData characterData,
        CharacterRarity rarity,
        CurrentStatus status,
        IReadOnlyList<SkillData> skills)
    {
        if (characterData == null || status == null)
            return null;

        return new CharacterInstance(
            characterData,
            rarity,
            status,
            skills);
    }

    /// <summary>
    /// 候補の重みを合計し、その比率で一つを選択する共通関数。
    /// </summary>
    private bool TrySelectWeighted<T>(
        IReadOnlyList<T> candidates,
        Func<T, int> getWeight,
        out T selected)
        where T : class
    {
        selected = null;

        if (candidates == null ||
            candidates.Count == 0 ||
            getWeight == null)
        {
            return false;
        }

        int totalWeight = CalculateTotalWeight(
            candidates,
            getWeight);

        if (totalWeight <= 0)
            return false;

        int randomValue = UnityEngine.Random.Range(
            0,
            totalWeight);

        foreach (T candidate in candidates)
        {
            if (candidate == null)
                continue;

            randomValue -= GetValidWeight(
                candidate,
                getWeight);

            if (randomValue < 0)
            {
                selected = candidate;
                return true;
            }
        }

        return false;
    }

    private int CalculateTotalWeight<T>(
        IReadOnlyList<T> candidates,
        Func<T, int> getWeight)
        where T : class
    {
        int totalWeight = 0;

        foreach (T candidate in candidates)
        {
            if (candidate == null)
                continue;

            totalWeight += GetValidWeight(
                candidate,
                getWeight);
        }

        return totalWeight;
    }

    private int GetValidWeight<T>(
        T candidate,
        Func<T, int> getWeight)
        where T : class
    {
        return Mathf.Max(
            1,
            getWeight(candidate));
    }
}
