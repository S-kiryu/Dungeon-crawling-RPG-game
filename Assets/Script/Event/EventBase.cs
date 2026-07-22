using UnityEngine;

public abstract class EventBase : MonoBehaviour
{
    public abstract MapEventType EventType { get; }

    public abstract void Execute(MapNode node);
}