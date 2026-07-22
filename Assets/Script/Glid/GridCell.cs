using System;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    public Vector2Int Position { get; private set; }

    // public Unit CurrentUnit { get; private set; }

    public TerrainType Terrain { get; private set; }

    public void Initialize(Vector2Int position)
    {
        Position = position;
    }

    public void SetMaterial(Material material) 
    {
        _renderer.material = material;
    }
}