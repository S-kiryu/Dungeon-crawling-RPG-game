using UnityEngine;

/// <summary>
/// セル単体が持っている情報を管理するクラス
/// </summary>
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

    /// <summary>
    /// セルにユニットを配置する
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool TrySetUnit(Unit unit)
    {
        if (unit == null || IsOccupied)
            return false;

        CurrentUnit = unit;
        return true;
    }

    /// <summary>
    /// セルからユニットを削除する
    /// </summary>
    public void RemoveUnit()
    {
            CurrentUnit = null;
    }

    /// <summary>
    /// セルのマテリアルを設定する
    /// </summary>
    /// <param name="material"></param>
    public void SetMaterial(Material material)
    {
        _renderer.material = material;
    }
}
