using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 編成を管理するクラス
/// </summary>
[DefaultExecutionOrder(-900)]
public class FormationManager : MonoBehaviour
{
    private const int FormationSize = 4;

    public static FormationManager Instance
    {
        get;
        private set;
    }

    [SerializeField]
    private List<string> _formationIds = new();

    public int SlotCount => FormationSize;

    public bool CanStartBattle =>
        GetAssignedCharacters().Count >= 1;

    /// <summary>
    /// 編成が変わったことを感知する
    /// </summary>
    public event Action FormationChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EnsureSlots();

        DontDestroyOnLoad(
            gameObject);
    }

    /// <summary>
    /// 指定した編成にいるユニットを取得
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public CharacterInstance GetCharacterAt(
        int slotIndex)
    {
        EnsureSlots();

        if (!IsValidSlot(slotIndex))
            return null;

        string instanceId =
            _formationIds[slotIndex];

        if (string.IsNullOrEmpty(instanceId))
            return null;

        if (CharacterRoster.Instance == null)
            return null;

        CharacterInstance character =
            CharacterRoster.Instance.FindById(
                instanceId);

        if (character == null ||
            !character.CanDeploy)
        {
            return null;
        }

        return character;
    }

    /// <summary>
    /// 編成キャラを全員取得
    /// </summary>
    /// <returns></returns>
    public List<CharacterInstance>
        GetAssignedCharacters()
    {
        EnsureSlots();

        List<CharacterInstance> characters =
            new();

        for (int slotIndex = 0;
             slotIndex < FormationSize;
             slotIndex++)
        {
            CharacterInstance character =
                GetCharacterAt(slotIndex);

            if (character != null)
                characters.Add(character);
        }

        return characters;
    }

    /// <summary>
    /// 選択したキャラを編成に入れる
    /// </summary>
    /// <param name="targetSlotIndex"></param>
    /// <param name="candidate"></param>
    /// <returns></returns>
    public bool TryAssignCharacter(
        int targetSlotIndex,
        CharacterInstance candidate)
    {
        EnsureSlots();
        PruneInvalidSlots();

        if (!IsValidSlot(targetSlotIndex) ||
            candidate == null ||
            !candidate.CanDeploy)
        {
            return false;
        }

        int currentSlotIndex =
            FindSlotIndex(candidate.InstanceId);

        // すでに同じ枠にいる
        if (currentSlotIndex ==
            targetSlotIndex)
        {
            return true;
        }

        string targetCharacterId =
            _formationIds[targetSlotIndex];

        if (currentSlotIndex >= 0)
        {
            // 候補が別枠にいた場合は入れ替える
            _formationIds[currentSlotIndex] =
                targetCharacterId;
        }

        _formationIds[targetSlotIndex] =
            candidate.InstanceId;

        FormationChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// スロットを空にする
    /// </summary>
    /// <param name="slotIndex"></param>
    public void ClearSlot(int slotIndex)
    {
        EnsureSlots();

        if (!IsValidSlot(slotIndex))
            return;

        if (string.IsNullOrEmpty(
                _formationIds[slotIndex]))
        {
            return;
        }

        _formationIds[slotIndex] =
            string.Empty;

        FormationChanged?.Invoke();
    }

    /// <summary>
    /// 死んでいるキャラなどを取り除くためのもの
    /// </summary>
    public void PruneInvalidSlots()
    {
        EnsureSlots();

        if (CharacterRoster.Instance == null)
            return;

        bool changed = false;

        for (int slotIndex = 0;
             slotIndex < FormationSize;
             slotIndex++)
        {
            string instanceId =
                _formationIds[slotIndex];

            if (string.IsNullOrEmpty(instanceId))
                continue;

            CharacterInstance character =
                CharacterRoster.Instance.FindById(
                    instanceId);

            if (character != null &&
                character.CanDeploy)
            {
                continue;
            }

            _formationIds[slotIndex] =
                string.Empty;

            changed = true;
        }

        if (changed)
            FormationChanged?.Invoke();
    }

    /// <summary>
    /// そのキャラが何番目のスロットにいるのか探す
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    private int FindSlotIndex(
        string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return -1;

        for (int slotIndex = 0;
             slotIndex < FormationSize;
             slotIndex++)
        {
            if (_formationIds[slotIndex] ==
                instanceId)
            {
                return slotIndex;
            }
        }

        return -1;
    }

    private bool IsValidSlot(int slotIndex)
    {
        return
            slotIndex >= 0 &&
            slotIndex < FormationSize;
    }

    private void EnsureSlots()
    {
        while (_formationIds.Count <
               FormationSize)
        {
            _formationIds.Add(
                string.Empty);
        }

        if (_formationIds.Count >
            FormationSize)
        {
            _formationIds.RemoveRange(
                FormationSize,
                _formationIds.Count -
                FormationSize);
        }
    }
}