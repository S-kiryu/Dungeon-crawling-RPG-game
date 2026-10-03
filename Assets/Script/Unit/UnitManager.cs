using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{

    public IReadOnlyList<Unit> Units => _units;

    public event Action UnitsReady;

    /// <summary>
    /// 指定したチームの生存しているユニットを取得
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<Unit> GetLivingUnits(
        TeamType team)
    {
        return _units.FindAll(unit =>
            unit != null &&
            !unit.IsDead &&
            unit.Team == team);
    }
    [SerializeField]
    private GridManager _gridManager;

    [SerializeField]
    private UnitGenerator _unitGenerator;

    [SerializeField]
    private BattleScenarioData _scenario;

    private readonly List<Unit> _units = new();

    private void Start()
    {
        SpawnUnits(_scenario.PlayerUnits);
        SpawnUnits(_scenario.EnemyUnits);

        UnitsReady?.Invoke();
    }

    /// <summary>
    /// 指定したユニット設定データに基づいてユニットを生成し、グリッド上に配置する
    /// </summary>
    /// <param name="settings"></param>
    private void SpawnUnits(
    UnitSettingData[] settings)
    {
        if (settings == null)
            return;

        foreach (UnitSettingData setting in settings)
        {
            if (setting == null ||
                setting.CharacterData == null)
            {
                continue;
            }

            if (!_gridManager.TryGetCell(
                    setting.GridPosition,
                    out GridCell cell))
            {
                continue;
            }

            Unit unit = _unitGenerator.Spawn(
                setting.CharacterData,
                cell);

            if (unit != null)
                _units.Add(unit);
        }
    }
}
