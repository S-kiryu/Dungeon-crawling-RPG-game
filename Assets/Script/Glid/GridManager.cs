using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// グリットに指示を送るクラス
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private GridCell _gridPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Material _whiteMaterial;
    [SerializeField] private Material _grayMaterial;
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _attackRangeMaterial;
    [SerializeField] private Material _attackTargetMaterial;

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

        if (distance > unit.Status.MoveLength)
            return false;

        currentCell.RemoveUnit();

        if (!targetCell.TrySetUnit(unit))
        {
            currentCell.TrySetUnit(unit);
            return false;
        }

        _battleManager.ChangeState(BattleState.Moving);

        unit.MoveTo(targetCell, () =>
        {
            _battleManager.ChangeState(BattleState.SelectAfterMoveCommand);
        });

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

    /// <summary>
    /// 各ステートのクリック判定
    /// </summary>
    /// <param name="clickedCell"></param>
    private void OnCellClicked(GridCell clickedCell)
    {
        switch (_battleManager.CurrentState)
        {
            //キャラ選択用のステート
            case BattleState.SelectUnit:
                if (!clickedCell.IsOccupied)
                    return;
                if (clickedCell.CurrentUnit.Team != TeamType.Player) 
                {
                    Debug.Log("playerじゃないよ");
                    return;
                }
                //移動選択のステートへ
                SelectUnit(clickedCell.CurrentUnit);
                break;

            //キャラを移動させるステート
            case BattleState.SelectMoveTarget:
                if (_selectedUnit == null)
                    return;

                GridCell previousCell = _selectedUnit.CurrentCell;

                if (TryMoveUnit(_selectedUnit, clickedCell.Position))
                {
                    SetDefaultMaterial(previousCell);

                    //攻撃か待機ができるステート
                    _battleManager.ChangeState(BattleState.SelectAfterMoveCommand);
                    ShowAttackRange(_selectedUnit);
                }
                break;

                //ボタンで呼ばれてる
                //敵を選択した時のステート
            case BattleState.SelectAttackTarget:
                if (_selectedUnit == null ||
                    _selectedUnit.RangeData == null ||
                    _selectedUnit.RangeData.Offsets == null ||
                    !clickedCell.IsOccupied)
                    return;

                Vector2Int targetOffset = clickedCell.Position - _selectedUnit.CurrentCell.Position;
                bool isInActionRange = false;

                foreach (Vector2Int offset in _selectedUnit.RangeData.Offsets)
                {
                    if (offset == targetOffset)
                    {
                        isInActionRange = true;
                        break;
                    }
                }

                if (!isInActionRange)
                    return;
                Debug.Log("攻撃対象選択");
                if (clickedCell.CurrentUnit.Team == TeamType.Enemy)
                {
                    Debug.Log("敵を選択した");
                    Debug.Log($"敵が攻撃食らう前{clickedCell.CurrentUnit.Status.CurrentHP}HP");
                    //攻撃ステートに移行
                    ClearAttackRange();
                    Debug.Log("攻撃！！！");
                    clickedCell.CurrentUnit.TakeDamage(_selectedUnit.Status.Attack);
                    //Debug.Log($"攻撃力{_selectedUnit.Status.Attack}\n食らったあと{clickedCell.CurrentUnit.Status.CurrentHP}HP");
                    _battleManager.ChangeState(BattleState.EnemyTurn);
                    return;
                }
                break;
            //AIステート
            case BattleState.EnemyTurn:
                break;
        }
    }

    public void ShowAttackRange(Unit unit)
    {
        if (unit == null || unit.RangeData == null)
            return;

        foreach (Vector2Int offset in unit.RangeData.Offsets)
        {
            Vector2Int position = unit.CurrentCell.Position + offset;

            if (!TryGetCell(position, out GridCell cell))
                continue;

            cell.SetMaterial(_attackRangeMaterial);

            // 敵だけを強調したい場合
            if (cell.IsOccupied && cell.CurrentUnit.Team == TeamType.Enemy)
            {
                cell.SetMaterial(_attackTargetMaterial);
            }
        }
    }

    private void ClearAttackRange()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SetDefaultMaterial(_grid[x, y]);
            }
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
