using UnityEngine;

/// <summary>
/// バトルシナリオのデータを管理するクラス
/// </summary>
[CreateAssetMenu(menuName = "Battle/Battle Scenario")]
public class BattleScenarioData : ScriptableObject
{
    public string ScenarioName;
    public UnitSettingData[] PlayerUnits;
    public UnitSettingData[] EnemyUnits;
}