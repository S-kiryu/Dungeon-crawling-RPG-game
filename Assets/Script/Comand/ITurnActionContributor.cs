/// <summary>
/// 装備、バフ、ステージ効果などからターン構成を変更する拡張点。
/// UnitのMonoBehaviourに実装すると、ターン開始時に自動で呼ばれる。
/// </summary>
public interface ITurnActionContributor
{
    void ConfigureTurn(BattleTurnContext context);
}
