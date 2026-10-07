using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DungeonRunModelをシーン間で保持するComposition Root。
/// </summary>
[DefaultExecutionOrder(-800)]
public class DungeonRunSession : MonoBehaviour
{
    public static DungeonRunSession Instance { get; private set; }

    public DungeonRunModel Model => _model;
    public IReadOnlyList<List<MapNode>> Columns => _model?.Columns;
    public MapNode CurrentNode => _model?.CurrentNode;
    public MapNode PendingNode => _model?.PendingNode;
    public bool HasActiveRun => _model != null && _model.HasActiveRun;

    public void StartNewRun(
        int mapLength,
        int minimumWidth,
        int maximumWidth)
    {
        _model.StartNewRun(mapLength, minimumWidth, maximumWidth);
    }

    public bool CanSelect(MapNode node)
    {
        return _model != null && _model.CanSelect(node);
    }

    public bool BeginNode(MapNode node)
    {
        return _model != null && _model.BeginNode(node);
    }

    public bool CompletePendingNode(out MapEventType completedType)
    {
        if (_model != null)
            return _model.CompletePendingNode(out completedType);

        completedType = MapEventType.Start;
        return false;
    }

    public void CancelPendingNode()
    {
        _model?.CancelPendingNode();
    }

    public void EndRun()
    {
        _model?.EndRun();
    }

    private DungeonRunModel _model;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _model = new DungeonRunModel(() => new MapGenerator());
        DontDestroyOnLoad(gameObject);
    }
}
