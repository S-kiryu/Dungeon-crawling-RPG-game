using UnityEngine;
using UnityEngine.SceneManagement;

public class BossEvent : EventBase
{
    public override MapEventType EventType =>
        MapEventType.Boss;
    
    public override void Execute(
        MapNode node)
    {
        if (FormationManager.Instance == null ||
            !FormationManager.Instance.CanStartBattle)
        {
            Debug.LogWarning(
                "出撃可能な編成がありません。");

            DungeonRunSession.Instance
                ?.CancelPendingNode();

            return;
        }

        SceneManager.LoadScene(
            "BattleScene");
    }
}