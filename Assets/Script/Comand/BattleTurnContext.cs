using System;

/// <summary>
/// 1体の行動ターンに必要な情報をまとめた実行コンテキスト。
/// </summary>
public sealed class BattleTurnContext
{
    public Unit Actor { get; }
    public GridManager Grid { get; }
    public TurnActionSlots Slots { get; }
    public TurnActionSet Actions { get; }

    /// <summary>
    /// 1体のユニットの行動ターンに必要な情報をまとめた実行コンテキスト
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="grid"></param>
    /// <param name="slots"></param>
    /// <param name="actions"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public BattleTurnContext(
        Unit actor,
        GridManager grid,
        TurnActionSlots slots,
        TurnActionSet actions)
    {
        Actor = actor != null
            ? actor
            : throw new ArgumentNullException(nameof(actor));

        Grid = grid != null
            ? grid
            : throw new ArgumentNullException(nameof(grid));

        Slots = slots ??
            throw new ArgumentNullException(nameof(slots));

        Actions = actions ??
            throw new ArgumentNullException(nameof(actions));
    }
}
