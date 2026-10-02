using UnityEngine;

/// <summary>
/// 現在行動中のユニットが持つスキルを一覧表示する。
/// </summary>
public class SkillSelectionPanel : MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    [Header("表示先")]
    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private Transform _contentRoot;

    [Header("生成するボタン")]
    [SerializeField]
    private SkillButtonUI _skillButtonPrefab;

    private void Awake()
    {
        Close();
    }

    private void OnEnable()
    {
        if (_battleManager == null)
            return;

        _battleManager.TurnActionsChanged +=
            HandleTurnActionsChanged;
    }

    private void OnDisable()
    {
        if (_battleManager == null)
            return;

        _battleManager.TurnActionsChanged -=
            HandleTurnActionsChanged;
    }

    /// <summary>
    /// スキルボタンから呼ぶ。
    /// </summary>
    public void Open()
    {
        if (_battleManager == null ||
            !_battleManager.CanUseSkill)
        {
            return;
        }

        Unit currentUnit =
            _battleManager.CurrentTurnUnit;

        if (currentUnit == null ||
            currentUnit.Skills.Count == 0)
        {
            return;
        }

        Rebuild(currentUnit);

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(true);
        }
    }

    /// <summary>
    /// 閉じるボタンから呼ぶ。
    /// </summary>
    public void Close()
    {
        ClearSkillPreview();

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private void Rebuild(
        Unit unit)
    {
        if (_contentRoot == null ||
            _skillButtonPrefab == null)
        {
            return;
        }

        ClearButtons();

        foreach (SkillData skill in
                 unit.Skills)
        {
            if (skill == null)
                continue;

            SkillData selectedSkill = skill;

            SkillButtonUI button =
                Instantiate(
                    _skillButtonPrefab,
                    _contentRoot);

            button.Setup(
                selectedSkill,

                () => SelectSkill(
                    selectedSkill),

                () => PreviewSkill(
                    selectedSkill),

                ClearSkillPreview);
        }
    }

    private void PreviewSkill(
        SkillData skill)
    {
        if (_battleManager != null)
        {
            _battleManager.PreviewSkillRange(
                skill);
        }
    }

    private void ClearSkillPreview()
    {
        if (_battleManager != null)
        {
            _battleManager
                .ClearSkillRangePreview();
        }
    }

    private void SelectSkill(
        SkillData skill)
    {
        if (_battleManager.TrySelectSkill(
                skill))
        {
            Close();
        }
    }

    private void ClearButtons()
    {
        for (int index =
                 _contentRoot.childCount - 1;
             index >= 0;
             index--)
        {
            Destroy(
                _contentRoot
                    .GetChild(index)
                    .gameObject);
        }
    }

    private void HandleTurnActionsChanged()
    {
        if (_battleManager.CurrentState !=
            BattleState.SelectCommand)
        {
            Close();
        }
    }
}
