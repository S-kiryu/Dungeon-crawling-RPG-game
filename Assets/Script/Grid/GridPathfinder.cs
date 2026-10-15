using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// グリッドの経路と到達可能範囲を計算するModelサービス。
/// 表示や入力には関与しない。
/// </summary>
public sealed class GridPathfinder
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

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
        Dictionary<GridCell, GridCell> previousCells = new();

        searchQueue.Enqueue(startCell);
        previousCells[startCell] = null;

        while (searchQueue.Count > 0)
        {
            GridCell currentCell = searchQueue.Dequeue();

            if (currentCell == destinationCell)
            {
                path = BuildPath(previousCells, destinationCell);
                return true;
            }

            foreach (Vector2Int direction in Directions)
            {
                GridCell nextCell = _findCell(
                    currentCell.Position + direction);

                if (nextCell == null ||
                    previousCells.ContainsKey(nextCell) ||
                    nextCell.Terrain == TerrainType.Wall ||
                    nextCell.IsOccupied &&
                    nextCell.CurrentUnit != movingUnit)
                {
                    continue;
                }

                previousCells[nextCell] = currentCell;
                searchQueue.Enqueue(nextCell);
            }
        }

        return false;
    }

    public HashSet<GridCell> GetReachableCells(
        GridCell startCell,
        int movementRange)
    {
        HashSet<GridCell> reachableCells = new();

        if (startCell == null || movementRange < 0)
            return reachableCells;

        Queue<(GridCell cell, int distance)> queue = new();
        queue.Enqueue((startCell, 0));
        reachableCells.Add(startCell);

        while (queue.Count > 0)
        {
            (GridCell currentCell, int distance) = queue.Dequeue();

            if (distance >= movementRange)
                continue;

            foreach (Vector2Int direction in Directions)
            {
                GridCell nextCell = _findCell(
                    currentCell.Position + direction);

                if (nextCell == null ||
                    reachableCells.Contains(nextCell) ||
                    nextCell.Terrain == TerrainType.Wall ||
                    nextCell.IsOccupied)
                {
                    continue;
                }

                reachableCells.Add(nextCell);
                queue.Enqueue((nextCell, distance + 1));
            }
        }

        return reachableCells;
    }

    public GridPathfinder(Func<Vector2Int, GridCell> findCell)
    {
        _findCell = findCell ??
            throw new ArgumentNullException(nameof(findCell));
    }

    private readonly Func<Vector2Int, GridCell> _findCell;

    private static List<GridCell> BuildPath(
        IReadOnlyDictionary<GridCell, GridCell> previousCells,
        GridCell destinationCell)
    {
        List<GridCell> path = new();
        GridCell currentCell = destinationCell;

        while (currentCell != null)
        {
            path.Add(currentCell);
            currentCell = previousCells[currentCell];
        }

        path.Reverse();
        return path;
    }
}
