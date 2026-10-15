using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 行動順一覧に表示するユニット1体分のUI。
/// </summary>
public class TurnOrderEntryUI : MonoBehaviour
{

    public void Setup(
        Unit unit,
        bool isCurrentTurn)
    {
        if (unit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        CharacterData data = unit.Data;

        if (_iconImage != null)
        {
            Sprite icon =
                data != null
                    ? data.Icon
                    : null;

            _iconImage.sprite = icon;
            _iconImage.enabled = icon != null;
            _iconImage.preserveAspect = true;
        }

        if (_nameText != null)
        {
            _nameText.text =
                data != null &&
                !string.IsNullOrWhiteSpace(
                    data.CharacterName)
                    ? data.CharacterName
                    : unit.name;
        }

        if (_teamBackground != null)
        {
            _teamBackground.color =
                unit.Team == TeamType.Player
                    ? _playerColor
                    : _enemyColor;
        }

        if (_currentTurnFrame != null)
        {
            _currentTurnFrame.SetActive(
                isCurrentTurn);
        }
    }
    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private Image _teamBackground;

    [SerializeField]
    private GameObject _currentTurnFrame;

    [Header("チーム色")]
    [SerializeField]
    private Color _playerColor =
        new Color(0.2f, 0.55f, 1f, 1f);

    [SerializeField]
    private Color _enemyColor =
        new Color(1f, 0.25f, 0.2f, 1f);
}
