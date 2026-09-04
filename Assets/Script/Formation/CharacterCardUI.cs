using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// キャラクターのカードUIを表すクラス。
/// </summary>
public class CharacterCardUI : MonoBehaviour
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _characterImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _rarityText;

    [SerializeField]
    private TMP_Text _levelText;

    [SerializeField]
    private GameObject _selectedFrame;

    [SerializeField]
    private GameObject _formationMark;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField, Range(0f, 1f)]
    private float _inFormationAlpha = 0.5f;

    private CharacterInstance _character;

    private bool _isInFormation;

    public CharacterInstance Character =>
        _character;

    /// <summary>
    /// 表示するキャラクター情報を設定する。
    /// </summary>
    public void Setup(
        CharacterInstance character,
        bool isInFormation,
        Action<CharacterInstance> onClicked)
    {
        if (character == null)
            return;

        _character = character;
        _isInFormation = isInFormation;

        if (_characterImage != null)
        {
            _characterImage.sprite =
                character.CharacterData != null
                    ? character.CharacterData.Icon
                    : null;
        }

        if (_nameText != null)
        {
            _nameText.text =
                character.CharacterData != null
                    ? character.CharacterData.CharacterName
                    : "不明なキャラクター";
        }

        if (_rarityText != null)
        {
            _rarityText.text =
                character.Rarity.ToString();
        }

        if (_levelText != null)
        {
            _levelText.text =
                character.Status != null
                    ? $"Lv.{character.Status.Level}"
                    : "Lv.-";
        }

        if (_formationMark != null)
        {
            _formationMark.SetActive(
                isInFormation
            );
        }

        SetSelected(false);

        if (_button == null)
            return;

        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(
            () => onClicked?.Invoke(_character)
        );
    }

    /// <summary>
    /// カードの選択状態を設定する。
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (_selectedFrame != null)
        {
            _selectedFrame.SetActive(
                selected
            );
        }

        if (_canvasGroup != null)
        {
            // 選択中は編成済みでも濃く表示する。
            _canvasGroup.alpha =
                selected || !_isInFormation
                    ? 1f
                    : _inFormationAlpha;
        }
    }
}