using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// マップ上の戦闘イベントを表すクラス
/// </summary>
public class BattleEvent : EventBase
{
    public override MapEventType EventType =>
        MapEventType.Battle;

    public override void Execute(MapNode node)
    {
        if (FormationManager.Instance == null ||
            !FormationManager.Instance.CanStartBattle)
        {
            Debug.LogWarning(
                "出撃可能な編成がありません。");
            return;
        }

        SceneManager.LoadScene("BattleScene");
    }
}