/// <summary>
/// 対象へ与えるダメージを、行動実行前に計算できる行動。
/// </summary>
public interface IDamagePreviewAction
{
    bool TryGetDamagePreview(
        BattleTurnContext context,
        GridCell targetCell,
        out DamagePreview preview);
}
