using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 一つのマップノードを描画するView。
/// </summary>
public class MapNodeUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Serializable]
    private sealed class EventVisual
    {
        public MapEventType EventType = MapEventType.Start;

        [Tooltip("ノード本体（背景・枠）の画像。未設定ならPrefabの画像を維持します。")]
        public Sprite NodeSprite = null;

        [Tooltip("ノード中央に表示するイベントアイコン。")]
        public Sprite EventIcon = null;

        [Tooltip("ホバー情報に表示するイメージ。未設定ならイベントアイコンを使います。")]
        public Sprite PreviewImage = null;
    }

    public void Setup(
        MapNode node,
        bool isCurrentNode,
        bool isSelectable,
        Action<MapNode> onSelected,
        Action<MapNode, Sprite> onHovered,
        Action<MapNode> onHoverEnded)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        ApplyVisual(node);
        _node = node;
        _onHovered = onHovered;
        _onHoverEnded = onHoverEnded;
        _eventText.text = node.EventType.ToString();
        _button.image.color = isCurrentNode ? Color.black : Color.white;
        _eventText.color = isCurrentNode ? Color.white : Color.black;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onSelected?.Invoke(node));

        SetSelectable(isSelectable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowInfo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideInfo();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowInfo();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HideInfo();
    }

    private void OnDisable()
    {
        HideInfo();
    }

    private void Awake()
    {
        if (_nodeImage == null && _button != null)
            _nodeImage = _button.image;

        if (_nodeImage != null)
            _prefabNodeSprite = _nodeImage.sprite;
    }

    private void ApplyVisual(MapNode node)
    {
        EventVisual visual = GetVisual(node.EventType);

        Sprite nodeSprite = node.NodeSprite != null
            ? node.NodeSprite
            : visual?.NodeSprite;

        if (_nodeImage != null)
            _nodeImage.sprite = nodeSprite != null
                ? nodeSprite
                : _prefabNodeSprite;

        Sprite eventIcon = node.EventIcon != null
            ? node.EventIcon
            : visual?.EventIcon;

        _previewImage = node.PreviewImage != null
            ? node.PreviewImage
            : visual?.PreviewImage != null
                ? visual.PreviewImage
                : eventIcon;

        if (_eventIconImage != null)
        {
            _eventIconImage.sprite = eventIcon;
            _eventIconImage.enabled = eventIcon != null;
        }

        if (_eventText != null)
        {
            bool hasIcon = eventIcon != null && _eventIconImage != null;
        }
    }

    private EventVisual GetVisual(MapEventType eventType)
    {
        foreach (EventVisual visual in _eventVisuals)
        {
            if (visual != null && visual.EventType == eventType)
                return visual;
        }

        return null;
    }

    private void ShowInfo()
    {
        if (_node != null)
            _onHovered?.Invoke(_node, _previewImage);
    }

    private void HideInfo()
    {
        if (_node != null)
            _onHoverEnded?.Invoke(_node);
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

    [Header("ノード表示")]

    [Tooltip("ノード本体（背景・枠）を表示するImage。未設定ならButtonのImageを使います。")]
    [SerializeField]
    private Image _nodeImage;

    [Tooltip("イベントアイコンを表示するImage。")]
    [SerializeField]
    private Image _eventIconImage;

    [Tooltip("アイコンがある場合もイベント名を表示します。")]
    [SerializeField]
    private bool _showEventNameWithIcon;

    [Tooltip("Start、Bossなど、イベント種別ごとの本体画像とアイコン設定。")]
    [SerializeField]
    private List<EventVisual> _eventVisuals = new();

    [Header("ノードの色")]
    
    
    [Tooltip("選択可能なノードの色")]
    [SerializeField]
    private Color _selectableColor = Color.white;

    [Tooltip("選択不可能なノードの色")]
    [SerializeField]
    private Color _unselectableColor = Color.black;

    [Tooltip("選択可能なノードのテキスト色")]
    [SerializeField]
    private Color _selectableTextColor = Color.black;

    [Tooltip("選択不可能なノードのテキスト色")]
    [SerializeField]
    private Color _unselectableTextColor = Color.white;

    private Sprite _prefabNodeSprite;
    private Sprite _previewImage;
    private MapNode _node;
    private Action<MapNode, Sprite> _onHovered;
    private Action<MapNode> _onHoverEnded;
}
