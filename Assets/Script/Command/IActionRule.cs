/// <summary>
/// 状態異常やステージ効果などによる行動制限。
/// </summary>
public interface IActionRule
{
    ActionAvailability Evaluate(
        IBattleAction action,
        BattleTurnContext context);
}
