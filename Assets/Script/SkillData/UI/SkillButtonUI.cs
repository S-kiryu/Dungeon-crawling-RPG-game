using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// スキル一覧に表示するボタン。
/// </summary>
public class SkillButtonUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _descriptionText;

    private Action _onPointerEnter;
    private Action _onPointerExit;

    public void Setup(
        SkillData skill,
        Action onClicked,
        Action onPointerEnter,
        Action onPointerExit)
    {
        if (skill == null)
            return;

        _onPointerEnter = onPointerEnter;
        _onPointerExit = onPointerExit;

        if (_nameText != null)
        {
            _nameText.text =
                string.IsNullOrWhiteSpace(
                    skill.SkillName)
                    ? "名前なし"
                    : skill.SkillName;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text =
                skill.Description;
        }

        if (_iconImage != null)
        {
            _iconImage.sprite =
                skill.Icon;

            _iconImage.enabled =
                skill.Icon != null;
        }

        if (_button == null)
            return;

        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(
            () => onClicked?.Invoke());
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        _onPointerEnter?.Invoke();
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        _onPointerExit?.Invoke();
    }
}
