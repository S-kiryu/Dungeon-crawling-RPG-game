using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 戦闘状態をUIへ表示する。
/// 入力処理はBattleManagerが担当する。
/// </summary>
public class BattleHUD : MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    [Header("ターン情報")]
    [SerializeField]
    private TMP_Text _roundText;

    [SerializeField]
    private TMP_Text _currentUnitText;

    [SerializeField]
    private TMP_Text _hpText;

    [SerializeField]
    private TMP_Text _actionSlotText;

    [SerializeField]
    private TMP_Text _stateText;

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

    private void OnEnable()
    {
        if (_battleManager == null)
            return;

        _battleManager.TurnActionsChanged += Refresh;
        _battleManager.BattleEnded += HandleBattleEnded;

        Refresh();
    }

    private void OnDisable()
    {
        if (_battleManager == null)
            return;

        _battleManager.TurnActionsChanged -= Refresh;
        _battleManager.BattleEnded -= HandleBattleEnded;
    }

    private void Refresh()
    {
        RefreshTurnInformation();
        RefreshActionSlots();
        RefreshButtons();
        RefreshStateText();
    }

    private void RefreshTurnInformation()
    {
        if (_roundText != null)
        {
            _roundText.text =
                $"Round {_battleManager.RoundCount}";
        }

        Unit currentUnit =
            _battleManager.CurrentTurnUnit;

        if (currentUnit == null)
        {
            if (_currentUnitText != null)
                _currentUnitText.text = "行動中: -";

            if (_hpText != null)
                _hpText.text = "HP: -";

            return;
        }

        if (_currentUnitText != null)
        {
            _currentUnitText.text =
                $"行動中: {currentUnit.name}";
        }

        if (_hpText != null &&
            currentUnit.Status != null)
        {
            _hpText.text =
                $"HP: {currentUnit.Status.CurrentHP}/" +
                $"{currentUnit.Status.MaxHP}";
        }
    }

    private void RefreshActionSlots()
    {
        if (_actionSlotText == null)
            return;

        TurnActionSlots slots =
            _battleManager.CurrentTurnSlots;

        if (slots == null)
        {
            _actionSlotText.text =
                "移動: - / 主行動: -";
            return;
        }

        int movement =
            slots.GetRemaining(ActionSlot.Movement);

        int main =
            slots.GetRemaining(ActionSlot.Main);

        _actionSlotText.text =
            $"移動: {movement} / 主行動: {main}";
    }

    private void RefreshButtons()
    {
        if (_moveButton != null)
        {
            _moveButton.interactable =
                _battleManager.CanMove;
        }

        if (_attackButton != null)
        {
            _attackButton.interactable =
                _battleManager.CanAttack;
        }

        if (_skillButton != null)
        {
            _skillButton.interactable =
                _battleManager.CanUseSkill;
        }

        if (_waitButton != null)
        {
            _waitButton.interactable =
                _battleManager.CanWait;
        }

        if (_cancelButton != null)
        {
            _cancelButton.gameObject.SetActive(
                _battleManager.CurrentState ==
                BattleState.SelectTarget);
        }
    }

    private void RefreshStateText()
    {
        if (_stateText == null)
            return;

        _stateText.text =
            _battleManager.CurrentState switch
            {
                BattleState.PreparingTurn =>
                    "ターン準備中",

                BattleState.SelectCommand =>
                    "行動を選択してください",

                BattleState.SelectTarget =>
                    CreateTargetSelectionMessage(),

                BattleState.ExecutingAction =>
                    "行動中",

                BattleState.EnemyTurn =>
                    "敵のターン",

                BattleState.BattleFinished =>
                    "戦闘終了",

                _ => string.Empty
            };
    }

    private string CreateTargetSelectionMessage()
    {
        IBattleAction action =
            _battleManager.SelectedAction;

        return action == null
            ? "対象を選択してください"
            : $"{action.DisplayName}の対象を選択";
    }

    private void HandleBattleEnded(bool playerWon)
    {
        Refresh();

        SetButtonsInteractable(false);
    }

    private void SetButtonsInteractable(
        bool interactable)
    {
        if (_moveButton != null)
            _moveButton.interactable = interactable;

        if (_attackButton != null)
            _attackButton.interactable = interactable;

        if (_skillButton != null)
            _skillButton.interactable = interactable;

        if (_waitButton != null)
            _waitButton.interactable = interactable;

        if (_cancelButton != null)
            _cancelButton.interactable = interactable;
    }
}