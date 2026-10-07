using System;
using System.Collections.Generic;

/// <summary>
/// 出撃編成を管理するModel。
/// </summary>
public sealed class FormationModel
{
    public const int MaximumSlotCount = 4;

    public int SlotCount => MaximumSlotCount;
    public bool CanStartBattle => GetAssignedCharacters().Count > 0;

    public event Action Changed;

    public CharacterInstance GetCharacterAt(int slotIndex)
    {
        EnsureSlots();

        if (!IsValidSlot(slotIndex))
            return null;

        string instanceId = _formationIds[slotIndex];

        if (string.IsNullOrEmpty(instanceId))
            return null;

        CharacterInstance character = _findCharacter(instanceId);

        return character != null && character.CanDeploy
            ? character
            : null;
    }

    public List<CharacterInstance> GetAssignedCharacters()
    {
        EnsureSlots();
        List<CharacterInstance> characters = new();

        for (int slotIndex = 0;
             slotIndex < MaximumSlotCount;
             slotIndex++)
        {
            CharacterInstance character = GetCharacterAt(slotIndex);

            if (character != null)
            {
                characters.Add(character);
            }
        }

        return characters;
    }

    public bool TryAssignCharacter(
        int targetSlotIndex,
        CharacterInstance candidate)
    {
        EnsureSlots();
        PruneInvalidSlots();

        if (!IsValidSlot(targetSlotIndex) ||
            candidate == null ||
            !candidate.CanDeploy)
        {
            return false;
        }

        int currentSlotIndex = FindSlotIndex(candidate.InstanceId);

        if (currentSlotIndex == targetSlotIndex)
            return true;

        string targetCharacterId = _formationIds[targetSlotIndex];

        if (currentSlotIndex >= 0)
        {
            _formationIds[currentSlotIndex] = targetCharacterId;
        }

        _formationIds[targetSlotIndex] = candidate.InstanceId;
        Changed?.Invoke();
        return true;
    }

    public void ClearSlot(int slotIndex)
    {
        EnsureSlots();

        if (!IsValidSlot(slotIndex) ||
            string.IsNullOrEmpty(_formationIds[slotIndex]))
        {
            return;
        }

        _formationIds[slotIndex] = string.Empty;
        Changed?.Invoke();
    }

    public void PruneInvalidSlots()
    {
        EnsureSlots();
        bool changed = false;

        for (int slotIndex = 0;
             slotIndex < MaximumSlotCount;
             slotIndex++)
        {
            string instanceId = _formationIds[slotIndex];

            if (string.IsNullOrEmpty(instanceId))
                continue;

            CharacterInstance character = _findCharacter(instanceId);

            if (character != null && character.CanDeploy)
                continue;

            _formationIds[slotIndex] = string.Empty;
            changed = true;
        }

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    public FormationModel(
        List<string> formationIds,
        Func<string, CharacterInstance> findCharacter)
    {
        _formationIds = formationIds ??
            throw new ArgumentNullException(nameof(formationIds));
        _findCharacter = findCharacter ??
            throw new ArgumentNullException(nameof(findCharacter));

        EnsureSlots();
    }

    private readonly List<string> _formationIds;
    private readonly Func<string, CharacterInstance> _findCharacter;

    private int FindSlotIndex(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return -1;

        for (int slotIndex = 0;
             slotIndex < MaximumSlotCount;
             slotIndex++)
        {
            if (_formationIds[slotIndex] == instanceId)
                return slotIndex;
        }

        return -1;
    }

    private static bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 &&
               slotIndex < MaximumSlotCount;
    }

    private void EnsureSlots()
    {
        while (_formationIds.Count < MaximumSlotCount)
        {
            _formationIds.Add(string.Empty);
        }

        if (_formationIds.Count > MaximumSlotCount)
        {
            _formationIds.RemoveRange(
                MaximumSlotCount,
                _formationIds.Count - MaximumSlotCount);
        }
    }
}
