using UnityEngine;
using System.Collections;
public class Unit : MonoBehaviour
{
    public CurrentStatus Status { get; private set; }
    public GridCell CurrentCell { get; private set; }
    public TeamType Team { get; private set; }
    public ActionRangeData RangeData { get; private set; }
    private bool _isMoving = false;
    public bool IsMoving => _isMoving;
    public bool IsDead => Status.CurrentHP <= 0;

    public bool Initialize(CharacterData characterData, GridCell gridCell, TeamType team, ActionRangeData actionRange)
    {
        if (characterData == null || characterData.Status == null)
            return false;

        if (gridCell == null || !gridCell.TrySetUnit(this))
            return false;

        Status = new CurrentStatus(characterData.Status);
        CurrentCell = gridCell;
        transform.position = gridCell.transform.position;
        Team = team;
        RangeData = actionRange;

        return true;
    }

    public void MoveTo(GridCell destination, System.Action onComplete)
    {
        if (_isMoving)
            return;

        CurrentCell = destination;
        StartCoroutine(MoveRoutine(destination.transform.position, onComplete));
    }

    private IEnumerator MoveRoutine(Vector3 destinationPosition, System.Action onComplete)
    {
        _isMoving = true;

        while (Vector3.Distance(transform.position, destinationPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destinationPosition,
                Status.Speed * Time.deltaTime);

            yield return null;
        }

        transform.position = destinationPosition;
        _isMoving = false;

        onComplete?.Invoke();
    }
}
