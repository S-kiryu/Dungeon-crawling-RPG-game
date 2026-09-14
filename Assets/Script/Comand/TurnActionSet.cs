using System;
using System.Collections.Generic;

/// <summary>
/// 現在のターンで利用できるアクションと制限ルールを管理する。
/// </summary>
public sealed class TurnActionSet
{
    private readonly List<IBattleAction> _actions = new();
    private readonly List<IActionRule> _rules = new();

    public IReadOnlyList<IBattleAction> Actions => _actions;

    public event Action Changed;

    public bool Add(IBattleAction action)
    {
        if (action == null || string.IsNullOrWhiteSpace(action.Id))
            return false;

        for (int index = 0; index < _actions.Count; index++)
        {
            if (_actions[index].Id != action.Id)
                continue;

            _actions[index] = action;
            Changed?.Invoke();
            return true;
        }

        _actions.Add(action);
        Changed?.Invoke();
        return true;
    }

    public bool Remove(string actionId)
    {
        for (int index = 0; index < _actions.Count; index++)
        {
            if (_actions[index].Id != actionId)
                continue;

            _actions.RemoveAt(index);
            Changed?.Invoke();
            return true;
        }

        return false;
    }

    public bool TryGet(
        string actionId,
        out IBattleAction action)
    {
        foreach (IBattleAction candidate in _actions)
        {
            if (candidate.Id != actionId)
                continue;

            action = candidate;
            return true;
        }

        action = null;
        return false;
    }

    public ActionAvailability GetAvailability(
        string actionId,
        BattleTurnContext context)
    {
        if (!TryGet(actionId, out IBattleAction action))
        {
            return ActionAvailability.Unavailable(
                "このターンでは使用できません。");
        }

        ActionAvailability availability =
            action.GetAvailability(context);

        if (!availability.CanExecute)
            return availability;

        foreach (IActionRule rule in _rules)
        {
            availability = rule.Evaluate(action, context);

            if (!availability.CanExecute)
                return availability;
        }

        return ActionAvailability.Available();
    }

    public bool AddRule(IActionRule rule)
    {
        if (rule == null || _rules.Contains(rule))
            return false;

        _rules.Add(rule);
        Changed?.Invoke();
        return true;
    }

    public bool RemoveRule(IActionRule rule)
    {
        if (!_rules.Remove(rule))
            return false;

        Changed?.Invoke();
        return true;
    }
}
