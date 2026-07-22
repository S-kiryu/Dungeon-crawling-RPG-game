using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private MapUI _mapUI;
    [SerializeField] private MapEventManager _mapEventManager;
    [SerializeField, Tooltip("ボスまで何歩で行けるのか")] private int _mapLength;
    [SerializeField, Tooltip("最小の横幅")] private int _mapMinimumWidth;
    [SerializeField, Tooltip("最大の横幅")] private int _mapMaximumWidth;

    //生成したノードを管理するリスト
    private List<List<MapNode>> _columns; 
    private MapData _mapData;
    private MapGenerator _mapGenerator;
    private MapNode _currentNode;

    public IReadOnlyList<List<MapNode>> Columns => _columns;
    public IReadOnlyList<MapNode> SelectableNodes => _currentNode.NextNodes;
    public MapNode CurrentNode => _currentNode;

    private void Awake()
    {
        _mapData = new MapData();
        _mapGenerator = new MapGenerator();
    }


    private void Start()
    {
        _columns = _mapGenerator.GenerateMap(
            _mapLength,
            _mapMinimumWidth,
            _mapMaximumWidth);

        _mapData.SetNextNode(_columns);
        _currentNode = _columns[0][0];

        _mapUI.ShowMap(Columns, this);
    }


    //各ステートのイベント
    public void SelectNode(MapNode selectedNode)
    {
        if (!_currentNode.NextNodes.Contains(selectedNode))
            return;

        _currentNode = selectedNode;

        _mapEventManager.Execute(_currentNode);

    }
    private void DebugMap()
    {
        for (int columnIndex = 0; columnIndex < _columns.Count; columnIndex++)
        {
            List<MapNode> column = _columns[columnIndex];

            Debug.Log($"--- {columnIndex}列目：{column.Count}ノード ---");

            for (int nodeIndex = 0; nodeIndex < column.Count; nodeIndex++)
            {
                MapNode node = column[nodeIndex];

                Debug.Log(
                    $"ノード {nodeIndex} | " +
                    $"イベント: {node.EventType} | " +
                    $"接続先数: {node.NextNodes.Count}");
            }
        }
    }
}
