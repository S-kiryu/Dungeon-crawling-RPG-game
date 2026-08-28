using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitGenerator _unitGenerator;

    [Header("使うユニットを設定する場所")]
    [SerializeField] private UnitSettingData[] _initialUnits;

    [Header("スキルをセットする場所")]

    private readonly List<Unit> _units = new();

    public IReadOnlyList<Unit> Units => _units;
    public event System.Action UnitsReady;

    private void Start()
    {
        SetUP();
        UnitsReady?.Invoke();
    }

    /// <summary>
    /// 初期ユニットを生成して配置します。
    /// </summary>
    private void SetUP() 
    {
        foreach (UnitSettingData setting in _initialUnits)
        {
            if (!_gridManager.TryGetCell(setting.GridPosition, out GridCell cell))
            {
                Debug.LogWarning($"セルがありません: {setting.GridPosition}");
                continue;
            }

            Unit unit = _unitGenerator.Spawn(setting.CharacterData, cell);

            if (unit != null)
                _units.Add(unit);
        }
    }

    /// <summary>
    /// 指定されたチームの生存しているユニットを取得します。
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<Unit> GetLivingUnits(TeamType team)
    {
        return _units.FindAll(unit =>
            unit != null &&
            !unit.IsDead &&
            unit.Team == team);
    }
}
