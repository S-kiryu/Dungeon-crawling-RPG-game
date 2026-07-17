using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ノードデータ
/// </summary>
public class MapNode : MonoBehaviour
{
    private MapEventType _eventType;
    private List<MapNode> _NextNodes;

    /// <summary>
    /// マップのタイプといける次のノードを設定する
    /// </summary>
    /// <param name="eventType">タイプ</param>
    /// <param name="nextNodes">次のノード</param>
    public MapNode(MapEventType eventType, List<MapNode>[] nextNodes)
    {
        _NextNodes = new List<MapNode>();

        _eventType = eventType;
        foreach (var node in nextNodes)
        {
            _NextNodes.AddRange(node);
        }
    }
}
