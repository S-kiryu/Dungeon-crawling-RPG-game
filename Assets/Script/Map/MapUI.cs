using System.Collections.Generic;
using UnityEngine;

public class MapUI : MonoBehaviour
{
    [SerializeField] private MapNodeUI _nodePrefab;
    [SerializeField] private RectTransform _nodeParent;
    [SerializeField] private float _columnSpacing = 200f;
    [SerializeField] private float _nodeSpacing = 100f;
    [SerializeField] private MapLineUI _linePrefab;

    private readonly Dictionary<MapNode, RectTransform> _nodeRects = new();

    public void ShowMap(
    IReadOnlyList<List<MapNode>> columns,
    MapManager mapManager)
    {
        for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            List<MapNode> column = columns[columnIndex];

            for (int nodeIndex = 0; nodeIndex < column.Count; nodeIndex++)
            {
                MapNodeUI nodeUI =
                    Instantiate(_nodePrefab, _nodeParent);

                float x = columnIndex * _columnSpacing;
                float y = (nodeIndex - (column.Count - 1) / 2f) * _nodeSpacing;

                nodeUI.GetComponent<RectTransform>().anchoredPosition =new Vector2(x, y);
                nodeUI.Setup(column[nodeIndex], mapManager);

                RectTransform nodeRect = nodeUI.GetComponent<RectTransform>();

                nodeRect.anchoredPosition = new Vector2(x, y);

                nodeUI.Setup(column[nodeIndex], mapManager);

                _nodeRects.Add(column[nodeIndex], nodeRect);
            }
        }
        DrawLines(columns);
    }

    /// <summary>
    /// 線でつなぎをつくる
    /// </summary>
    /// <param name="columns"></param>
    private void DrawLines(IReadOnlyList<List<MapNode>> columns)
    {
        foreach (List<MapNode> column in columns)
        {
            foreach (MapNode node in column)
            {
                foreach (MapNode nextNode in node.NextNodes)
                {
                    MapLineUI lineUI =
                        Instantiate(_linePrefab, _nodeParent);

                    lineUI.Draw(
                        _nodeRects[node].anchoredPosition,
                        _nodeRects[nextNode].anchoredPosition);

                    // ノードより後ろに表示する
                    lineUI.transform.SetAsFirstSibling();
                }
            }
        }
    }
}