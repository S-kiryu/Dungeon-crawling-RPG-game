using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 戦闘HUDの描画だけを担当するView兼Composition Root。
/// </summary>
public class BattleHUD : MonoBehaviour, IBattleHudView
{
    public void Render(BattleHudState state)
    {
        SetText(_roundText, state.RoundText);
        SetText(_currentUnitText, state.CurrentUnitText);
        SetText(_battleStateText, state.BattleStateText);
        SetText(_movementSlotText, state.MovementSlotText);
        SetText(_mainActionSlotText, state.MainActionSlotText);

        SetButtonInteractable(_moveButton, state.CanMove);
        SetButtonInteractable(_attackButton, state.CanAttack);
        SetButtonInteractable(_skillButton, state.CanUseSkill);
        SetButtonInteractable(_waitButton, state.CanWait);

        if (_cancelButton != null)
        {
            _cancelButton.gameObject.SetActive(state.ShowCancel);
        }
    }

    [SerializeField]
    private BattleManager _battleManager;

    [Header("ターン情報")]
    [SerializeField]
    private TMP_Text _roundText;

    [SerializeField]
    private TMP_Text _currentUnitText;

    [SerializeField]
    private TMP_Text _battleStateText;

    [Header("行動枠")]
    [SerializeField]
    private TMP_Text _movementSlotText;

    [SerializeField]
    private TMP_Text _mainActionSlotText;

    [Header("コマンドボタン")]
    [SerializeField]
    private Button _moveButton;

    [SerializeField]
    private Button _attackButton;

    [SerializeField]
    private Button _skillButton;

    [SerializeField]
    private Button _waitButton;

    [SerializeField]
    private Button _cancelButton;

    private BattleHudPresenter _presenter;

    private void OnEnable()
    {
        if (_battleManager == null)
            return;

        _presenter ??= new BattleHudPresenter(_battleManager, this);
        _presenter.Activate();
    }

    private void OnDisable()
    {
        _presenter?.Deactivate();
    }

    private static void SetButtonInteractable(
        Button button,
        bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private static void SetText(
        TMP_Text target,
        string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
