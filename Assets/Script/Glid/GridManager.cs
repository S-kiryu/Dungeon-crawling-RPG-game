using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private Material _selectedMaterial;

    private GridCell[,] _grid;
    public GridCell[,] Grid => _grid;

    private float _cellSize;
    private Unit _selectedUnit;

    private void Awake()
    {
        _cellSize = _gridPrefab.GetComponentInChildren<Renderer>().bounds.size.x;
        GenerateGrid();
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        //今押したところにレイキャストを飛ばしてグリットを取る
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycast missed");
            return;
        }

        Debug.Log($"Raycast hit: {hit.collider.gameObject.name}, layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}");

        GridCell cell = hit.collider.GetComponent<GridCell>();

        if (cell == null)
        {
            Debug.Log("Hit object has no GridCell");
            return;
        }

        OnCellClicked(cell);
    }

    /// <summary>
    /// グリット生成
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


    /// <summary>
    /// 指定したユニットをグリットに移動させる関数
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public bool TryMoveUnit(Unit unit, Vector2Int destination)
    {
        if (unit == null)
            return false;

        if (!TryGetCell(destination, out GridCell targetCell))
            return false;

        if (targetCell.IsOccupied)
            return false;

        GridCell currentCell = unit.CurrentCell;
        int distance = Mathf.Abs(currentCell.Position.x - destination.x)
                     + Mathf.Abs(currentCell.Position.y - destination.y);

        if (distance > unit.Status.MoveSpeed)
            return false;

        currentCell.RemoveUnit();

        if (!targetCell.TrySetUnit(unit))
        {
            currentCell.TrySetUnit(unit);
            return false;
        }

        unit.MoveTo(targetCell);
        return true;
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

    private void OnCellClicked(GridCell clickedCell)
    {
        if (clickedCell.IsOccupied)
        {
            SelectUnit(clickedCell.CurrentUnit);
            return;
        }

        if (_selectedUnit == null)
            return;

        GridCell previousCell = _selectedUnit.CurrentCell;
        if (TryMoveUnit(_selectedUnit, clickedCell.Position))
        {
            SetDefaultMaterial(previousCell);
            _selectedUnit = null;
        }
    }

    private void SelectUnit(Unit unit)
    {
        ClearSelection();
        _selectedUnit = unit;
        _selectedUnit.CurrentCell.SetMaterial(_selectedMaterial);
    }

    private void ClearSelection()
    {
        if (_selectedUnit == null)
            return;

        SetDefaultMaterial(_selectedUnit.CurrentCell);
        _selectedUnit = null;
    }

    private void SetDefaultMaterial(GridCell cell)
    {
        bool isWhite = (cell.Position.x + cell.Position.y) % 2 == 0;
        cell.SetMaterial(isWhite ? _whiteMaterial : _grayMaterial);
    }
}
