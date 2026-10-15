using UnityEngine;

public class BattleScenarioData : ScriptableObject
{
    public string ScenarioName;

    [Header("マップ表示")]
    public MapNodeDifficulty Difficulty =
        MapNodeDifficulty.Normal;

    [Min(0)]
    public int RewardGold = 50;

    [Header("味方ユニット")]
    public UnitSettingData[] PlayerUnits;

    [Header("敵ユニット")]
    public UnitSettingData[] EnemyUnits;
}
