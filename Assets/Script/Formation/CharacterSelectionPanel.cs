using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionPanel :
    MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private FormationManager _formationManager;

    [SerializeField]
    private CharacterCardUI _cardPrefab;

    [SerializeField]
    private Transform _cardParent;

    [Header("ボタン")]
    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    [SerializeField]
    private Button _removeButton;

    [Header("選択キャラ詳細")]
    [SerializeField]
    private Image _selectedCharacterImage;

    [SerializeField]
    private TMP_Text _selectedCharacterName;

    [SerializeField]
    private TMP_Text _selectedCharacterStatus;

    private readonly List<CharacterCardUI>
        _generatedCards = new();

    private int _editingSlotIndex = -1;

    private CharacterInstance
        _selectedCharacter;

    private void Awake()
    {
        _confirmButton.onClick.AddListener(
            Confirm);

        _cancelButton.onClick.AddListener(
            Cancel);

        _removeButton.onClick.AddListener(
            RemoveCharacter);

        gameObject.SetActive(false);
    }

    public void Open(int slotIndex)
    {
        if (_formationManager == null)
        {
            _formationManager =
                FormationManager.Instance;
        }

        _editingSlotIndex =
            slotIndex;

        _selectedCharacter =
            _formationManager
                .GetCharacterAt(slotIndex);

        gameObject.SetActive(true);

        GenerateCharacterCards();
        RefreshSelection();
    }

    private void GenerateCharacterCards()
    {
        ClearCharacterCards();

        if (CharacterRoster.Instance == null)
            return;

        List<CharacterInstance> characters =
            CharacterRoster.Instance
                .GetDeployableCharacters();

        List<CharacterInstance> formation =
            _formationManager
                .GetAssignedCharacters();

        foreach (CharacterInstance character
                 in characters)
        {
            CharacterCardUI card =
                Instantiate(
                    _cardPrefab,
                    _cardParent);

            bool isInFormation =
                formation.Contains(character);

            card.Setup(
                character,
                isInFormation,
                SelectCharacter);

            _generatedCards.Add(card);
        }
    }

    private void ClearCharacterCards()
    {
        foreach (CharacterCardUI card
                 in _generatedCards)
        {
            if (card != null)
            {
                Destroy(
                    card.gameObject);
            }
        }

        _generatedCards.Clear();
    }

    private void SelectCharacter(
        CharacterInstance character)
    {
        _selectedCharacter =
            character;

        RefreshSelection();
    }

    private void RefreshSelection()
    {
        foreach (CharacterCardUI card
                 in _generatedCards)
        {
            card.SetSelected(
                card.Character ==
                _selectedCharacter);
        }

        bool hasSelection =
            _selectedCharacter != null;

        _confirmButton.interactable =
            hasSelection;

        if (_selectedCharacterImage != null)
        {
            _selectedCharacterImage.enabled =
                hasSelection;

            _selectedCharacterImage.sprite =
                hasSelection
                    ? _selectedCharacter
                        .CharacterData.Icon
                    : null;
        }

        if (_selectedCharacterName != null)
        {
            _selectedCharacterName.text =
                hasSelection
                    ? _selectedCharacter
                        .CharacterData
                        .CharacterName
                    : "キャラを選択";
        }

        if (_selectedCharacterStatus != null)
        {
            _selectedCharacterStatus.text =
                hasSelection
                    ? CreateStatusText(
                        _selectedCharacter)
                    : string.Empty;
        }
    }

    private string CreateStatusText(
        CharacterInstance character)
    {
        CurrentStatus status =
            character.Status;

        return
            $"{character.Rarity}\n" +
            $"Lv.{status.Level}\n" +
            $"体力 {status.CurrentHP}/" +
            $"{status.MaxHP}\n" +
            $"攻撃 {status.Attack}\n" +
            $"防御 {status.Defense}\n" +
            $"素早さ {status.Speed}";
    }

    private void Confirm()
    {
        if (_selectedCharacter == null ||
            _editingSlotIndex < 0)
        {
            return;
        }

        bool assigned =
            _formationManager
                .TryAssignCharacter(
                    _editingSlotIndex,
                    _selectedCharacter);

        if (assigned)
            Close();
    }

    private void RemoveCharacter()
    {
        if (_editingSlotIndex >= 0)
        {
            _formationManager.ClearSlot(
                _editingSlotIndex);
        }

        Close();
    }

    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        _editingSlotIndex = -1;
        _selectedCharacter = null;

        ClearCharacterCards();

        gameObject.SetActive(false);
    }
}