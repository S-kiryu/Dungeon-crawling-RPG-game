using System.Collections.Generic;
using UnityEngine;

public class MapEventManager : MonoBehaviour
{
    [SerializeField] private List<EventBase> _events;

    private Dictionary<MapEventType, EventBase> _eventTable;

    private void Awake()
    {
        _eventTable = new Dictionary<MapEventType, EventBase>();

        foreach (EventBase mapEvent in _events)
        {
            _eventTable[mapEvent.EventType] = mapEvent;
        }
    }

    /// <summary>
    /// 指定のマップノードのイベントを実行する
    /// </summary>
    /// <param name="node">実行したいノード</param>
    public void Execute(MapNode node)
    {
        if (_eventTable.TryGetValue(node.EventType, out EventBase mapEvent))
        {
            mapEvent.Execute(node);
            return;
        }

        Debug.LogWarning($"{node.EventType} のイベントが未登録です。");
    }
}