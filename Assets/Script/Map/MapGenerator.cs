using System.Collections.Generic;

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

            int width = _randomSource.Range(
                mapMinimumWidth,
                mapMaximumWidth + 1);

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

                SetupNodeInfo(node, i, mapLength);

                column.Add(node);
            }

            columns.Add(column);
        }
        return columns;
    }
    public MapGenerator(
        DungeonStageData stageData,
        IRandomSource randomSource = null)
    {
        _stageData = stageData;
        _randomSource = randomSource ?? new UnityRandomSource();
    }

    private readonly DungeonStageData _stageData;

    private readonly IRandomSource _randomSource;

    private void SetupNodeInfo(
        MapNode node,
        int columnIndex,
        int mapLength)
    {
        switch (node.EventType)
        {
            case MapEventType.Start:
                return;

            case MapEventType.Shop:
                return;

            case MapEventType.Break:
                return;

            case MapEventType.Boss:
                node.EnemyCount = 1;
                node.Difficulty = MapNodeDifficulty.Boss;
                node.RewardGold = 300;
                return;

            case MapEventType.Battle:
                node.BattleScenario = GetRandomMobScenario();

                node.EnemyCount =
                    node.BattleScenario?.EnemyUnits?.Length ?? 0;

                node.Difficulty =
                    node.BattleScenario?.Difficulty ??
                    MapNodeDifficulty.None;

                node.RewardGold =
                    node.BattleScenario?.RewardGold ?? 0;

                return;
        }
    }

    /// <summary>
    /// ランダムにモブのシナリオを渡す
    /// </summary>
    /// <returns></returns>
    private BattleScenarioData GetRandomMobScenario()
    {
        BattleScenarioData[] scenarios =
            _stageData?.MobScenarios;

        if (scenarios == null || scenarios.Length == 0)
            return null;

        int index = _randomSource.Range(
            0,
            scenarios.Length);

        return scenarios[index];
    }

    /// <summary>
    /// ランダムにマップのタイプを渡す
    /// </summary>
    /// <returns></returns>
    private MapEventType GetRandomEvent()
    {
        int random = _randomSource.Range(0, 100);

        if (random < 50)
            return MapEventType.Battle;


        if (random < 85)
            return MapEventType.Shop;

        return MapEventType.Break;
    }
}
