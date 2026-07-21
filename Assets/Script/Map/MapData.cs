using System.Collections.Generic;
using UnityEngine;

public class MapData
{
    /// <summary>
    /// 次のノードを設定する
    /// </summary>
    public void SetNextNode(List<List<MapNode>> columns)
    {
        for (int columnIndex = 0; columnIndex < columns.Count - 1; columnIndex++)
        {
            List<MapNode> currentColumn = columns[columnIndex];
            List<MapNode> nextColumn = columns[columnIndex + 1];

            // 1. 現在列の全ノードに、最低1本の出口を作る
            foreach (MapNode currentNode in currentColumn)
            {
                MapNode nextNode =
                    nextColumn[Random.Range(0, nextColumn.Count)];

                currentNode.AddNextNode(nextNode);
            }

            // 2. 次列の全ノードに、最低1本の入口を保証する
            foreach (MapNode nextNode in nextColumn)
            {
                bool hasIncomingPath = currentColumn.Exists(
                    currentNode => currentNode.NextNodes.Contains(nextNode));

                if (!hasIncomingPath)
                {
                    MapNode currentNode =
                        currentColumn[Random.Range(0, currentColumn.Count)];

                    currentNode.AddNextNode(nextNode);
                }
            }
        }
    }
}
