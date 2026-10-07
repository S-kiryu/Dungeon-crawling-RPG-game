using System;

public interface IBattleHudSource
{
    Unit CurrentTurnUnit { get; }
    int RoundCount { get; }
    BattleState CurrentState { get; }
    TurnActionSlots CurrentTurnSlots { get; }
    IBattleAction SelectedAction { get; }
    bool CanMove { get; }
    bool CanAttack { get; }
    bool CanUseSkill { get; }
    bool CanWait { get; }

    event Action TurnActionsChanged;
}
