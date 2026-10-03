using UnityEngine;

/// <summary>
/// 敵ユニットへカーソルを合わせたとき、
/// 現在地から移動できる範囲を表示する。
/// </summary>
public class EnemyThreatPreviewController :
    MonoBehaviour
{
    [SerializeField]
    private GridManager _gridManager;

    private Unit _previewedEnemy;

    private void OnEnable()
    {
        if (_gridManager != null)
        {
            _gridManager.HoveredCellChanged +=
                HandleHoveredCellChanged;
        }
    }

    private void OnDisable()
    {
        if (_gridManager != null)
        {
            _gridManager.HoveredCellChanged -=
                HandleHoveredCellChanged;

            _gridManager.ClearEnemyThreatRange();
        }
    }

    private void HandleHoveredCellChanged(
        GridCell hoveredCell)
    {
        Unit hoveredUnit =
            hoveredCell != null
                ? hoveredCell.CurrentUnit
                : null;

        if (hoveredUnit != null &&
            !hoveredUnit.IsDead &&
            hoveredUnit.Team ==
                TeamType.Enemy)
        {
            if (_previewedEnemy == hoveredUnit)
                return;

            _previewedEnemy = hoveredUnit;

            _gridManager.ShowEnemyMovementRange(
                hoveredUnit);

            return;
        }

        _previewedEnemy = null;
        _gridManager.ClearEnemyThreatRange();
    }
}
