using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップ上のノードを描画するUIクラス。
/// </summary>
public class MapNodeUI : MonoBehaviour
{
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

    /// <summary>
    /// UIを初期化する。
    /// </summary>
    public void Setup(
        MapNode node,
        MapManager mapManager)
    {
        _eventText.text =
            node.EventType.ToString();

        bool isCurrentNode =
            node == mapManager.CurrentNode;

        // 現在地だけ黒くする
        _button.image.color =
            isCurrentNode
                ? Color.black
                : Color.white;

        _eventText.color =
            isCurrentNode
                ? Color.white
                : Color.black;

        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(
            () => mapManager.SelectNode(node)
        );
    }

    /// <summary>
    /// ノードの選択可否と見た目を変更する。
    /// </summary>
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
            _eventText.color =
                canSelect
                    ? _selectableTextColor
                    : _unselectableTextColor;
        }
    }
}