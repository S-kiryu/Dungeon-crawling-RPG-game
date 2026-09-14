/// <summary>
/// アクションが現在実行可能か
/// </summary>
public readonly struct ActionAvailability
{
    public bool CanExecute { get; }
    public string Reason { get; }

    private ActionAvailability(
        bool canExecute,
        string reason)
    {
        CanExecute = canExecute;
        Reason = reason;
    }

    /// <summary>
    /// アクションが実行可能であることを示す
    /// </summary>
    /// <returns></returns>
    public static ActionAvailability Available()
    {
        return new ActionAvailability(true, string.Empty);
    }

    /// <summary>
    /// アクションが実行不可能であることを示す
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static ActionAvailability Unavailable(
        string reason)
    {
        return new ActionAvailability(false, reason);
    }
}
