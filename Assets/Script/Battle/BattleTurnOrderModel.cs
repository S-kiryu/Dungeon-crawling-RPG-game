using System.Collections.Generic;

/// <summary>
/// 速度順のラウンドと次の行動者を管理するModel。
/// </summary>
public sealed class BattleTurnOrderModel
{
    public UnitModel Current { get; private set; }
    public int RoundCount { get; private set; }

    public UnitModel Advance(IReadOnlyList<UnitModel> participants)
    {
        while (true)
        {
            _currentIndex++;

            if (_currentIndex >= _turnOrder.Count)
            {
                BuildRound(participants);
                _currentIndex++;

                if (_turnOrder.Count == 0)
                {
                    Current = null;
                    return null;
                }
            }

            Current = _turnOrder[_currentIndex];

            if (IsEligible(Current))
                return Current;
        }
    }

    public void ClearCurrent()
    {
        Current = null;
    }

    public List<UnitModel> GetUpcoming(int maximumCount)
    {
        List<UnitModel> result = new();

        if (maximumCount <= 0 ||
            _turnOrder.Count == 0 ||
            _currentIndex < 0)
        {
            return result;
        }

        AddUpcoming(
            result,
            _currentIndex,
            _turnOrder.Count,
            maximumCount);

        if (result.Count < maximumCount)
        {
            AddUpcoming(
                result,
                0,
                _turnOrder.Count,
                maximumCount);
        }

        return result;
    }

    private readonly List<UnitModel> _turnOrder = new();
    private int _currentIndex = -1;

    private void BuildRound(IReadOnlyList<UnitModel> participants)
    {
        _turnOrder.Clear();

        if (participants != null)
        {
            foreach (UnitModel participant in participants)
            {
                if (IsEligible(participant))
                {
                    _turnOrder.Add(participant);
                }
            }
        }

        _turnOrder.Sort((left, right) =>
            right.Status.Speed.CompareTo(left.Status.Speed));

        _currentIndex = -1;
        RoundCount++;
    }

    private void AddUpcoming(
        ICollection<UnitModel> result,
        int startIndex,
        int endIndex,
        int maximumCount)
    {
        for (int index = startIndex;
             index < endIndex && result.Count < maximumCount;
             index++)
        {
            UnitModel unit = _turnOrder[index];

            if (IsEligible(unit))
            {
                result.Add(unit);
            }
        }
    }

    private static bool IsEligible(UnitModel unit)
    {
        return unit != null &&
               !unit.IsDead &&
               unit.Team != TeamType.Neutral;
    }
}
