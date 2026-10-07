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
        bool spawnedFormation = SpawnFormationUnits();

        // BattleSceneを直接再生した場合は、従来どおり
        // シナリオに設定された味方をテスト用として使用する。
        if (!spawnedFormation)
        {
            SpawnUnits(_scenario?.PlayerUnits);
        }

        SpawnUnits(_scenario?.EnemyUnits);

        UnitsReady?.Invoke();
    }

    /// <summary>
    /// 準備画面で編成したキャラクター個体を、シナリオの
    /// 味方配置座標へ生成する。
    /// </summary>
    private bool SpawnFormationUnits()
    {
        FormationManager formation =
            FormationManager.Instance;

        if (formation == null)
            return false;

        List<CharacterInstance> characters =
            formation.GetAssignedCharacters();
        UnitSettingData[] spawnPoints =
            _scenario?.PlayerUnits;

        if (characters.Count == 0 ||
            spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return false;
        }

        int spawnCount = Mathf.Min(
            characters.Count,
            spawnPoints.Length);
        bool spawnedAny = false;

        for (int index = 0; index < spawnCount; index++)
        {
            UnitSettingData spawnPoint = spawnPoints[index];

            if (spawnPoint == null)
                continue;

            if (!_gridManager.TryGetCell(
                    spawnPoint.GridPosition,
                    out GridCell cell))
            {
                continue;
            }

            Unit unit = _unitGenerator.Spawn(
                characters[index],
                cell);

            if (unit == null)
                continue;

            _units.Add(unit);
            spawnedAny = true;
        }

        if (characters.Count > spawnPoints.Length)
        {
            Debug.LogWarning(
                "編成人数に対して、シナリオの味方配置地点が不足しています。");
        }

        return spawnedAny;
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
