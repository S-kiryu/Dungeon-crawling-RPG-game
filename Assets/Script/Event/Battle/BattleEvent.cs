using UnityEngine;

public class BattleEvent : EventBase
{
    public override MapEventType EventType => MapEventType.Battle;

    public override void Execute(MapNode node) 
    {
        Debug.Log("戦闘開始");
    }
}
