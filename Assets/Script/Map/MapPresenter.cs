using System;
using System.Collections.Generic;

/// <summary>
/// 探索Model、イベント実行、マップViewを接続するPresenter。
/// </summary>
public sealed class MapPresenter
{
    private static readonly IReadOnlyList<MapNode> EmptyNodes =
        Array.Empty<MapNode>();

    public IReadOnlyList<List<MapNode>> Columns => _model.Columns;
    public MapNode CurrentNode => _model.CurrentNode;
    public IReadOnlyList<MapNode> SelectableNodes =>
        _model.CurrentNode?.NextNodes ?? EmptyNodes;

    public void Initialize(
        int mapLength,
        int minimumWidth,
        int maximumWidth)
    {
        if (!_model.HasActiveRun)
        {
            _model.StartNewRun(
                mapLength,
                minimumWidth,
                maximumWidth);
        }

        RefreshView();
    }

    public void SelectNode(MapNode selectedNode)
    {
        if (!_model.BeginNode(selectedNode))
            return;

        bool waitsForBattleResult =
            selectedNode.EventType == MapEventType.Battle ||
            selectedNode.EventType == MapEventType.Boss;

        _executeEvent(selectedNode);

        if (!waitsForBattleResult)
        {
            _model.CompletePendingNode(out _);
            RefreshView();
        }
    }

    public bool CanSelectNode(MapNode node)
    {
        return _model.CanSelect(node);
    }

    public MapPresenter(
        DungeonRunModel model,
        IMapView view,
        Action<MapNode> executeEvent)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _executeEvent = executeEvent ??
            throw new ArgumentNullException(nameof(executeEvent));
    }

    private readonly DungeonRunModel _model;
    private readonly IMapView _view;
    private readonly Action<MapNode> _executeEvent;

    private void RefreshView()
    {
        _view.ShowMap(
            _model.Columns,
            _model.CurrentNode,
            CanSelectNode,
            SelectNode);
    }
}
