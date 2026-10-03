using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 一つのマップノードを描画するView。
/// </summary>
public class MapNodeUI : MonoBehaviour
{
    public void Setup(
        MapNode node,
        bool isCurrentNode,
        bool isSelectable,
        Action<MapNode> onSelected)
    {
        _eventText.text = node.EventType.ToString();
        _button.image.color = isCurrentNode ? Color.black : Color.white;
        _eventText.color = isCurrentNode ? Color.white : Color.black;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onSelected?.Invoke(node));

        SetSelectable(isSelectable);
    }

    public void SetSelectable(bool canSelect)
    {
        if (_button == null)
            return;

        ColorBlock colors = _button.colors;
        colors.normalColor = _selectableColor;
        colors.highlightedColor = _selectableColor;
        colors.selectedColor = _selectableColor;
        colors.disabledColor = _unselectableColor;

        _button.colors = colors;
        _button.interactable = canSelect;

        if (_eventText != null)
        {
            _eventText.color = canSelect
                ? _selectableTextColor
                : _unselectableTextColor;
        }
    }

    [SerializeField]
    private TMP_Text _eventText;

    [SerializeField]
    private Button _button;

    [Header("ノードの色")]
    [SerializeField]
    private Color _selectableColor = Color.white;

    [SerializeField]
    private Color _unselectableColor = Color.black;

    [SerializeField]
    private Color _selectableTextColor = Color.black;

    [SerializeField]
    private Color _unselectableTextColor = Color.white;
}
