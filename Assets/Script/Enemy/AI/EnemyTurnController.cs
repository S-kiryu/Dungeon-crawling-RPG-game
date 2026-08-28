using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    [SerializeField]
    private UnitManager _unitManager;

    [SerializeField]
    private GridManager _gridManager;

    [SerializeField]
    private float _attackWaitSeconds = 0.5f;

    public IEnumerator ExecuteAction(Unit enemy)
    {
        if (enemy == null ||
            enemy.IsDead ||
            enemy.Team != TeamType.Enemy)
        {
            yield break;
        }

        yield return ExecuteEnemyAction(enemy);
    }

    private IEnumerator ExecuteEnemyAction(Unit enemy)
    {
        // 現在地から攻撃できるなら先に攻撃
        Unit attackTarget = FindAttackablePlayer(enemy);

        if (attackTarget != null)
        {
            Attack(enemy, attackTarget);

            yield return new WaitForSeconds(
                _attackWaitSeconds
            );

            yield break;
        }

        // プレイヤーを攻撃できる位置までの経路を探す
        if (TryFindBestAttackPath(
                enemy,
                out List<GridCell> path))
        {
            // 今回のターンに移動できる場所を取得
            int destinationIndex = Mathf.Min(
                enemy.Status.MoveLength,
                path.Count - 1
            );

            if (destinationIndex > 0)
            {
                GridCell destination =
                    path[destinationIndex];

                bool moveCompleted = false;

                bool startedMoving =
                    _gridManager.TryMoveUnit(
                        enemy,
                        destination.Position,
                        () =>
                        {
                            moveCompleted = true;
                        }
                    );

                if (startedMoving)
                {
                    yield return new WaitUntil(
                        () => moveCompleted
                    );
                }
            }
        }

        // 移動後に再び攻撃可能か確認
        attackTarget = FindAttackablePlayer(enemy);

        if (attackTarget != null)
        {
            Attack(enemy, attackTarget);

            yield return new WaitForSeconds(
                _attackWaitSeconds
            );
        }
    }

    private Unit FindAttackablePlayer(Unit enemy)
    {
        if (enemy == null ||
            enemy.RangeData == null ||
            enemy.RangeData.Offsets == null)
        {
            return null;
        }

        List<Unit> players =
            _unitManager.GetLivingUnits(TeamType.Player);

        Unit bestTarget = null;

        foreach (Unit player in players)
        {
            if (player == null ||
                player.IsDead ||
                player.CurrentCell == null)
            {
                continue;
            }

            if (!IsInAttackRange(enemy, player))
                continue;

            // 攻撃可能な中でHPが低い相手を優先
            if (bestTarget == null ||
                player.Status.CurrentHP <
                bestTarget.Status.CurrentHP)
            {
                bestTarget = player;
            }
        }

        return bestTarget;
    }

    private bool IsInAttackRange(
        Unit attacker,
        Unit target)
    {
        if (attacker.CurrentCell == null ||
            target.CurrentCell == null ||
            attacker.RangeData == null ||
            attacker.RangeData.Offsets == null)
        {
            return false;
        }

        Vector2Int targetOffset =
            target.CurrentCell.Position -
            attacker.CurrentCell.Position;

        foreach (Vector2Int attackOffset
                 in attacker.RangeData.Offsets)
        {
            if (attackOffset == targetOffset)
                return true;
        }

        return false;
    }

    private bool TryFindBestAttackPath(
        Unit enemy,
        out List<GridCell> bestPath)
    {
        bestPath = null;

        if (enemy.RangeData == null ||
            enemy.RangeData.Offsets == null)
        {
            return false;
        }

        List<Unit> players =
            _unitManager.GetLivingUnits(TeamType.Player);

        foreach (Unit player in players)
        {
            if (player == null ||
                player.IsDead ||
                player.CurrentCell == null)
            {
                continue;
            }

            foreach (Vector2Int attackOffset
                     in enemy.RangeData.Offsets)
            {
                /*
                 * targetPosition - enemyPosition
                 *     = attackOffset
                 *
                 * enemyPosition
                 *     = targetPosition - attackOffset
                 */
                Vector2Int attackPosition =
                    player.CurrentCell.Position -
                    attackOffset;

                if (!_gridManager.TryGetCell(
                        attackPosition,
                        out GridCell attackCell))
                {
                    continue;
                }

                // 自分以外のユニットがいるマスは使えない
                if (attackCell.IsOccupied &&
                    attackCell.CurrentUnit != enemy)
                {
                    continue;
                }

                if (!_gridManager.TryFindPath(
                        enemy.CurrentCell,
                        attackCell,
                        enemy,
                        out List<GridCell> path))
                {
                    continue;
                }

                // 最短経路を選ぶ
                if (bestPath == null ||
                    path.Count < bestPath.Count)
                {
                    bestPath = path;
                }
            }
        }

        return bestPath != null;
    }

    private void Attack(
        Unit attacker,
        Unit target)
    {
        if (attacker == null ||
            target == null ||
            attacker.IsDead ||
            target.IsDead)
        {
            return;
        }

        Debug.Log(
            $"{attacker.name} が " +
            $"{target.name} を攻撃"
        );

        target.TakeDamage(
            attacker.Status.Attack
        );
    }
}