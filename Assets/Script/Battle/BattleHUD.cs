using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BattleManagerの状態を戦闘UIへ表示する。
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

    private void OnEnable()
    {
        if (_battleManager == null)
            return;

        _battleManager.TurnActionsChanged +=
            Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (_battleManager == null)
            return;

        _battleManager.TurnActionsChanged -=
            Refresh;
    }

    private void Refresh()
    {
        RefreshTurnInformation();
        RefreshActionSlots();
        RefreshBattleState();
        RefreshButtons();
    }

    private void RefreshTurnInformation()
    {
        if (_roundText != null)
        {
            _roundText.text =
                $"ラウンド {_battleManager.RoundCount}";
        }

        if (_currentUnitText == null)
            return;

        Unit currentUnit =
            _battleManager.CurrentTurnUnit;

        if (currentUnit == null)
        {
            _currentUnitText.text =
                "行動中: -";
            return;
        }

        string unitName =
            currentUnit.Data != null &&
            !string.IsNullOrWhiteSpace(
                currentUnit.Data.CharacterName)
                ? currentUnit.Data.CharacterName
                : currentUnit.name;

        _currentUnitText.text =
            $"行動中: {unitName}";
    }

    private void RefreshActionSlots()
    {
        TurnActionSlots slots =
            _battleManager.CurrentTurnSlots;

        if (slots == null)
        {
            SetText(
                _movementSlotText,
                "移動: -");

            SetText(
                _mainActionSlotText,
                "主行動: -");

            return;
        }

        int movement =
            slots.GetRemaining(
                ActionSlot.Movement);

        int mainAction =
            slots.GetRemaining(
                ActionSlot.Main);

        SetText(
            _movementSlotText,
            $"移動: {movement}");

        SetText(
            _mainActionSlotText,
            $"主行動: {mainAction}");
    }

    private void RefreshBattleState()
    {
        if (_battleStateText == null)
            return;

        _battleStateText.text =
            _battleManager.CurrentState switch
            {
                BattleState.PreparingTurn =>
                    "ターン準備中",

                BattleState.SelectCommand =>
                    "行動を選択してください",

                BattleState.SelectTarget =>
                    GetTargetSelectionText(),

                BattleState.ExecutingAction =>
                    "行動中",

                BattleState.EnemyTurn =>
                    "敵のターン",

                BattleState.BattleFinished =>
                    "戦闘終了",

                _ => string.Empty
            };
    }

    private string GetTargetSelectionText()
    {
        IBattleAction action =
            _battleManager.SelectedAction;

        if (action == null)
            return "対象を選択してください";

        return
            $"{action.DisplayName}の対象を選択";
    }

    private void RefreshButtons()
    {
        SetButtonInteractable(
            _moveButton,
            _battleManager.CanMove);

        SetButtonInteractable(
            _attackButton,
            _battleManager.CanAttack);

        SetButtonInteractable(
            _skillButton,
            _battleManager.CanUseSkill);

        SetButtonInteractable(
            _waitButton,
            _battleManager.CanWait);

        if (_cancelButton != null)
        {
            _cancelButton.gameObject.SetActive(
                _battleManager.CurrentState ==
                BattleState.SelectTarget);
        }
    }

    private void SetButtonInteractable(
        Button button,
        bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private void SetText(
        TMP_Text target,
        string value)
    {
        if (target != null)
            target.text = value;
    }
}