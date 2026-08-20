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

    [SerializeField]
    private Color _damageColor = Color.red;

    [SerializeField]
    private float _damageFlashSeconds = 0.2f;

    [SerializeField]
    private float _moveAnimationSpeed = 5f;

    private Renderer[] _renderers;
    private Coroutine _damageFlashCoroutine;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
    }

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

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        damage -= Status.Defense;

        if (damage < 0)
            damage = 0;

        Status.CurrentHP -= damage;

        bool died = Status.CurrentHP <= 0;

        if (died)
            Status.CurrentHP = 0;

        if (_damageFlashCoroutine != null)
        {
            StopCoroutine(_damageFlashCoroutine);
        }

        _damageFlashCoroutine = StartCoroutine(
            DamageFlashRoutine(died)
        );
    }

    private void Dead() 
    {
        CurrentCell.RemoveUnit();
        CurrentCell = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ダメージを受けた際に赤く点滅させるコルーチン
    /// </summary>
    /// <param name="dieAfterFlash"></param>
    /// <returns></returns>
    private IEnumerator DamageFlashRoutine(
    bool dieAfterFlash)
    {
        MaterialPropertyBlock[] originalBlocks =
            new MaterialPropertyBlock[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer targetRenderer = _renderers[i];

            MaterialPropertyBlock originalBlock =
                new MaterialPropertyBlock();

            targetRenderer.GetPropertyBlock(originalBlock);
            originalBlocks[i] = originalBlock;

            MaterialPropertyBlock damageBlock =
                new MaterialPropertyBlock();

            targetRenderer.GetPropertyBlock(damageBlock);

            // URP Litなど
            damageBlock.SetColor(
                "_BaseColor",
                _damageColor
            );

            // Standard Shaderなど
            damageBlock.SetColor(
                "_Color",
                _damageColor
            );

            targetRenderer.SetPropertyBlock(damageBlock);
        }

        yield return new WaitForSeconds(
            _damageFlashSeconds
        );

        // 元の見た目へ戻す
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].SetPropertyBlock(
                originalBlocks[i]
            );
        }

        _damageFlashCoroutine = null;

        // 倒された場合も赤い表示を見せてから消す
        if (dieAfterFlash)
        {
            Dead();
        }
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
                        _moveAnimationSpeed * Time.deltaTime
                    );

                yield return null;
            }

            transform.position = destinationPosition;
        }

        _isMoving = false;
        onComplete?.Invoke();
    }
}
