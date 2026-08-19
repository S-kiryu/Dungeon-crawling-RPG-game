using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>
    /// 指定されたセルまで移動する
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onComplete"></param>
    public void MoveAlongPath(
    IReadOnlyList<GridCell> path,
    System.Action onComplete)
    {
        if (_isMoving ||
            path == null ||
            path.Count < 2)
        {
            return;
        }

        // 最終目的地を現在セルとして設定
        CurrentCell = path[path.Count - 1];

        StartCoroutine(
            MovePathRoutine(path, onComplete)
        );
    }

    public void TakeDamage(int damage)
    {
        damage -= Status.Defense;

        if (damage < 0) damage = 0;

        Status.CurrentHP -= damage;

        if (Status.CurrentHP <= 0)
        {
            Status.CurrentHP = 0;
            Dead();
        }
    }

    private void Dead() 
    {
        CurrentCell.RemoveUnit();
        CurrentCell = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 指定されたパスに沿って移動するコルーチン
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator MovePathRoutine(
    IReadOnlyList<GridCell> path,
    System.Action onComplete)
    {
        _isMoving = true;

        // path[0]は移動開始地点なので1から開始
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 destinationPosition =
                path[i].transform.position;

            while (Vector3.Distance(
                       transform.position,
                       destinationPosition) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        destinationPosition,
                        Status.Speed * Time.deltaTime
                    );

                yield return null;
            }

            transform.position = destinationPosition;
        }

        _isMoving = false;
        onComplete?.Invoke();
    }
}
