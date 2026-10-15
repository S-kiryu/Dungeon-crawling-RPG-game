using System;

public interface IFormationScreenView
{
    event Action<int> SlotSelected;

    void ShowCharacterSelection(int slotIndex);
    void ShowSlot(int slotIndex, CharacterInstance character);
}
