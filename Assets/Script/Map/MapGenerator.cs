using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    /// <summary>
    /// マップ生成の関数
    /// </summary>
    /// <param name="columns">マップのノードを入れる型</param>
    /// <param name="mapLength">マップの長さ</param>
    /// <param name="mapMinimumWidth">マップの横の最小値</param>
    /// <param name="mapMaximumWidth">マップの横の最大値</param>
    public List<List<MapNode>> GenerateMap(
    int mapLength,
    int mapMinimumWidth,
    int mapMaximumWidth)
    {
        List<List<MapNode>> columns = new();
        for (int i = 0; i < mapLength; i++)
        {
            List<MapNode> column = new();

            int width = Random.Range(mapMinimumWidth, mapMaximumWidth + 1);

            // 最初と最後は1マス固定
            if (i == 0 || i == mapLength - 1)
            {
                width = 1;
            }

            for (int j = 0; j < width; j++)
            {
                MapNode node = new MapNode();

                if (i == 0)
                    node.EventType = MapEventType.Start;
                else if (i == mapLength - 1)
                    node.EventType = MapEventType.Boss;
                else
                    node.EventType = GetRandomEvent();

                column.Add(node);
            }

            columns.Add(column);
        }
        return columns;
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
