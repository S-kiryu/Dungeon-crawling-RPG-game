using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// グリットに指示を送るクラス
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private GridCell _gridPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Material _whiteMaterial;
    [SerializeField] private Material _grayMaterial;

    private GridCell[,] _grid;
    public GridCell[,] Grid => _grid;

    private float _cellSize;

    private void Awake()
    {
        _cellSize = _gridPrefab.GetComponentInChildren<Renderer>().bounds.size.x;
        GenerateGrid();
    }

    /// <summary>
    /// グリット生成の
    /// </summary>
    private void GenerateGrid()
    {
        _grid = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * _cellSize, 0, y * _cellSize);

                GridCell cell = Instantiate(_gridPrefab, position, Quaternion.identity);
                cell.Initialize(new Vector2Int(x, y));

                if ((x + y) % 2 == 0)
                {
                    cell.SetMaterial(_whiteMaterial);
                }
                else
                {
                    cell.SetMaterial(_grayMaterial);
                }

                _grid[x, y] = cell;
            }
        }
    }

    public bool TryGetCell(Vector2Int position, out GridCell cell)
    {
        cell = null;

        bool isOutOfRange =
            position.x < 0 || position.x >= width ||
            position.y < 0 || position.y >= height;

        if (isOutOfRange)
            return false;

        cell = _grid[position.x, position.y];
        return true;
    }

    public void SetUnit(int x,int y,Unit unit) 
    {
            _grid[x, y].TrySetUnit(unit);
    }

    public void RemoveUnit(int x, int y) 
    {
        _grid[x, y].RemoveUnit();
    }
}