using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダンジョンランのセッションを管理するクラス
/// </summary>
[DefaultExecutionOrder(-800)]
public class DungeonRunSession : MonoBehaviour
{
    public static DungeonRunSession Instance
    {
        get;
        private set;
    }

    private List<List<MapNode>> _columns;
    private MapNode _currentNode;
    private MapNode _pendingNode;

    public IReadOnlyList<List<MapNode>> Columns =>
        _columns;

    public MapNode CurrentNode =>
        _currentNode;

    public MapNode PendingNode =>
        _pendingNode;

    public bool HasActiveRun =>
        _columns != null &&
        _columns.Count > 0 &&
        _currentNode != null;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 新しいダンジョンランを開始する
    /// </summary>
    /// <param name="mapLength"></param>
    /// <param name="minimumWidth"></param>
    /// <param name="maximumWidth"></param>
    public void StartNewRun(
        int mapLength,
        int minimumWidth,
        int maximumWidth)
    {
        MapGenerator generator =
            new MapGenerator();

        MapData mapData =
            new MapData();

        _columns = generator.GenerateMap(
            mapLength,
            minimumWidth,
            maximumWidth);

        mapData.SetNextNode(_columns);

        _currentNode = _columns[0][0];
        _pendingNode = null;
    }

    /// <summary>
    /// 指定されたノードを選択可能かどうかを判定する
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool CanSelect(
        MapNode node)
    {
        return
            node != null &&
            _currentNode != null &&
            _pendingNode == null &&
            _currentNode.NextNodes.Contains(node);
    }


    /// <summary>
    /// 指定されたノードを選択する
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool BeginNode(
        MapNode node)
    {
        if (!CanSelect(node))
            return false;

        _pendingNode = node;
        return true;
    }


    /// <summary>
    /// 保留中のノードを確定する
    /// </summary>
    /// <param name="completedType"></param>
    /// <returns></returns>
    public bool CompletePendingNode(
        out MapEventType completedType)
    {
        completedType = MapEventType.Start;

        if (_pendingNode == null)
            return false;

        completedType =
            _pendingNode.EventType;

        _currentNode =
            _pendingNode;

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
}