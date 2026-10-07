using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CharacterRosterModelをシーン間で保持するComposition Root。
/// </summary>
[DefaultExecutionOrder(-1000)]
public class CharacterRoster : MonoBehaviour
{
    public static CharacterRoster Instance { get; private set; }

    public CharacterRosterModel Model => _model;
    public IReadOnlyList<CharacterInstance> OwnedCharacters =>
        _model?.Characters ?? _ownedCharacters;

    public event Action Changed
    {
        add
        {
            if (_model != null)
                _model.Changed += value;
        }
        remove
        {
            if (_model != null)
                _model.Changed -= value;
        }
    }

    public bool Add(CharacterInstance character)
    {
        return _model != null && _model.Add(character);
    }

    public CharacterInstance FindById(string instanceId)
    {
        return _model?.FindById(instanceId);
    }

    public List<CharacterInstance> GetDeployableCharacters()
    {
        return _model?.GetDeployableCharacters() ?? new List<CharacterInstance>();
    }

    [SerializeField]
    private List<CharacterInstance> _ownedCharacters = new();

    private CharacterRosterModel _model;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _model = new CharacterRosterModel(_ownedCharacters);
        DontDestroyOnLoad(gameObject);
    }
}
