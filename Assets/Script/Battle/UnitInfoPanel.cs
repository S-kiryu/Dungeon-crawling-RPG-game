using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択されたユニットの情報を表示するUI。
/// 入力処理はBattleManagerが担当する。
/// </summary>
public class UnitInfoPanel : MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    [Header("表示領域")]
    [SerializeField]
    private GameObject _panelRoot;

    [Header("基本情報")]
    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _teamText;

    [Header("HP")]
    [SerializeField]
    private Image _hpFillImage;

    [SerializeField]
    private TMP_Text _hpText;

    [SerializeField]
    private Color _healthyHPColor =
        new Color(0.2f, 0.8f, 0.2f);

    [SerializeField]
    private Color _warningHPColor =
        new Color(1f, 0.7f, 0.1f);

    [SerializeField]
    private Color _dangerHPColor =
        new Color(0.9f, 0.15f, 0.15f);

    [Header("ステータス")]
    [SerializeField]
    private TMP_Text _levelText;

    [SerializeField]
    private TMP_Text _attackText;

    [SerializeField]
    private TMP_Text _defenseText;

    [SerializeField]
    private TMP_Text _speedText;

    [SerializeField]
    private TMP_Text _moveText;

    [SerializeField]
    private TMP_Text _weightText;

    private Unit _displayedUnit;

    private void OnEnable()
    {
        if (_battleManager == null)
        {
            HidePanel();
            return;
        }

        _battleManager.InspectedUnitChanged +=
            HandleInspectedUnitChanged;

        _battleManager.TurnActionsChanged +=
            Refresh;

        HandleInspectedUnitChanged(
            _battleManager.InspectedUnit);
    }

    private void OnDisable()
    {
        if (_battleManager == null)
            return;

        _battleManager.InspectedUnitChanged -=
            HandleInspectedUnitChanged;

        _battleManager.TurnActionsChanged -=
            Refresh;
    }

    /// <summary>
    /// 情報表示対象が変更されたときに呼ばれる。
    /// </summary>
    private void HandleInspectedUnitChanged(
        Unit unit)
    {
        _displayedUnit = unit;

        if (_displayedUnit == null)
        {
            HidePanel();
            return;
        }

        ShowPanel();
        Refresh();
    }

    /// <summary>
    /// 選択中ユニットの情報を再表示する。
    /// </summary>
    private void Refresh()
    {
        if (_displayedUnit == null ||
            _displayedUnit.Status == null)
        {
            HidePanel();
            return;
        }

        CharacterData data =
            _displayedUnit.Data;

        CurrentStatus status =
            _displayedUnit.Status;

        RefreshName(data);
        RefreshIcon(data);
        RefreshTeam();
        RefreshHP(status);
        RefreshStatus(status);
    }

    private void RefreshName(
        CharacterData data)
    {
        if (_nameText == null)
            return;

        if (data != null &&
            !string.IsNullOrWhiteSpace(
                data.CharacterName))
        {
            _nameText.text =
                data.CharacterName;
            return;
        }

        _nameText.text =
            _displayedUnit.name;
    }

    private void RefreshIcon(
        CharacterData data)
    {
        if (_iconImage == null)
            return;

        Sprite icon =
            data != null
                ? data.Icon
                : null;

        _iconImage.sprite = icon;
        _iconImage.enabled = icon != null;
        _iconImage.preserveAspect = true;
    }

    private void RefreshTeam()
    {
        if (_teamText == null)
            return;

        _teamText.text =
            _displayedUnit.Team switch
            {
                TeamType.Player => "味方",
                TeamType.Enemy => "敵",
                TeamType.Neutral => "中立",
                _ => "不明"
            };
    }

    private void RefreshHP(
        CurrentStatus status)
    {
        int maximumHP =
            Mathf.Max(1, status.MaxHP);

        int currentHP =
            Mathf.Clamp(
                status.CurrentHP,
                0,
                maximumHP);

        float hpRate =
            (float)currentHP /
            maximumHP;

        if (_hpText != null)
        {
            _hpText.text =
                $"{currentHP} / {maximumHP}";
        }

        if (_hpFillImage == null)
            return;

        _hpFillImage.fillAmount =
            hpRate;

        _hpFillImage.color =
            GetHPColor(hpRate);
    }

    private Color GetHPColor(
        float hpRate)
    {
        if (hpRate <= 0.25f)
            return _dangerHPColor;

        if (hpRate <= 0.5f)
            return _warningHPColor;

        return _healthyHPColor;
    }

    private void RefreshStatus(
        CurrentStatus status)
    {
        SetText(
            _levelText,
            $"Lv. {status.Level}");

        SetText(
            _attackText,
            $"攻撃{status.Attack}");

        SetText(
            _defenseText,
            $"防御{status.Defense}");

        SetText(
            _speedText,
            $"素早さ{status.Speed}");

        SetText(
            _moveText,
            $"移動力{status.MoveLength}");

        SetText(
            _weightText,
            $"重量{status.Weight}");
    }

    private void SetText(
        TMP_Text targetText,
        string value)
    {
        if (targetText != null)
            targetText.text = value;
    }

    private void ShowPanel()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(true);
    }

    private void HidePanel()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }
}
