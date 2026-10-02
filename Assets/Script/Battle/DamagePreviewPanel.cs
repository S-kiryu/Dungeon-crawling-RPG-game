using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 対象マスにカーソルを合わせたとき、予測ダメージを表示する。
/// </summary>
public class DamagePreviewPanel : MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    [SerializeField]
    private GridManager _gridManager;

    [Header("表示領域")]
    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private CanvasGroup _panelCanvasGroup;

    [SerializeField]
    private RectTransform _panelRect;

    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private Vector2 _pointerOffset =
        new Vector2(24f, -24f);

    [Header("表示テキスト")]
    [SerializeField]
    private TMP_Text _damageText;

    [SerializeField]
    private TMP_Text _hpText;

    private GridCell _hoveredCell;

    private void Awake()
    {
        if (_panelCanvasGroup == null &&
            _panelRoot != null)
        {
            _panelCanvasGroup =
                _panelRoot.GetComponent<CanvasGroup>();

            if (_panelCanvasGroup == null)
            {
                _panelCanvasGroup =
                    _panelRoot.AddComponent<CanvasGroup>();
            }
        }

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (_gridManager != null)
        {
            _gridManager.HoveredCellChanged +=
                HandleHoveredCellChanged;
        }

        if (_battleManager != null)
        {
            _battleManager.TurnActionsChanged +=
                Refresh;
        }

        Hide();
    }

    private void OnDisable()
    {
        if (_gridManager != null)
        {
            _gridManager.HoveredCellChanged -=
                HandleHoveredCellChanged;
        }

        if (_battleManager != null)
        {
            _battleManager.TurnActionsChanged -=
                Refresh;
        }
    }

    private void LateUpdate()
    {
        if (_panelCanvasGroup == null ||
            _panelCanvasGroup.alpha <= 0f)
        {
            return;
        }

        UpdatePanelPosition();
    }

    private void HandleHoveredCellChanged(
        GridCell cell)
    {
        _hoveredCell = cell;
        Refresh();
    }

    private void Refresh()
    {
        if (_battleManager == null ||
            _hoveredCell == null ||
            !_battleManager
                .TryGetSelectedDamagePreview(
                    _hoveredCell,
                    out DamagePreview preview))
        {
            Hide();
            return;
        }

        SetVisible(true);

        if (_damageText != null)
        {
            _damageText.text =
                $"予測ダメージ {preview.Damage}";
        }

        if (_hpText != null)
        {
            _hpText.text =
                $"HP {preview.CurrentHP}" +
                $" → {preview.RemainingHP}";
        }

        UpdatePanelPosition();
    }

    private void UpdatePanelPosition()
    {
        if (_panelRect == null ||
            _canvas == null ||
            Mouse.current == null)
        {
            return;
        }

        RectTransform canvasRect =
            _canvas.transform as RectTransform;

        if (canvasRect == null)
            return;

        Vector2 screenPosition =
            Mouse.current.position.ReadValue() +
            _pointerOffset;

        Camera canvasCamera =
            _canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPosition))
        {
            _panelRect.anchoredPosition =
                localPosition;
        }
    }

    private void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (_panelRoot != null &&
            !_panelRoot.activeSelf)
        {
            _panelRoot.SetActive(true);
        }

        if (_panelCanvasGroup == null)
            return;

        _panelCanvasGroup.alpha =
            visible ? 1f : 0f;

        _panelCanvasGroup.interactable = false;
        _panelCanvasGroup.blocksRaycasts = false;
    }
}
