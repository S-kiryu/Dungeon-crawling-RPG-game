using System;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    public Vector2Int Position { get; private set; }

    public Unit CurrentUnit { get; private set; }

    public TerrainType Terrain { get; private set; }

    public bool IsOccupied => CurrentUnit != null;

    public void Initialize(Vector2Int position)
    {
        Position = position;
    }

    public bool TrySetUnit(Unit unit)
    {
        if (unit == null || IsOccupied)
            return false;

        CurrentUnit = unit;
        return true;
    }

    public void RemoveUnit()
    {
            CurrentUnit = null;
    }

    public void SetMaterial(Material material)
    {
        _renderer.material = material;
    }
}