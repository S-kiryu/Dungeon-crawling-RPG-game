using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private GridManager _gridManager;

    [SerializeField]
    private UnitGenerator _unitGenerator;

    [Header("プレイヤー初期配置")]
    [SerializeField]
    private Vector2Int[] _playerSpawnPositions;

    [Header("敵の初期配置")]
    [SerializeField]
    private UnitSettingData[] _enemyUnits;

    private readonly List<Unit> _units = new();

    public IReadOnlyList<Unit> Units => _units;

    public event Action UnitsReady;

    private void Start()
    {
        SpawnFormation();
        SpawnEnemies();

        UnitsReady?.Invoke();
    }

    private void SpawnFormation()
    {
        if (FormationManager.Instance == null)
        {
            Debug.LogError(
                "FormationManagerが存在しません。");
            return;
        }

        List<CharacterInstance> formation =
            FormationManager.Instance
                .GetAssignedCharacters();

        int spawnCount = Mathf.Min(
            formation.Count,
            _playerSpawnPositions.Length);

        for (int index = 0;
             index < spawnCount;
             index++)
        {
            if (!_gridManager.TryGetCell(
                    _playerSpawnPositions[index],
                    out GridCell cell))
            {
                Debug.LogWarning(
                    $"プレイヤー配置セルがありません: " +
                    $"{_playerSpawnPositions[index]}");
                continue;
            }

            Unit unit = _unitGenerator.Spawn(
                formation[index],
                cell);

            if (unit != null)
                _units.Add(unit);
        }
    }

    private void SpawnEnemies()
    {
        foreach (UnitSettingData setting
                 in _enemyUnits)
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
                Debug.LogWarning(
                    $"敵配置セルがありません: " +
                    $"{setting.GridPosition}");
                continue;
            }

            Unit unit = _unitGenerator.Spawn(
                setting.CharacterData,
                cell);

            if (unit != null)
                _units.Add(unit);
        }
    }

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
}