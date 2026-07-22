using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private UnitSpawnSetting[] _initialUnits;

    private readonly List<Unit> _units = new();

    private void Start()
    {
        SetUP();
    }

    private void SetUP() 
    {
        foreach (UnitSpawnSetting setting in _initialUnits)
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
}
