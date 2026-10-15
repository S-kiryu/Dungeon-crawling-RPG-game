using System;
using UnityEngine;

/// <summary>
/// 編成スロットの入力と描画を担当するView兼Composition Root。
/// </summary>
public class FormationScreenUI : MonoBehaviour, IFormationScreenView
{
    public event Action<int> SlotSelected;

    public void ShowCharacterSelection(int slotIndex)
    {
        _selectionPanel.Open(slotIndex);
    }

    public void ShowSlot(
        int slotIndex,
        CharacterInstance character)
    {
        if (slotIndex < 0 || slotIndex >= _formationSlots.Length)
            return;

        _formationSlots[slotIndex].Refresh(character);
    }

    [SerializeField]
    private FormationManager _formationManager;

    [SerializeField]
    private FormationSlotUI[] _formationSlots;

    [SerializeField]
    private CharacterSelectionPanel _selectionPanel;

    private FormationScreenPresenter _presenter;

    private void Awake()
    {
        for (int slotIndex = 0;
             slotIndex < _formationSlots.Length;
             slotIndex++)
        {
            _formationSlots[slotIndex].Setup(
                slotIndex,
                HandleSlotSelected);
        }
    }

    private void OnEnable()
    {
        if (_formationManager == null)
        {
            _formationManager = FormationManager.Instance;
        }

        if (_formationManager?.Model == null)
            return;

        _presenter ??= new FormationScreenPresenter(
            _formationManager.Model,
            this);

        _presenter.Activate();
    }

    private void OnDisable()
    {
        _presenter?.Deactivate();
    }

    private void HandleSlotSelected(int slotIndex)
    {
        SlotSelected?.Invoke(slotIndex);
    }
}
