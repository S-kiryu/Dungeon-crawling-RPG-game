using UnityEngine;

/// <summary>
/// ユニットが受けた実ダメージを、画面上のポップアップとして生成する。
/// </summary>
public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private DamagePopupUI _popupPrefab;

    [SerializeField]
    private Vector3 _worldOffset =
        new Vector3(0f, 1.5f, 0f);

    private void OnEnable()
    {
        Unit.AnyUnitDamaged +=
            HandleUnitDamaged;
    }

    private void OnDisable()
    {
        Unit.AnyUnitDamaged -=
            HandleUnitDamaged;
    }

    private void HandleUnitDamaged(
        Unit unit,
        int damage)
    {
        if (unit == null ||
            _canvas == null ||
            _popupPrefab == null ||
            Camera.main == null)
        {
            return;
        }

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(
                unit.transform.position +
                _worldOffset);

        if (screenPosition.z < 0f)
            return;

        RectTransform canvasRect =
            _canvas.transform as RectTransform;

        if (canvasRect == null)
            return;

        Camera canvasCamera =
            _canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPosition))
        {
            return;
        }

        DamagePopupUI popup =
            Instantiate(
                _popupPrefab,
                canvasRect);

        RectTransform popupRect =
            popup.transform as RectTransform;

        if (popupRect == null)
        {
            Destroy(popup.gameObject);
            return;
        }

        popupRect.anchoredPosition =
            localPosition;

        popup.Show(damage);
    }
}
