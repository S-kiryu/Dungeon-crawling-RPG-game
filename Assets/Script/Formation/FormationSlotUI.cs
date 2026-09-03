using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormationSlotUI : MonoBehaviour
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _characterImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _detailText;

    [SerializeField]
    private GameObject _emptyDisplay;

    private int _slotIndex;

    /// <summary>
    /// キャラを指定のインデックスにセットする
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="onClicked"></param>
    public void Setup(
        int slotIndex,
        Action<int> onClicked)
    {
        _slotIndex = slotIndex;

        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(
            () => onClicked?.Invoke(
                _slotIndex));
    }

    public void Refresh(
        CharacterInstance character)
    {
        bool hasCharacter =
            character != null;

        if (_emptyDisplay != null)
        {
            _emptyDisplay.SetActive(
                !hasCharacter);
        }

        if (_characterImage != null)
        {
            _characterImage.enabled =
                hasCharacter;

            _characterImage.sprite =
                hasCharacter
                    ? character
                        .CharacterData.Icon
                    : null;
        }

        if (_nameText != null)
        {
            _nameText.text =
                hasCharacter
                    ? character
                        .CharacterData
                        .CharacterName
                    : "空き";
        }

        if (_detailText != null)
        {
            _detailText.text =
                hasCharacter
                    ? CreateDetailText(character)
                    : string.Empty;
        }
    }

    /// <summary>
    /// そのキャラの情報
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    private string CreateDetailText(
        CharacterInstance character)
    {
        return
            $"{character.Rarity} " +
            $"Lv.{character.Status.Level}\n" +
            $"HP " +
            $"{character.Status.CurrentHP}/" +
            $"{character.Status.MaxHP}";
    }
}