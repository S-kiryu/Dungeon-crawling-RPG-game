using System;
using UnityEngine;

/// <summary>
/// ガチャを実行し、生成されたキャラクターを所持一覧へ追加する。
/// </summary>
public class GachaManager : MonoBehaviour
{
    [SerializeField]
    private GachaSettings _settings;

    [SerializeField]
    private CharacterRoster _roster;

    private CharacterGenerator _generator;

    public event Action<CharacterInstance> CharacterGenerated;

    private void Awake()
    {
        _generator = new CharacterGenerator(_settings);

        if (_roster == null)
            _roster = CharacterRoster.Instance;
    }

    /// <summary>
    /// Unity UI Buttonから呼び出す単発ガチャ。
    /// </summary>
    public void DrawOneFromButton()
    {
        DrawOne();
    }

    /// <summary>
    /// 一回のガチャ処理
    /// </summary>
    /// <returns></returns>
    public CharacterInstance DrawOne()
    {
        if (_generator == null)
        {
            Debug.LogError(
                "CharacterGeneratorがありません。",
                this);
            return null;
        }

        if (_roster == null)
            _roster = CharacterRoster.Instance;

        if (_roster == null)
        {
            Debug.LogError(
                "CharacterRosterがシーンにありません。",
                this);
            return null;
        }

        CharacterInstance character =
            _generator.Generate();

        if (character == null)
            return null;

        if (!_roster.Add(character))
            return null;

        CharacterGenerated?.Invoke(character);

        Debug.Log(
            CreateResultMessage(character),
            this);

        return character;
    }

    /// <summary>
    /// ガチャ結果を表示する物
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    private string CreateResultMessage(
        CharacterInstance character)
    {
        string skillNames = string.Empty;

        foreach (SkillData skill in character.Skills)
        {
            if (!string.IsNullOrEmpty(skillNames))
                skillNames += ", ";

            skillNames += skill.SkillName;
        }

        CurrentStatus status = character.Status;

        return
            $"獲得: " +
            $"{character.CharacterData.CharacterName} " +
            $"{character.Rarity}\n" +
            $"ID: {character.InstanceId}\n" +
            $"HP: {status.MaxHP} " +
            $"攻撃: {status.Attack} " +
            $"防御: {status.Defense} " +
            $"速度: {status.Speed}\n" +
            $"スキル: {skillNames}";
    }
}
