using UnityEngine;

public class MapLineUI : MonoBehaviour
{
    [SerializeField] private float _thickness = 5f;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Draw(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;

        _rectTransform.anchoredPosition = (from + to) / 2f;
        _rectTransform.sizeDelta =
            new Vector2(direction.magnitude, _thickness);

        float angle = Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        _rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }
}