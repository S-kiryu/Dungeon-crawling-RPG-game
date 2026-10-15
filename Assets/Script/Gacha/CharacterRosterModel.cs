using System;
using System.Collections.Generic;

/// <summary>
/// 所持キャラクターを管理するModel。
/// </summary>
public sealed class CharacterRosterModel
{
    public IReadOnlyList<CharacterInstance> Characters => _characters;

    public event Action Changed;

    public bool Add(CharacterInstance character)
    {
        if (character == null)
            return false;

        _characters.Add(character);
        Changed?.Invoke();
        return true;
    }

    public CharacterInstance FindById(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return null;

        return _characters.Find(character =>
            character != null &&
            character.InstanceId == instanceId);
    }

    public List<CharacterInstance> GetDeployableCharacters()
    {
        return _characters.FindAll(character =>
            character != null && character.CanDeploy);
    }

    public CharacterRosterModel(List<CharacterInstance> characters)
    {
        _characters = characters ??
            throw new ArgumentNullException(nameof(characters));
    }

    private readonly List<CharacterInstance> _characters;
}
