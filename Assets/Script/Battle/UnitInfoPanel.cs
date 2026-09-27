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
    private Slider _hpSlider;

    [SerializeField]
    private TMP_Text _hpText;

    [Header("ステータス")]
    [SerializeField]
    private TMP_Text _statusText;

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
    /// 現在選択されているユニットの情報を再表示する。
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

    /// <summary>
    /// ユニットの名前を更新する
    /// </summary>
    /// <param name="data"></param>
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

    /// <summary>
    /// ユニットのアイコンを更新する
    /// </summary>
    /// <param name="data"></param>
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

    /// <summary>
    /// ユニットのチームを更新する
    /// </summary>
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

    /// <summary>
    /// ユニットのHPを更新する
    /// </summary>
    /// <param name="status"></param>
    private void RefreshHP(
        CurrentStatus status)
    {
        if (_hpText != null)
        {
            _hpText.text =
                $"HP {status.CurrentHP} / " +
                $"{status.MaxHP}";
        }

        if (_hpSlider == null)
            return;

        _hpSlider.minValue = 0;
        _hpSlider.maxValue =
            Mathf.Max(1, status.MaxHP);

        _hpSlider.SetValueWithoutNotify(
            Mathf.Clamp(
                status.CurrentHP,
                0,
                status.MaxHP));
    }

    /// <summary>
    /// ユニットのステータスを更新する
    /// </summary>
    /// <param name="status"></param>
    private void RefreshStatus(
        CurrentStatus status)
    {
        if (_statusText == null)
            return;

        _statusText.text =
            $"攻撃　{status.Attack}\n" +
            $"防御　{status.Defense}\n" +
            $"速度　{status.Speed}\n" +
            $"移動　{status.MoveLength}";
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