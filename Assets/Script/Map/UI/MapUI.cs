using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップを描画し、入力をPresenterへ通知するView。
/// </summary>
public class MapUI : MonoBehaviour, IMapView
{
    public void ShowMap(
        IReadOnlyList<List<MapNode>> columns,
        MapNode currentNode,
        Func<MapNode, bool> canSelect,
        Action<MapNode> selectNode)
    {
        ClearMap();
        EnsureInfoPanel();

        for (int columnIndex = 0;
             columnIndex < columns.Count;
             columnIndex++)
        {
            List<MapNode> column = columns[columnIndex];

            for (int nodeIndex = 0;
                 nodeIndex < column.Count;
                 nodeIndex++)
            {
                MapNode node = column[nodeIndex];
                MapNodeUI nodeUI = Instantiate(_nodePrefab, _nodeParent);
                RectTransform nodeRect =
                    nodeUI.GetComponent<RectTransform>();

                float x = columnIndex * _columnSpacing;
                float y =
                    (nodeIndex - (column.Count - 1) / 2f) *
                    _nodeSpacing;

                nodeRect.anchoredPosition = new Vector2(x, y);
                nodeUI.Setup(
                    node,
                    node == currentNode,
                    canSelect(node),
                    selectNode,
                    ShowNodeInfo,
                    HideNodeInfo);

                _nodeRects.Add(node, nodeRect);
                _spawnedNodes.Add(nodeUI);
            }
        }

        DrawLines(columns);
    }

    [SerializeField]
    private MapNodeUI _nodePrefab;

    [SerializeField]
    private RectTransform _nodeParent;

    [SerializeField]
    private float _columnSpacing = 200f;

    [SerializeField]
    private float _nodeSpacing = 100f;

    [SerializeField]
    private MapLineUI _linePrefab;

    [Header("ノード情報")]
    [Tooltip("ホバー時に表示する情報パネルのPrefab。")]
    [SerializeField]
    private MapNodeInfoPanel _infoPanelPrefab;

    [Tooltip("情報パネルの生成先。未設定ならノードと同じCanvas直下に生成します。")]
    [SerializeField]
    private RectTransform _infoPanelParent;

    private readonly Dictionary<MapNode, RectTransform> _nodeRects = new();
    private readonly List<MapNodeUI> _spawnedNodes = new();
    private readonly List<MapLineUI> _spawnedLines = new();
    private MapNodeInfoPanel _infoPanel;
    private MapNode _hoveredNode;

    private void DrawLines(IReadOnlyList<List<MapNode>> columns)
    {
        foreach (List<MapNode> column in columns)
        {
            foreach (MapNode node in column)
            {
                foreach (MapNode nextNode in node.NextNodes)
                {
                    MapLineUI lineUI = Instantiate(_linePrefab, _nodeParent);

                    lineUI.Draw(
                        _nodeRects[node].anchoredPosition,
                        _nodeRects[nextNode].anchoredPosition);

                    lineUI.transform.SetAsFirstSibling();
                    _spawnedLines.Add(lineUI);
                }
            }
        }
    }

    private void ClearMap()
    {
        _hoveredNode = null;
        _infoPanel?.Hide();

        foreach (MapNodeUI node in _spawnedNodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }

        foreach (MapLineUI line in _spawnedLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }

        _spawnedNodes.Clear();
        _spawnedLines.Clear();
        _nodeRects.Clear();
    }

    private void EnsureInfoPanel()
    {
        if (_infoPanel != null)
            return;

        if (_infoPanelPrefab == null)
        {
            Debug.LogWarning(
                "MapUI: MapNodeInfoPanel Prefabが未設定です。");
            return;
        }

        Transform parent = _infoPanelParent;

        if (parent == null && _nodeParent != null)
        {
            Canvas canvas =
                _nodeParent.GetComponentInParent<Canvas>();
            parent = canvas != null
                ? canvas.transform
                : null;
        }

        if (parent == null)
        {
            Debug.LogWarning(
                "MapUI: MapNodeInfoPanelの生成先が見つかりません。");
            return;
        }

        _infoPanel = Instantiate(
            _infoPanelPrefab,
            parent,
            false);
        _infoPanel.Hide();
    }

    private void ShowNodeInfo(
        MapNode node,
        Sprite previewImage)
    {
        _hoveredNode = node;
        _infoPanel?.Show(node, previewImage);
    }

    private void HideNodeInfo(MapNode node)
    {
        if (_hoveredNode != node)
            return;

        _hoveredNode = null;
        _infoPanel?.Hide();
    }
}
