using System;
using System.Collections.Generic;

public interface IMapView
{
    void ShowMap(
        IReadOnlyList<List<MapNode>> columns,
        MapNode currentNode,
        Func<MapNode, bool> canSelect,
        Action<MapNode> selectNode);
}
