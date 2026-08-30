using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public CurrentStatus Status { get; private set; }
    public GridCell CurrentCell { get; private set; }
    public TeamType Team { get; private set; }
    public ActionRangeData RangeData { get; private set; }

    /// <summary>
    /// プレイヤーの所持キャラから生成された場合に設定される。
    /// 敵やテスト用ユニットの場合はnull。
    /// </summary>
    public CharacterInstance SourceCharacter
    {
        get;
        private set;
    }

    public bool IsMoving => _isMoving;

    public bool IsDead =>
        Status != null &&
        Status.CurrentHP <= 0;

    [Header("ダメージ演出")]
    [SerializeField]
    private Color _damageColor = Color.red;

    [SerializeField]
    private float _damageFlashSeconds = 0.2f;

    [Header("移動")]
    [SerializeField]
    private float _moveAnimationSpeed = 5f;

    private bool _isMoving;
    private Renderer[] _renderers;
    private Coroutine _damageFlashCoroutine;

    private void Awake()
    {
        _renderers =
            GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// 所持キャラ個体からプレイヤーユニットを初期化する。
    /// </summary>
    public bool Initialize(
        CharacterInstance character,
        GridCell gridCell)
    {
        if (character == null ||
            !character.CanDeploy ||
            character.CharacterData == null ||
            character.Status == null)
        {
            return false;
        }

        if (gridCell == null ||
            !gridCell.TrySetUnit(this))
        {
            return false;
        }

        SourceCharacter = character;

        // 所持キャラのステータスを直接変更しないようにコピーする
        Status = new CurrentStatus(
            character.Status);

        CurrentCell = gridCell;
        Team = TeamType.Player;

        RangeData =
            character.CharacterData.RangeData;

        transform.position =
            gridCell.transform.position;

        return true;
    }

    /// <summary>
    /// CharacterDataから敵やテスト用ユニットを初期化する。
    /// </summary>
    public bool Initialize(
        CharacterData characterData,
        GridCell gridCell,
        TeamType team,
        ActionRangeData actionRange)
    {
        if (characterData == null ||
            characterData.Status == null)
        {
            return false;
        }

        if (gridCell == null ||
            !gridCell.TrySetUnit(this))
        {
            return false;
        }

        SourceCharacter = null;

        Status = new CurrentStatus(
            characterData.Status);

        CurrentCell = gridCell;
        Team = team;
        RangeData = actionRange;

        transform.position =
            gridCell.transform.position;

        return true;
    }

    /// <summary>
    /// 指定された経路に沿って移動する。
    /// </summary>
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

        // 最終目的地を現在セルとして登録する
        CurrentCell = path[path.Count - 1];

        StartCoroutine(
            MovePathRoutine(
                path,
                onComplete));
    }

    /// <summary>
    /// ダメージを受ける。
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (Status == null || IsDead)
            return;

        int actualDamage =
            Mathf.Max(
                0,
                damage - Status.Defense);

        Status.CurrentHP -= actualDamage;

        Status.CurrentHP =
            Mathf.Max(
                0,
                Status.CurrentHP);

        // 所持キャラの場合はHPと死亡を反映する
        ReflectStatusToSourceCharacter();

        bool died =
            Status.CurrentHP <= 0;

        if (_damageFlashCoroutine != null)
        {
            StopCoroutine(
                _damageFlashCoroutine);
        }

        _damageFlashCoroutine =
            StartCoroutine(
                DamageFlashRoutine(died));
    }

    /// <summary>
    /// 戦闘中の状態を所持キャラへ反映する。
    /// </summary>
    private void ReflectStatusToSourceCharacter()
    {
        if (SourceCharacter == null)
            return;

        SourceCharacter.ApplyBattleResult(
            Status);
    }

    /// <summary>
    /// ユニットを死亡状態にする。
    /// </summary>
    private void Dead()
    {
        if (CurrentCell != null)
        {
            CurrentCell.RemoveUnit();
            CurrentCell = null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// ダメージを受けたときの点滅演出。
    /// </summary>
    private IEnumerator DamageFlashRoutine(
        bool dieAfterFlash)
    {
        MaterialPropertyBlock[] originalBlocks =
            new MaterialPropertyBlock[
                _renderers.Length];

        for (int index = 0;
             index < _renderers.Length;
             index++)
        {
            Renderer targetRenderer =
                _renderers[index];

            MaterialPropertyBlock originalBlock =
                new MaterialPropertyBlock();

            targetRenderer.GetPropertyBlock(
                originalBlock);

            originalBlocks[index] =
                originalBlock;

            MaterialPropertyBlock damageBlock =
                new MaterialPropertyBlock();

            targetRenderer.GetPropertyBlock(
                damageBlock);

            // URP Lit用
            damageBlock.SetColor(
                "_BaseColor",
                _damageColor);

            // Standard Shader用
            damageBlock.SetColor(
                "_Color",
                _damageColor);

            targetRenderer.SetPropertyBlock(
                damageBlock);
        }

        yield return new WaitForSeconds(
            _damageFlashSeconds);

        for (int index = 0;
             index < _renderers.Length;
             index++)
        {
            if (_renderers[index] == null)
                continue;

            _renderers[index].SetPropertyBlock(
                originalBlocks[index]);
        }

        _damageFlashCoroutine = null;

        if (dieAfterFlash)
        {
            Dead();
        }
    }

    /// <summary>
    /// 経路に沿って移動するコルーチン。
    /// </summary>
    private IEnumerator MovePathRoutine(
        IReadOnlyList<GridCell> path,
        System.Action onComplete)
    {
        _isMoving = true;

        // path[0]は現在地なので1から開始する
        for (int index = 1;
             index < path.Count;
             index++)
        {
            Vector3 destinationPosition =
                path[index].transform.position;

            while (Vector3.Distance(
                       transform.position,
                       destinationPosition) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        destinationPosition,
                        _moveAnimationSpeed *
                        Time.deltaTime);

                yield return null;
            }

            transform.position =
                destinationPosition;
        }

        _isMoving = false;
        onComplete?.Invoke();
    }
}