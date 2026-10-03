using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 現在以降の行動順を一覧表示する。
/// </summary>
public class TurnOrderUI : MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    [SerializeField]
    private Transform _contentRoot;

    [SerializeField]
    private TurnOrderEntryUI _entryPrefab;

    [SerializeField]
    [Min(1)]
    private int _maximumVisibleCount = 6;

    private readonly List<TurnOrderEntryUI>
        _entries = new();

    private void OnEnable()
    {
        if (_battleManager != null)
        {
            _battleManager.TurnOrderChanged +=
                Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (_battleManager != null)
        {
            _battleManager.TurnOrderChanged -=
                Refresh;
        }
    }

    private void Refresh()
    {
        if (_battleManager == null ||
            _contentRoot == null ||
            _entryPrefab == null)
        {
            return;
        }

        List<Unit> upcomingUnits =
            _battleManager
                .GetUpcomingTurnUnits(
                    _maximumVisibleCount);

        EnsureEntryCount(
            upcomingUnits.Count);

        for (int index = 0;
             index < _entries.Count;
             index++)
        {
            if (index >= upcomingUnits.Count)
            {
                _entries[index]
                    .gameObject
                    .SetActive(false);

                continue;
            }

            Unit unit = upcomingUnits[index];

            bool isCurrentTurn =
                index == 0 &&
                unit ==
                _battleManager.CurrentTurnUnit;

            _entries[index].Setup(
                unit,
                isCurrentTurn);
        }
    }

    private void EnsureEntryCount(
        int requiredCount)
    {
        while (_entries.Count <
               requiredCount)
        {
            TurnOrderEntryUI entry =
                Instantiate(
                    _entryPrefab,
                    _contentRoot);

            _entries.Add(entry);
        }
    }
}
