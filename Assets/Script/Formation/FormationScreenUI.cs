using UnityEngine;

public class FormationScreenUI :
    MonoBehaviour
{
    [SerializeField]
    private FormationManager
        _formationManager;

    [SerializeField]
    private FormationSlotUI[]
        _formationSlots;

    [SerializeField]
    private CharacterSelectionPanel
        _selectionPanel;

    private void Awake()
    {
        if (_formationManager == null)
        {
            _formationManager =
                FormationManager.Instance;
        }

        for (int slotIndex = 0;
             slotIndex <
             _formationSlots.Length;
             slotIndex++)
        {
            _formationSlots[slotIndex]
                .Setup(
                    slotIndex,
                    OpenCharacterSelection);
        }
    }

    private void OnEnable()
    {
        if (_formationManager == null)
        {
            _formationManager =
                FormationManager.Instance;
        }

        if (_formationManager == null)
            return;

        _formationManager.FormationChanged +=
            Refresh;

        _formationManager.PruneInvalidSlots();

        Refresh();
    }

    private void OnDisable()
    {
        if (_formationManager != null)
        {
            _formationManager
                .FormationChanged -=
                Refresh;
        }
    }

    private void OpenCharacterSelection(
        int slotIndex)
    {
        _selectionPanel.Open(
            slotIndex);
    }

    private void Refresh()
    {
        for (int slotIndex = 0;
             slotIndex <
             _formationSlots.Length;
             slotIndex++)
        {
            CharacterInstance character =
                _formationManager
                    .GetCharacterAt(
                        slotIndex);

            _formationSlots[slotIndex]
                .Refresh(character);
        }
    }
}
