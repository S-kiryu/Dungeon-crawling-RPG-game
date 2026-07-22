using UnityEngine;

public class BreakEvent : EventBase
{
    public override MapEventType EventType => MapEventType.Break;

    public override void Execute(MapNode node)
    {
        Debug.Log("休憩");
    }
}
