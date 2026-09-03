using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private CharacterInstance _character;

    public CharacterInstance Character =>
        _character;

    public void Setup(
        CharacterInstance character,
        bool isInFormation,
        Action<CharacterInstance> onClicked)
    {
        _character = character;

        _characterImage.sprite =
            character.CharacterData.Icon;

        _nameText.text =
            character
                .CharacterData
                .CharacterName;

        _rarityText.text =
            character.Rarity.ToString();

        _levelText.text =
            $"Lv.{character.Status.Level}";

        if (_formationMark != null)
        {
            _formationMark.SetActive(
                isInFormation);
        }

        SetSelected(false);

        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(
            () => onClicked?.Invoke(
                _character));
    }

    public void SetSelected(bool selected)
    {
        if (_selectedFrame != null)
        {
            _selectedFrame.SetActive(
                selected);
        }
    }
}