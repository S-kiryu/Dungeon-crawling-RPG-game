using UnityEngine;

/// <summary>
/// バトルシナリオのデータを管理するクラス
/// </summary>
[CreateAssetMenu(menuName = "Battle/Battle Scenario")]
public class BattleScenarioData : ScriptableObject
{
    public string ScenarioName;

    [Header("味方ユニット")]
    public UnitSettingData[] PlayerUnits;

    [Header("敵ユニット")]
    public UnitSettingData[] EnemyUnits;
}
