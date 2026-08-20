using System.Collections;
using System.Collections.Generic;
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

    private static readonly Vector2Int[] Directions =
{
    Vector2Int.up,
    Vector2Int.down,
    Vector2Int.left,
    Vector2Int.right
};

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
    public bool TryMoveUnit(
        Unit unit,
        Vector2Int destination,
        System.Action onComplete = null)
    {
        if (unit == null)
            return false;
        //移動先のグリットが存在するか、または移動先が占有されていないかを確認
        if (!TryGetCell(destination, out GridCell targetCell))
            return false;
        if (targetCell.IsOccupied)
            return false;

        GridCell currentCell = unit.CurrentCell;

        // BFSで実際に通れる経路を探す
        if (!TryFindPath(
                currentCell,
                targetCell,
                unit,
                out List<GridCell> path))
        {
            return false;
        }

        // pathには現在地も含まれるため、移動距離は-1
        int moveDistance = path.Count - 1;

        if (moveDistance > unit.Status.MoveLength)
            return false;

        currentCell.RemoveUnit();

        if (!targetCell.TrySetUnit(unit))
        {
            currentCell.TrySetUnit(unit);
            return false;
        }

        unit.MoveAlongPath(path, onComplete);

        return true;
    }

    /// <summary>
    /// BFSで経路探索を行う関数
    /// </summary>
    /// <param name="startCell"></param>
    /// <param name="destinationCell"></param>
    /// <param name="movingUnit"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool TryFindPath(
    GridCell startCell,
    GridCell destinationCell,
    Unit movingUnit,
    out List<GridCell> path)
    {
        path = null;

        if (startCell == null || destinationCell == null)
            return false;

        Queue<GridCell> searchQueue = new();

        // Key：調査したセル
        // Value：そのセルへ来る直前にいたセル
        Dictionary<GridCell, GridCell> previousCells = new();

        searchQueue.Enqueue(startCell);
        previousCells[startCell] = null;

        while (searchQueue.Count > 0)
        {
            GridCell currentCell = searchQueue.Dequeue();

            if (currentCell == destinationCell)
            {
                path = BuildPath(
                    previousCells,
                    destinationCell
                );

                return true;
            }

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int nextPosition =
                    currentCell.Position + direction;

                if (!TryGetCell(
                        nextPosition,
                        out GridCell nextCell))
                {
                    continue;
                }

                // すでに調査したセル
                if (previousCells.ContainsKey(nextCell))
                    continue;

                // 壁は通れない
                if (nextCell.Terrain == TerrainType.Wall)
                    continue;

                // 他のユニットがいるセルは通れない
                if (nextCell.IsOccupied &&
                    nextCell.CurrentUnit != movingUnit)
                {
                    continue;
                }

                previousCells[nextCell] = currentCell;
                searchQueue.Enqueue(nextCell);
            }
        }

        // 目的地までの経路がなかった
        return false;
    }

    private List<GridCell> BuildPath(
    Dictionary<GridCell, GridCell> previousCells,
    GridCell destinationCell)
    {
        List<GridCell> path = new();

        GridCell currentCell = destinationCell;

        while (currentCell != null)
        {
            path.Add(currentCell);
            currentCell = previousCells[currentCell];
        }

        // 目的地→現在地になっているので反転する
        path.Reverse();

        return path;
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
                _battleManager.ChangeState(BattleState.SetMove);
                break;

            //キャラを移動させるステート
            case BattleState.SelectMoveTarget:
                if (_selectedUnit == null)
                    return;

                GridCell previousCell = _selectedUnit.CurrentCell;
                Unit movingUnit = _selectedUnit;

                bool startedMoving = TryMoveUnit(
                    movingUnit,
                    clickedCell.Position,
                    () =>
                    {
                        // 移動完了後に攻撃・待機を選択可能にする
                        _battleManager.ChangeState(
                            BattleState.SelectAfterMoveCommand
                        );

                        ShowAttackRange(movingUnit);
                    }
                );

                if (startedMoving)
                {
                    SetDefaultMaterial(previousCell);

                    _battleManager.ChangeState(
                        BattleState.Moving
                    );
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

                //攻撃範囲内かどうかの判定
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
                Unit targetUnit = clickedCell.CurrentUnit;

                if (targetUnit.Team != TeamType.Enemy)
                    return;

                _battleManager.ChangeState(BattleState.Attacking);

                // 攻撃処理
                targetUnit.TakeDamage(
                    _selectedUnit.Status.Attack
                );

                // 敵ターン開始
                _battleManager.StartEnemyTurn();

                break;

            //攻撃中のステート
            case BattleState.Attacking:

                break;

            //AIステート
            case BattleState.EnemyTurn:
                break;
        }
    }

    /// <summary>
    /// 指定したユニットの攻撃範囲を表示する関数
    /// </summary>
    /// <param name="unit"></param>
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

    /// <summary>
    /// 選択中のユニットを解除し、攻撃範囲をクリアする関数
    /// </summary>
    public void ClearBattleSelection()
    {
        ClearAttackRange();
        _selectedUnit = null;
    }

    /// <summary>
    /// 攻撃範囲をクリアする関数
    /// </summary>
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

    /// <summary>
    /// 選択したユニットを設定
    /// </summary>
    /// <param name="unit"></param>
    private void SelectUnit(Unit unit)
    {
        ClearSelection();
        _selectedUnit = unit;
        _selectedUnit.CurrentCell.SetMaterial(_selectedMaterial);
    }


    /// <summary>
    /// 選択をクリアする
    /// </summary>
    private void ClearSelection()
    {
        if (_selectedUnit == null)
            return;

        SetDefaultMaterial(_selectedUnit.CurrentCell);
        _selectedUnit = null;
    }

    /// <summary>
    ///materialを元に戻す
    /// </summary>
    /// <param name="cell"></param>
    private void SetDefaultMaterial(GridCell cell)
    {
        bool isWhite = (cell.Position.x + cell.Position.y) % 2 == 0;
        cell.SetMaterial(isWhite ? _whiteMaterial : _grayMaterial);
    }
}
