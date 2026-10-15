using System;

/// <summary>
/// 編成Modelを編成画面へ反映するPresenter。
/// </summary>
public sealed class FormationScreenPresenter
{

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _model.Changed += Refresh;
        _view.SlotSelected += HandleSlotSelected;
        _model.PruneInvalidSlots();
        Refresh();
    }

    public void Deactivate()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _model.Changed -= Refresh;
        _view.SlotSelected -= HandleSlotSelected;
    }
    public FormationScreenPresenter(
        FormationModel model,
        IFormationScreenView view)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    private readonly FormationModel _model;
    private readonly IFormationScreenView _view;
    private bool _isActive;

    private void HandleSlotSelected(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _model.SlotCount)
            return;

        _view.ShowCharacterSelection(slotIndex);
    }

    private void Refresh()
    {
        for (int slotIndex = 0;
             slotIndex < _model.SlotCount;
             slotIndex++)
        {
            _view.ShowSlot(
                slotIndex,
                _model.GetCharacterAt(slotIndex));
        }
    }
}
