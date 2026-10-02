using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("範囲枠の色")]
    [SerializeField]
    private Color _selectedOutlineColor =
        Color.white;

    [SerializeField]
    private Color _movementOutlineColor =
        new Color(0.2f, 0.6f, 1f, 1f);

    [SerializeField]
    private Color _attackRangeOutlineColor =
        new Color(1f, 0.8f, 0.1f, 1f);

    [SerializeField]
    private Color _attackTargetOutlineColor =
        new Color(1f, 0.15f, 0.1f, 1f);

    [SerializeField]
    private Color _supportTargetOutlineColor =
        new Color(0.2f, 1f, 0.35f, 1f);

    [SerializeField]
    private Color _hoverOutlineColor =
        new Color(0.2f, 1f, 1f, 1f);

    private GridCell[,] _grid;
    public GridCell[,] Grid => _grid;

    private float _cellSize;
    private Unit _selectedUnit;
    private GridCell _hoveredCell;

    public event System.Action<GridCell>
        HoveredCellChanged;

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

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            SetHoveredCell(null);
            return;
        }

        TryGetCellUnderMouse(
            out GridCell hoveredCell);

        SetHoveredCell(hoveredCell);

        if (Mouse.current.leftButton
            .wasPressedThisFrame &&
            hoveredCell != null)
        {
            OnCellClicked(hoveredCell);
        }
    }

    private void OnDisable()
    {
        SetHoveredCell(null);
    }

    private bool TryGetCellUnderMouse(
        out GridCell cell)
    {
        cell = null;

        Camera mainCamera = Camera.main;

        if (mainCamera == null ||
            Mouse.current == null)
        {
            return false;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(
                mousePosition);

        RaycastHit[] hits =
            Physics.RaycastAll(ray);

        float closestDistance =
            float.PositiveInfinity;

        foreach (RaycastHit hit in hits)
        {
            GridCell candidate =
                hit.collider
                    .GetComponentInParent<GridCell>();

            if (candidate == null ||
                hit.distance >= closestDistance)
            {
                continue;
            }

            cell = candidate;
            closestDistance = hit.distance;
        }

        return cell != null;
    }

    private void SetHoveredCell(
        GridCell nextCell)
    {
        if (_hoveredCell == nextCell)
            return;

        if (_hoveredCell != null)
        {
            _hoveredCell.HideHover();
        }

        _hoveredCell = nextCell;

        if (_hoveredCell != null)
        {
            _hoveredCell.ShowHover(
                _hoverOutlineColor);
        }

        HoveredCellChanged?.Invoke(
            _hoveredCell);
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

    /// <summary>
    /// BFSで探索した経路を逆順にたどって、目的地までの経路を作る
    /// </summary>
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

    /// <summary>
    /// 指定した座標のグリットセルを取得する関数
    /// </summary>
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
        if (clickedCell == null)
            return;

        _battleManager.OnCellClicked(clickedCell);
    }

    /// <summary>
    /// 通常攻撃の範囲内か判定する。
    /// </summary>
    public bool IsInActionRange(
        Unit unit,
        GridCell targetCell)
    {
        return IsInRange(
            unit,
            targetCell,
            unit != null
                ? unit.RangeData
                : null);
    }

    /// <summary>
    /// 指定した範囲データにセルが含まれるか判定する。
    /// </summary>
    public bool IsInRange(
        Unit unit,
        GridCell targetCell,
        ActionRangeData rangeData)
    {
        if (unit == null ||
            unit.CurrentCell == null ||
            targetCell == null ||
            rangeData == null ||
            rangeData.Offsets == null)
        {
            return false;
        }

        Vector2Int targetOffset =
            targetCell.Position -
            unit.CurrentCell.Position;

        foreach (Vector2Int offset in
                 rangeData.Offsets)
        {
            if (offset == targetOffset)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 指定セルを通常のマテリアルへ戻す
    /// </summary>
    public void RestoreCellMaterial(GridCell cell)
    {
        if (cell == null)
            return;

        cell.HideOutline();
    }

    /// <summary>
    /// 移動の範囲を表示する関数
    /// </summary>
    /// <param name="unit"></param>
    public void ShowMovementRange(Unit unit)
    {
        if (unit == null ||
            unit.CurrentCell == null ||
            unit.Status == null)
        {
            return;
        }

        // 前に表示していた攻撃範囲などを消す
        ClearAttackRange();

        Queue<(GridCell cell, int distance)> queue = new();
        HashSet<GridCell> visited = new();

        queue.Enqueue((unit.CurrentCell, 0));
        visited.Add(unit.CurrentCell);

        while (queue.Count > 0)
        {
            (GridCell currentCell, int distance) =
                queue.Dequeue();

            // 現在地は選択中のマテリアルにする
            if (distance == 0)
            {
                currentCell.ShowOutline(
                    _selectedOutlineColor);
            }
            else
            {
                currentCell.ShowOutline(
                    _movementOutlineColor);
            }

            if (distance >= unit.Status.MoveLength)
                continue;

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

                if (visited.Contains(nextCell))
                    continue;

                if (nextCell.Terrain == TerrainType.Wall)
                    continue;

                // 他のユニットがいる場所には移動できない
                if (nextCell.IsOccupied)
                    continue;

                visited.Add(nextCell);
                queue.Enqueue(
                    (nextCell, distance + 1)
                );
            }
        }
    }

    /// <summary>
    /// 指定したユニットの攻撃範囲を表示する関数
    /// </summary>
    /// <param name="unit"></param>
    public void ShowAttackRange(Unit unit)
    {
        if (unit == null ||
            unit.CurrentCell == null ||
            unit.RangeData == null ||
            unit.RangeData.Offsets == null)
        {
            return;
        }

        ClearAttackRange();

        unit.CurrentCell.ShowOutline(
            _selectedOutlineColor);

        foreach (Vector2Int offset in
                 unit.RangeData.Offsets)
        {
            Vector2Int position =
                unit.CurrentCell.Position + offset;

            if (!TryGetCell(
                    position,
                    out GridCell cell))
            {
                continue;
            }

            cell.ShowOutline(
                _attackRangeOutlineColor);

            if (cell.IsOccupied &&
                cell.CurrentUnit != null &&
                !cell.CurrentUnit.IsDead &&
                cell.CurrentUnit.Team != unit.Team &&
                cell.CurrentUnit.Team !=
                    TeamType.Neutral)
            {
                cell.ShowOutline(
                    _attackTargetOutlineColor);
            }
        }
    }

    /// <summary>
    /// スキルの選択範囲を表示する。
    /// </summary>
    public void ShowSkillRange(
        Unit caster,
        SkillData skill)
    {
        if (caster == null ||
            caster.CurrentCell == null ||
            skill == null ||
            skill.ActionRangeData == null ||
            skill.ActionRangeData.Offsets == null)
        {
            return;
        }

        ClearAttackRange();

        caster.CurrentCell.ShowOutline(
            _selectedOutlineColor);

        foreach (Vector2Int offset in
                 skill.ActionRangeData.Offsets)
        {
            Vector2Int position =
                caster.CurrentCell.Position + offset;

            if (!TryGetCell(
                    position,
                    out GridCell cell))
            {
                continue;
            }

            cell.ShowOutline(
                _attackRangeOutlineColor);

            if (!IsValidSkillTarget(
                    caster,
                    cell,
                    skill.TargetType))
            {
                continue;
            }

            Color targetColor =
                IsSupportSkillTarget(
                    skill.TargetType)
                    ? _supportTargetOutlineColor
                    : _attackTargetOutlineColor;

            cell.ShowOutline(targetColor);
        }
    }

    private bool IsSupportSkillTarget(
        SkillTargetType targetType)
    {
        return targetType ==
                   SkillTargetType.Ally ||
               targetType ==
                   SkillTargetType.Self;
    }

    /// <summary>
    /// スキルの対象条件を満たすか判定する。
    /// </summary>
    public bool IsValidSkillTarget(
        Unit caster,
        GridCell targetCell,
        SkillTargetType targetType)
    {
        if (caster == null || targetCell == null)
            return false;

        Unit target = targetCell.CurrentUnit;

        switch (targetType)
        {
            case SkillTargetType.Enemy:
                return target != null &&
                       !target.IsDead &&
                       target.Team != caster.Team &&
                       target.Team != TeamType.Neutral;

            case SkillTargetType.Ally:
                return target != null &&
                       !target.IsDead &&
                       target != caster &&
                       target.Team == caster.Team;

            case SkillTargetType.Self:
                return target == caster &&
                       !caster.IsDead;

            case SkillTargetType.AnyUnit:
                return target != null &&
                       !target.IsDead;

            case SkillTargetType.EmptyCell:
                return target == null;

            default:
                return false;
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
        if (_grid == null)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = _grid[x, y];

                if (cell != null)
                {
                    cell.HideOutline();
                }
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

        if (_selectedUnit != null &&
            _selectedUnit.CurrentCell != null)
        {
            _selectedUnit.CurrentCell.ShowOutline(
                _selectedOutlineColor);
        }
    }

    public void PreparePlayerAction(Unit unit)
    {
        ClearBattleSelection();

        if (unit == null ||
            unit.IsDead ||
            unit.Team != TeamType.Player)
        {
            return;
        }

        _selectedUnit = unit;

        if (_selectedUnit.CurrentCell != null)
        {
            _selectedUnit.CurrentCell.ShowOutline(
                _selectedOutlineColor);
        }
    }


    /// <summary>
    /// 選択をクリアする
    /// </summary>
    private void ClearSelection()
    {
        if (_selectedUnit == null)
            return;

        if (_selectedUnit.CurrentCell != null)
        {
            _selectedUnit.CurrentCell.HideOutline();
        }

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
