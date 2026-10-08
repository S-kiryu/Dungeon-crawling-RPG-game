using System;
using System.Collections.Generic;

/// <summary>
/// 一回の探索状態を保持するModel。
/// </summary>
public sealed class DungeonRunModel
{
    public IReadOnlyList<List<MapNode>> Columns => _columns;
    public MapNode CurrentNode => _currentNode;
    public MapNode PendingNode => _pendingNode;
    public bool HasActiveRun =>
        _columns != null &&
        _columns.Count > 0 &&
        _currentNode != null;


    public void StartNewRun(
        int mapLength,
        int minimumWidth,
        int maximumWidth)
    {
        MapGenerator generator = _createGenerator();
        MapData mapData = new();

        _columns = generator.GenerateMap(
            mapLength,
            minimumWidth,
            maximumWidth);

        mapData.SetNextNode(_columns);
        _currentNode = _columns[0][0];
        _pendingNode = null;
    }

    public bool CanSelect(MapNode node)
    {
        return node != null &&
               _currentNode != null &&
               _pendingNode == null &&
               _currentNode.NextNodes.Contains(node);
    }

    public bool BeginNode(MapNode node)
    {
        if (!CanSelect(node))
            return false;

        _pendingNode = node;
        return true;
    }

    public bool CompletePendingNode(out MapEventType completedType)
    {
        completedType = MapEventType.Start;

        if (_pendingNode == null)
            return false;

        completedType = _pendingNode.EventType;
        _currentNode = _pendingNode;
        _pendingNode = null;
        return true;
    }

    public void CancelPendingNode()
    {
        _pendingNode = null;
    }

    public void EndRun()
    {
        _columns = null;
        _currentNode = null;
        _pendingNode = null;
    }

    public DungeonRunModel(Func<MapGenerator> createGenerator)
    {
        _createGenerator = createGenerator ??
            throw new ArgumentNullException(nameof(createGenerator));
    }

    private readonly Func<MapGenerator> _createGenerator;
    private List<List<MapNode>> _columns;
    private MapNode _currentNode;
    private MapNode _pendingNode;
}
