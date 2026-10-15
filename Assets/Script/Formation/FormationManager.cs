using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FormationModelを保持するComposition Root兼、既存シーン向けFacade。
/// </summary>
[DefaultExecutionOrder(-900)]
public class FormationManager : MonoBehaviour
{
    public static FormationManager Instance { get; private set; }

    public FormationModel Model => _model;
    public int SlotCount => _model?.SlotCount ?? FormationModel.MaximumSlotCount;
    public bool CanStartBattle => _model != null && _model.CanStartBattle;

    public event Action FormationChanged
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

    public CharacterInstance GetCharacterAt(int slotIndex)
    {
        return _model?.GetCharacterAt(slotIndex);
    }

    public List<CharacterInstance> GetAssignedCharacters()
    {
        return _model?.GetAssignedCharacters() ??
               new List<CharacterInstance>();
    }

    public bool TryAssignCharacter(
        int targetSlotIndex,
        CharacterInstance candidate)
    {
        return _model != null &&
               _model.TryAssignCharacter(targetSlotIndex, candidate);
    }

    public void ClearSlot(int slotIndex)
    {
        _model?.ClearSlot(slotIndex);
    }

    public void PruneInvalidSlots()
    {
        _model?.PruneInvalidSlots();
    }

    [SerializeField]
    private List<string> _formationIds = new();

    private FormationModel _model;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _model = new FormationModel(_formationIds, FindCharacter);
        DontDestroyOnLoad(gameObject);
    }

    private static CharacterInstance FindCharacter(string instanceId)
    {
        return CharacterRoster.Instance?.FindById(instanceId);
    }
}
