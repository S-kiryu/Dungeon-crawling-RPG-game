using UnityEngine;

public class ShopEvent : EventBase
{
    public override MapEventType EventType => MapEventType.Shop;

    public override void Execute(MapNode node)
    {
        Debug.Log("お店");
    }
}
