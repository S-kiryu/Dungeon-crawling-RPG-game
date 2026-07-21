using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ノードデータ
/// </summary>
public class MapNode
{
    public MapEventType EventType;
    public List<MapNode> NextNodes { get; } = new();

    /// <summary>
    /// マップのタイプといける次のノードを設定する
    /// </summary>
    /// <param name="eventType">タイプ</param>
    /// <param name="nextNodes">次のノード</param>
    public void AddNextNode(MapNode nextNodes)
    {
        NextNodes.Add(nextNodes);
    }
}
