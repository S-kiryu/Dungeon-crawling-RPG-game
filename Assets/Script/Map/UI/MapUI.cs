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
                    selectNode);

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

    private readonly Dictionary<MapNode, RectTransform> _nodeRects = new();
    private readonly List<MapNodeUI> _spawnedNodes = new();
    private readonly List<MapLineUI> _spawnedLines = new();

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
}
