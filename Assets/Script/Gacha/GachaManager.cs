using System;
using UnityEngine;

/// <summary>
/// 雇用画面のView兼Composition Root。
/// クラス名は既存シーンとの互換性のため維持している。
/// </summary>
public class GachaManager : MonoBehaviour, IRecruitmentView
{
    public event Action<CharacterInstance> CharacterGenerated;

    /// <summary>
    /// Unity UI Buttonから呼び出す単発雇用。
    /// </summary>
    public void DrawOneFromButton()
    {
        DrawOne();
    }

    public CharacterInstance DrawOne()
    {
        if (!TryCreatePresenter())
            return null;

        return _presenter.RecruitOne();
    }

    public void ShowGeneratedCharacter(CharacterInstance character)
    {
        Debug.Log(CreateResultMessage(character), this);
    }

    public void ShowRecruitmentError(string message)
    {
        Debug.LogError(message, this);
    }

    [SerializeField]
    private GachaSettings _settings;

    [SerializeField]
    private CharacterRoster _roster;

    private RecruitmentPresenter _presenter;

    private void Awake()
    {
        TryCreatePresenter();
    }

    private bool TryCreatePresenter()
    {
        if (_presenter != null)
            return true;

        if (_roster == null)
        {
            _roster = CharacterRoster.Instance;
        }

        if (_settings == null)
        {
            ShowRecruitmentError("GachaSettingsが設定されていません。");
            return false;
        }

        if (_roster?.Model == null)
        {
            ShowRecruitmentError("CharacterRosterがシーンにありません。");
            return false;
        }

        _presenter = new RecruitmentPresenter(
            new CharacterGenerator(_settings),
            _roster.Model,
            this);

        _presenter.CharacterGenerated +=
            HandleCharacterGenerated;

        return true;
    }

    private void HandleCharacterGenerated(CharacterInstance character)
    {
        CharacterGenerated?.Invoke(character);
    }

    private static string CreateResultMessage(CharacterInstance character)
    {
        string skillNames = string.Empty;

        foreach (SkillData skill in character.Skills)
        {
            if (!string.IsNullOrEmpty(skillNames))
            {
                skillNames += ", ";
            }

            skillNames += skill.SkillName;
        }

        CurrentStatus status = character.Status;

        return
            $"雇用: {character.CharacterData.CharacterName} " +
            $"{character.Rarity}\n" +
            $"ID: {character.InstanceId}\n" +
            $"HP: {status.MaxHP} " +
            $"攻撃: {status.Attack} " +
            $"防御: {status.Defense} " +
            $"速度: {status.Speed}\n" +
            $"スキル: {skillNames}";
    }
}
