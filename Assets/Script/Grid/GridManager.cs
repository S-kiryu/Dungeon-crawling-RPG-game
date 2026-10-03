using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// グリッドのView兼Composition Root。
/// 経路・範囲計算は専用Modelサービスへ委譲する。
/// </summary>
public class GridManager : MonoBehaviour
{
    public GridCell[,] Grid => _grid;

    public event Action<GridCell> CellClicked;
    public event Action<GridCell> HoveredCellChanged;



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
        return _pathfinder.TryFindPath(
            startCell,
            destinationCell,
            movingUnit,
            out path);
    }

    /// <summary>
    /// 指定した座標のグリットセルを取得する関数
    /// </summary>
    public bool TryGetCell(Vector2Int position, out GridCell cell)
    {
        cell = null;

        bool isOutOfRange =
            position.x < 0 || position.x >= _width ||
            position.y < 0 || position.y >= _height;

        if (isOutOfRange)
            return false;

        cell = _grid[position.x, position.y];
        return true;
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
            targetCell == null)
        {
            return false;
        }

        return _rangeCalculator.IsInRange(
            unit.CurrentCell.Position,
            targetCell.Position,
            rangeData);
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
        // 前に表示していた攻撃範囲などを消す
        ClearAttackRange();

        HashSet<GridCell> reachableCells =
            GetReachableCells(unit);

        foreach (GridCell cell in
                 reachableCells)
        {
            if (cell == unit.CurrentCell)
            {
                cell.ShowOutline(
                    _selectedOutlineColor);
            }
            else
            {
                cell.ShowOutline(
                    _movementOutlineColor);
            }
        }
    }

    /// <summary>
    /// ユニットが現在の移動力で到達できるセルを取得する。
    /// 現在地も結果へ含む。
    /// </summary>
    public HashSet<GridCell> GetReachableCells(
        Unit unit)
    {
        if (unit == null ||
            unit.CurrentCell == null ||
            unit.Status == null)
        {
            return new HashSet<GridCell>();
        }

        return _pathfinder.GetReachableCells(
            unit.CurrentCell,
            unit.Status.MoveLength);
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
                _rangeCalculator.IsSupportTarget(
                    skill.TargetType)
                    ? _supportTargetOutlineColor
                    : _attackTargetOutlineColor;

            cell.ShowOutline(targetColor);
        }
    }

    /// <summary>
    /// スキルの対象条件を満たすか判定する。
    /// </summary>
    public bool IsValidSkillTarget(
        Unit caster,
        GridCell targetCell,
        SkillTargetType targetType)
    {
        return targetCell != null &&
               _rangeCalculator.IsValidTarget(
                   caster,
                   targetCell.CurrentUnit,
                   targetType);
    }

    /// <summary>
    /// 敵が現在地から通常攻撃できる範囲を表示する。
    /// 移動後の攻撃範囲は含めない。
    /// </summary>
    public void ShowEnemyThreatRange(
        Unit enemy)
    {
        ClearEnemyThreatRange();

        if (enemy == null ||
            enemy.IsDead ||
            enemy.Team != TeamType.Enemy ||
            enemy.CurrentCell == null ||
            enemy.RangeData == null ||
            enemy.RangeData.Offsets == null)
        {
            return;
        }

        foreach (Vector2Int offset in
                 enemy.RangeData.Offsets)
        {
            Vector2Int targetPosition =
                enemy.CurrentCell.Position +
                offset;

            if (!TryGetCell(
                    targetPosition,
                    out GridCell targetCell))
            {
                continue;
            }

            if (targetCell.Terrain ==
                TerrainType.Wall)
            {
                continue;
            }

            targetCell.ShowThreatOutline(
                _enemyThreatOutlineColor);
        }
    }

    /// <summary>
    /// 敵が現在地から移動できる範囲だけを表示する。
    /// 攻撃範囲および移動後攻撃範囲は含めない。
    /// </summary>
    public void ShowEnemyMovementRange(
        Unit enemy)
    {
        ClearEnemyThreatRange();

        if (enemy == null ||
            enemy.IsDead ||
            enemy.Team != TeamType.Enemy)
        {
            return;
        }

        HashSet<GridCell> reachableCells =
            GetReachableCells(enemy);

        foreach (GridCell cell in
                 reachableCells)
        {
            if (cell == enemy.CurrentCell)
                continue;

            cell.ShowThreatOutline(
                _enemyMovementOutlineColor);
        }
    }

    /// <summary>
    /// 敵の危険範囲表示だけを解除する。
    /// プレイヤーの行動範囲表示には影響しない。
    /// </summary>
    public void ClearEnemyThreatRange()
    {
        if (_grid == null)
            return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridCell cell = _grid[x, y];

                if (cell != null)
                    cell.HideThreatOutline();
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

    [SerializeField] private GridCell _gridPrefab;
    [FormerlySerializedAs("width")]
    [SerializeField]
    private int _width;

    [FormerlySerializedAs("height")]
    [SerializeField]
    private int _height;
    [SerializeField] private Material _whiteMaterial;
    [SerializeField] private Material _grayMaterial;

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

    [Header("敵危険範囲")]
    [SerializeField]
    private Color _enemyThreatOutlineColor =
        new Color(1f, 0.15f, 0.1f, 0.65f);

    [SerializeField]
    private Color _enemyMovementOutlineColor =
        new Color(0.65f, 0.25f, 1f, 0.75f);

    private GridCell[,] _grid;
    private float _cellSize;
    private Unit _selectedUnit;
    private GridCell _hoveredCell;
    private GridPathfinder _pathfinder;
    private GridRangeCalculator _rangeCalculator;

    private void Awake()
    {
        _pathfinder = new GridPathfinder(FindCell);
        _rangeCalculator = new GridRangeCalculator();
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
            CellClicked?.Invoke(hoveredCell);
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
        _grid = new GridCell[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
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
    /// 攻撃範囲をクリアする関数
    /// </summary>
    private void ClearAttackRange()
    {
        if (_grid == null)
            return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
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

    private GridCell FindCell(Vector2Int position)
    {
        return TryGetCell(position, out GridCell cell)
            ? cell
            : null;
    }
}
