public interface IBattleHudView
{
    void Render(BattleHudState state);
}

public readonly struct BattleHudState
{
    public string RoundText { get; }
    public string CurrentUnitText { get; }
    public string BattleStateText { get; }
    public string MovementSlotText { get; }
    public string MainActionSlotText { get; }
    public bool CanMove { get; }
    public bool CanAttack { get; }
    public bool CanUseSkill { get; }
    public bool CanWait { get; }
    public bool ShowCancel { get; }

    public BattleHudState(
        string roundText,
        string currentUnitText,
        string battleStateText,
        string movementSlotText,
        string mainActionSlotText,
        bool canMove,
        bool canAttack,
        bool canUseSkill,
        bool canWait,
        bool showCancel)
    {
        RoundText = roundText;
        CurrentUnitText = currentUnitText;
        BattleStateText = battleStateText;
        MovementSlotText = movementSlotText;
        MainActionSlotText = mainActionSlotText;
        CanMove = canMove;
        CanAttack = canAttack;
        CanUseSkill = canUseSkill;
        CanWait = canWait;
        ShowCancel = showCancel;
    }
}
