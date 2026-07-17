using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ノードデータ
/// </summary>
public class MapNode : MonoBehaviour
{
    public MapEventType EventType;
    private List<MapNode> _NextNodes = new();

    /// <summary>
    /// マップのタイプといける次のノードを設定する
    /// </summary>
    /// <param name="eventType">タイプ</param>
    /// <param name="nextNodes">次のノード</param>
    public void AddNextNode(MapNode nextNodes)
    {
        _NextNodes.Add(nextNodes);
    }
}
