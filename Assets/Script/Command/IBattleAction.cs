using System;

/// <summary>
/// プレイヤーがターン中に選択できる行動。
/// </summary>
public interface IBattleAction
{
    /// <summary>
    /// 行動の識別子
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 行動の表示名
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 行動のカテゴリ
    /// </summary>
    BattleActionCategory Category { get; }

    /// <summary>
    /// 行動がターゲットを必要とするかどうか
    /// </summary>
    bool RequiresTarget { get; }

    /// <summary>
    /// 行動が実行可能かどうかを判定する
    /// </summary>
    ActionAvailability GetAvailability(
        BattleTurnContext context);

    /// <summary>
    /// 行動のターゲット選択を開始する
    /// </summary>
    void BeginTargetSelection(
        BattleTurnContext context);

    /// <summary>
    /// 行動のターゲット選択を終了する
    /// </summary>
    BattleActionExecution TryExecute(
        BattleTurnContext context,
        GridCell target,
        Action onCompleted);
}

/// <summary>
/// 行動のカテゴリ
/// </summary>
public enum BattleActionCategory
{
    Movement,
    Attack,
    Skill,
    Utility
}

/// <summary>
/// 行動の実行結果
/// </summary>
public enum BattleActionExecution
{
    /// <summary>
    /// 行動が拒否されたことを示す
    /// </summary>
    Rejected,
    /// <summary>
    /// 行動が開始されたことを示す
    /// </summary>
    Started,
    /// <summary>
    /// 行動が完了したことを示す
    /// </summary>
    Completed,
    /// <summary>
    /// 行動がターン終了を示すことを示す
    /// </summary>
    EndTurn
}
