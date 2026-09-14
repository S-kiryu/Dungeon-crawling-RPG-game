using System;
using System.Collections.Generic;

/// <summary>
/// 現在のターンで残っている行動枠を管理する。
/// </summary>
public sealed class TurnActionSlots
{
    private readonly Dictionary<ActionSlot, int>
        _remaining = new();

    public event Action Changed;

    /// <summary>
    /// 指定された行動枠の残り数を取得する。
    /// </summary>
    public int GetRemaining(ActionSlot slot)
    {
        return _remaining.TryGetValue(
            slot,
            out int amount)
            ? amount
            : 0;
    }

    /// <summary>
    /// 指定された行動枠の残り数が指定された量以上かどうかを判定する。
    public bool Has(ActionSlot slot, int amount = 1)
    {
        return amount >= 0 &&
               GetRemaining(slot) >= amount;
    }

    /// <summary>
    /// 指定された行動枠の残り数を設定する。
    /// </summary>
    public void Set(ActionSlot slot, int amount)
    {
        _remaining[slot] = Math.Max(0, amount);
        Changed?.Invoke();
    }

    public void Add(ActionSlot slot, int amount = 1)
    {
        if (amount <= 0)
            return;

        Set(slot, GetRemaining(slot) + amount);
    }

    /// <summary>
    /// 指定された行動枠の残り数を消費する。
    /// </summary>
    public bool TryConsume(ActionSlot slot, int amount = 1)
    {
        if (amount <= 0 || !Has(slot, amount))
            return false;

        Set(slot, GetRemaining(slot) - amount);
        return true;
    }

    public void Clear(ActionSlot slot)
    {
        Set(slot, 0);
    }
}
