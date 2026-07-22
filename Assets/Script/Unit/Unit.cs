using UnityEngine;

public class Unit : MonoBehaviour
{
    public CurrentStatus Status { get; private set; }
    public GridCell CurrentCell { get; private set; }

    public bool Initialize(CharacterData characterData, GridCell gridCell)
    {
        if (characterData == null || characterData.Status == null)
            return false;

        if (gridCell == null || !gridCell.TrySetUnit(this))
            return false;

        Status = new CurrentStatus(characterData.Status);
        CurrentCell = gridCell;
        transform.position = gridCell.transform.position;

        return true;
    }
}
