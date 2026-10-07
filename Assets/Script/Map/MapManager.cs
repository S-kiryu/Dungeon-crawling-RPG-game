using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MapPresenterを構築するComposition Root兼、既存シーン向けFacade。
/// </summary>
public class MapManager : MonoBehaviour
{
    public IReadOnlyList<List<MapNode>> Columns => _presenter?.Columns;
    public MapNode CurrentNode => _presenter?.CurrentNode;
    public IReadOnlyList<MapNode> SelectableNodes =>
        _presenter?.SelectableNodes;

    public void SelectNode(MapNode selectedNode)
    {
        _presenter?.SelectNode(selectedNode);
    }

    public bool CanSelectNode(MapNode node)
    {
        return _presenter != null && _presenter.CanSelectNode(node);
    }

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

    private MapPresenter _presenter;

    private void Start()
    {
        DungeonRunSession session = DungeonRunSession.Instance;

        if (session?.Model == null)
        {
            Debug.LogError("DungeonRunSessionが存在しません。", this);
            return;
        }

        if (_mapUI == null || _mapEventManager == null)
        {
            Debug.LogError("MapのViewまたはEventManagerが未設定です。", this);
            return;
        }

        _presenter = new MapPresenter(
            session.Model,
            _mapUI,
            _mapEventManager.Execute);

        _presenter.Initialize(
            _mapLength,
            _mapMinimumWidth,
            _mapMaximumWidth);
    }
}
