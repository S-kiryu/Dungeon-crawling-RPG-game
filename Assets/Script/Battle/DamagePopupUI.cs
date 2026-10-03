using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 実ダメージを上方向へ移動させながらフェードアウトする。
/// </summary>
public class DamagePopupUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _damageText;

    [SerializeField]
    [Min(0.01f)]
    private float _duration = 0.8f;

    [SerializeField]
    private float _moveDistance = 60f;

    private RectTransform _rectTransform;

    public void Show(int damage)
    {
        _rectTransform =
            GetComponent<RectTransform>();

        if (_damageText == null ||
            _rectTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        _damageText.text =
            damage.ToString();

        StartCoroutine(
            PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float elapsed = 0f;

        Vector2 startPosition =
            _rectTransform.anchoredPosition;

        Color startColor =
            _damageText.color;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;

            float rate = Mathf.Clamp01(
                elapsed / _duration);

            _rectTransform.anchoredPosition =
                startPosition +
                Vector2.up *
                (_moveDistance * rate);

            Color color = startColor;
            color.a = 1f - rate;
            _damageText.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}
