using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private MapUI _mapUI;

    [SerializeField]
    private MapEventManager _mapEventManager;

    [SerializeField]
    private int _mapLength = 5;

    [SerializeField]
    private int _mapMinimumWidth = 1;

    [SerializeField]
    private int _mapMaximumWidth = 3;

    private DungeonRunSession _runSession;

    public IReadOnlyList<List<MapNode>> Columns =>
        _runSession.Columns;

    public MapNode CurrentNode =>
        _runSession.CurrentNode;

    public IReadOnlyList<MapNode> SelectableNodes =>
        _runSession.CurrentNode.NextNodes;

    private void Start()
    {
        _runSession =
            DungeonRunSession.Instance;

        if (_runSession == null)
        {
            Debug.LogError(
                "DungeonRunSessionが存在しません。");
            return;
        }

        // 初回入場時だけマップを生成する
        if (!_runSession.HasActiveRun)
        {
            _runSession.StartNewRun(
                _mapLength,
                _mapMinimumWidth,
                _mapMaximumWidth);
        }

        _mapUI.ShowMap(
            Columns,
            this);
    }

    public void SelectNode(
        MapNode selectedNode)
    {
        if (!_runSession.BeginNode(
                selectedNode))
        {
            return;
        }

        bool waitsForBattleResult =
            selectedNode.EventType ==
                MapEventType.Battle ||
            selectedNode.EventType ==
                MapEventType.Boss;

        _mapEventManager.Execute(
            selectedNode);

        // ショップと休憩は仮で即クリア扱い
        if (!waitsForBattleResult)
        {
            _runSession.CompletePendingNode(
                out _);
        }
    }
}