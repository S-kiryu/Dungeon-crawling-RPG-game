using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーが所持するキャラクター個体をシーン間で保持する。
/// </summary>
[DefaultExecutionOrder(-1000)]
public class CharacterRoster : MonoBehaviour
{
    public static CharacterRoster Instance
    {
        get;
        private set;
    }

    [SerializeField]
    private List<CharacterInstance> _ownedCharacters = new();

    public IReadOnlyList<CharacterInstance> OwnedCharacters =>
        _ownedCharacters;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Add(CharacterInstance character)
    {
        if (character == null)
            return false;

        _ownedCharacters.Add(character);
        return true;
    }

    public CharacterInstance FindById(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return null;

        return _ownedCharacters.Find(character =>
            character != null &&
            character.InstanceId == instanceId);
    }

    public List<CharacterInstance> GetDeployableCharacters()
    {
        return _ownedCharacters.FindAll(character =>
            character != null &&
            character.CanDeploy);
    }
}
