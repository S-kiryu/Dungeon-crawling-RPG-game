using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 戦闘ユニットのUnity View。
/// 既存コード向けの公開APIは内部のUnitModelへ委譲する。
/// </summary>
public class Unit : MonoBehaviour
{
    private static readonly int IsMovingParameter =
        Animator.StringToHash("IsMoving");
    private static readonly int AttackParameter =
        Animator.StringToHash("Attack");
    private static readonly int HitParameter =
        Animator.StringToHash("Hit");
    private static readonly int DieParameter =
        Animator.StringToHash("Die");

    private static readonly IReadOnlyList<SkillData> EmptySkills =
        Array.Empty<SkillData>();

    public UnitModel Model => _model;
    public CharacterData Data => _model?.Data;
    public CurrentStatus Status => _model?.Status;
    public GridCell CurrentCell { get; private set; }
    public TeamType Team => _model?.Team ?? TeamType.Neutral;
    public ActionRangeData RangeData => _model?.RangeData;
    public IReadOnlyList<SkillData> Skills =>
        _model?.Skills ?? EmptySkills;
    public CharacterInstance SourceCharacter =>
        _model?.SourceCharacter;
    public Vector2Int FacingDirection { get; private set; }
        = Vector2Int.right;
    public bool IsMoving => _isMoving;
    public bool IsDead => _model != null && _model.IsDead;

    /// <summary>
    /// いずれかのユニットが実際にダメージを受けたときに通知する。
    /// 第2引数は防御力を反映した実ダメージ。
    /// </summary>
    public static event Action<Unit, int> AnyUnitDamaged;

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

        return InitializeModel(
            new UnitModel(
                character.CharacterData,
                new CurrentStatus(character.Status),
                TeamType.Player,
                character.CharacterData.RangeData,
                character.Skills,
                character),
            gridCell);
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

        return InitializeModel(
            new UnitModel(
                characterData,
                new CurrentStatus(characterData.Status),
                team,
                actionRange,
                characterData.Skills),
            gridCell);
    }

    /// <summary>
    /// 指定された経路に沿って移動する。
    /// </summary>
    public void MoveAlongPath(
        IReadOnlyList<GridCell> path,
        Action onComplete)
    {
        if (_isMoving ||
            path == null ||
            path.Count < 2)
        {
            return;
        }

        CurrentCell = path[path.Count - 1];
        StartCoroutine(MovePathRoutine(path, onComplete));
    }

    /// <summary>
    /// ダメージを受ける。
    /// </summary>
    public void TakeDamage(int rawDamage)
    {
        if (_model == null || _model.IsDead)
            return;

        int actualDamage = _model.TakeDamage(rawDamage);
        AnyUnitDamaged?.Invoke(this, actualDamage);

        if (_animator != null)
        {
            _animator.SetTrigger(
                _model.IsDead
                    ? DieParameter
                    : HitParameter);
        }

        if (_damageFlashCoroutine != null)
        {
            StopCoroutine(_damageFlashCoroutine);
        }

        _damageFlashCoroutine = StartCoroutine(
            DamageFlashRoutine(_model.IsDead));
    }

    /// <summary>
    /// 通常攻撃・近接スキルの攻撃モーションを再生する。
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (_animator == null || IsDead)
            return;

        _animator.SetTrigger(AttackParameter);
    }

    /// <summary>
    /// グリッド上の4方向へユニットを向ける。
    /// </summary>
    public void SetFacing(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
            return;

        if (Mathf.Abs(direction.x) >=
            Mathf.Abs(direction.y))
        {
            FacingDirection = direction.x >= 0
                ? Vector2Int.right
                : Vector2Int.left;
        }
        else
        {
            FacingDirection = direction.y >= 0
                ? Vector2Int.up
                : Vector2Int.down;
        }

        Vector3 worldDirection = new Vector3(
            FacingDirection.x,
            0f,
            FacingDirection.y);

        transform.rotation = Quaternion.LookRotation(
            worldDirection,
            Vector3.up);
    }

    /// <summary>
    /// 防御力を反映した実際のダメージを計算する。
    /// </summary>
    public int CalculateDamageTaken(int rawDamage)
    {
        return _model?.CalculateDamageTaken(rawDamage) ?? 0;
    }

    [Header("ダメージ演出")]
    [SerializeField]
    private Color _damageColor = Color.red;

    [SerializeField]
    private float _damageFlashSeconds = 0.2f;

    [SerializeField]
    private float _deathAnimationSeconds = 1.5f;

    [Header("移動")]
    [SerializeField]
    private float _moveAnimationSpeed = 5f;

    private UnitModel _model;
    private bool _isMoving;
    private Renderer[] _renderers;
    private Renderer[] _placeholderRenderers;
    private GameObject _visualInstance;
    private Animator _animator;
    private Coroutine _damageFlashCoroutine;

    private void Awake()
    {
        _placeholderRenderers = GetComponents<Renderer>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    private bool InitializeModel(
        UnitModel model,
        GridCell gridCell)
    {
        if (model == null ||
            gridCell == null ||
            !gridCell.TrySetUnit(this))
        {
            return false;
        }

        _model = model;
        CurrentCell = gridCell;
        transform.position = gridCell.transform.position;
        CreateCharacterVisual(model.Data);
        SetFacing(
            model.Team == TeamType.Enemy
                ? Vector2Int.left
                : Vector2Int.right);

        return true;
    }

    private void CreateCharacterVisual(CharacterData characterData)
    {
        if (characterData == null || characterData.VisualPrefab == null)
            return;

        if (_visualInstance != null)
        {
            Destroy(_visualInstance);
        }

        _visualInstance = Instantiate(
            characterData.VisualPrefab,
            transform);

        Transform visualTransform = _visualInstance.transform;
        visualTransform.localPosition =
            characterData.VisualLocalPosition;
        visualTransform.localRotation = Quaternion.Euler(
            characterData.VisualLocalEulerAngles);
        visualTransform.localScale =
            characterData.VisualLocalScale;

        foreach (Renderer placeholderRenderer in _placeholderRenderers)
        {
            if (placeholderRenderer != null)
            {
                placeholderRenderer.enabled = false;
            }
        }

        _renderers =
            _visualInstance.GetComponentsInChildren<Renderer>();

        _animator =
            _visualInstance.GetComponentInChildren<Animator>();

        if (_animator != null)
        {
            _animator.applyRootMotion = false;

            if (characterData.AnimatorController != null)
            {
                _animator.runtimeAnimatorController =
                    characterData.AnimatorController;
            }
        }
    }

    private void RemoveFromBoard()
    {
        if (CurrentCell != null)
        {
            CurrentCell.RemoveUnit();
            CurrentCell = null;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator DamageFlashRoutine(bool removeAfterFlash)
    {
        MaterialPropertyBlock[] originalBlocks =
            new MaterialPropertyBlock[_renderers.Length];

        for (int index = 0; index < _renderers.Length; index++)
        {
            Renderer targetRenderer = _renderers[index];
            MaterialPropertyBlock originalBlock =
                new MaterialPropertyBlock();

            targetRenderer.GetPropertyBlock(originalBlock);
            originalBlocks[index] = originalBlock;

            MaterialPropertyBlock damageBlock =
                new MaterialPropertyBlock();

            targetRenderer.GetPropertyBlock(damageBlock);
            damageBlock.SetColor("_BaseColor", _damageColor);
            damageBlock.SetColor("_Color", _damageColor);
            targetRenderer.SetPropertyBlock(damageBlock);
        }

        yield return new WaitForSeconds(_damageFlashSeconds);

        for (int index = 0; index < _renderers.Length; index++)
        {
            if (_renderers[index] != null)
            {
                _renderers[index].SetPropertyBlock(originalBlocks[index]);
            }
        }

        _damageFlashCoroutine = null;

        if (removeAfterFlash)
        {
            float remainingDeathSeconds = Mathf.Max(
                0f,
                _deathAnimationSeconds -
                _damageFlashSeconds);

            if (remainingDeathSeconds > 0f)
            {
                yield return new WaitForSeconds(
                    remainingDeathSeconds);
            }

            RemoveFromBoard();
        }
    }

    private IEnumerator MovePathRoutine(
        IReadOnlyList<GridCell> path,
        Action onComplete)
    {
        _isMoving = true;
        SetMovingAnimation(true);

        for (int index = 1; index < path.Count; index++)
        {
            Vector3 destinationPosition =
                path[index].transform.position;

            Vector3 moveDirection =
                destinationPosition - transform.position;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                SetFacing(new Vector2Int(
                    Mathf.RoundToInt(moveDirection.x),
                    Mathf.RoundToInt(moveDirection.z)));
            }

            while (Vector3.Distance(
                       transform.position,
                       destinationPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destinationPosition,
                    _moveAnimationSpeed * Time.deltaTime);

                yield return null;
            }

            transform.position = destinationPosition;
        }

        _isMoving = false;
        SetMovingAnimation(false);
        onComplete?.Invoke();
    }

    private void SetMovingAnimation(bool isMoving)
    {
        if (_animator != null)
        {
            _animator.SetBool(
                IsMovingParameter,
                isMoving);
        }
    }
}
