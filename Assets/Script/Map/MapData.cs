using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapData : MonoBehaviour
{
    [SerializeField, Tooltip("ボスまで何歩で行けるのか")] private int _mapLength;
    [SerializeField, Tooltip("最小の横幅")] private int _mapMinimumWidth;
    [SerializeField, Tooltip("最大の横幅")] private int _mapMaximumWidth;

    //生成したノードを管理するリスト
    private List<List<MapNode>> _columns;

    /// <summary>
    /// マップの生成関数
    /// </summary>
    public void GenerateMap(List<List<MapNode>> colums)
    {
        colums = new List<List<MapNode>>();
        for (int i = 0; i < _mapLength; i++)
        {
            List<MapNode> column = new();

            int width = Random.Range(_mapMinimumWidth, _mapMaximumWidth + 1);

            // 最初と最後は1マス固定
            if (i == 0 || i == _mapLength - 1)
            {
                width = 1;
            }

            for (int j = 0; j < width; j++)
            {
                MapNode node = new MapNode();

                if (i == 0)
                    node.EventType = MapEventType.Start;
                else if (i == _mapLength - 1)
                    node.EventType = MapEventType.Boss;
                else
                    node.EventType = GetRandomEvent();

                column.Add(node);
            }

            colums.Add(column);
        }
        SetNextNode(colums);
    }

    /// <summary>
    /// 次のノードを設定する
    /// </summary>
    private void SetNextNode(List<List<MapNode>> colums)
    {
        for (int i = 0; i < colums.Count-1; i++)
        {
            if (colums.Count == i) return;

            List<MapNode> column = colums[i];
            List<MapNode> nextColumn = colums[i + 1];

            for (int j = 0; j < column.Count; j++)
            {
                int random = Random.Range(0, nextColumn.Count);
                
                column[j].AddNextNode(nextColumn[random]);
            }
        }
    }

    /// <summary>
    /// ランダムにマップのタイプを渡す
    /// </summary>
    /// <returns></returns>
    private MapEventType GetRandomEvent()
    {
        int random = Random.Range(0, 100);

        if (random < 50)
            return MapEventType.Battle;


        if (random < 85)
            return MapEventType.Shop;

        return MapEventType.Break;
    }
}
