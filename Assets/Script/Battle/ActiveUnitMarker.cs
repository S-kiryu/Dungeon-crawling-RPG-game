using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 現在行動中のユニットの足元へ、
/// プロシージャルな魔法陣を表示する。
/// </summary>
[DisallowMultipleComponent]
public class ActiveUnitMarker : MonoBehaviour
{
    private const string ShaderResourcePath =
        "Shaders/ActiveUnitMagicCircle";

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    private static readonly int RotationSpeedId =
        Shader.PropertyToID("_RotationSpeed");

    private static readonly int StateIntensityId =
        Shader.PropertyToID("_StateIntensity");

    public void Initialize(
        BattleManager battleManager)
    {
        Unsubscribe();

        _battleManager = battleManager;

        EnsureMarkerObject();
        Subscribe();
        Refresh();
    }

    [Header("配置")]
    [SerializeField]
    private float _heightOffset = 0.53f;

    [SerializeField]
    private float _diameter = 1.12f;

    [Header("見た目")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color _color =
        new Color(1.8f, 0.72f, 0.08f, 1f);

    [SerializeField]
    private float _normalRotationSpeed = 0.12f;

    [SerializeField]
    private float _executingRotationSpeed = 0.28f;

    private BattleManager _battleManager;
    private Unit _trackedUnit;
    private GameObject _markerObject;
    private MeshRenderer _markerRenderer;
    private Material _runtimeMaterial;
    private bool _subscribed;

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (_markerObject != null)
            _markerObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Unsubscribe();

        if (_markerObject != null)
            Destroy(_markerObject);

        if (_runtimeMaterial != null)
            Destroy(_runtimeMaterial);
    }

    private void LateUpdate()
    {
        if (_trackedUnit == null ||
            _markerObject == null ||
            !_markerObject.activeSelf)
        {
            return;
        }

        _markerObject.transform.position =
            _trackedUnit.transform.position +
            Vector3.up * _heightOffset;
    }

    private void Subscribe()
    {
        if (_subscribed ||
            _battleManager == null ||
            !isActiveAndEnabled)
        {
            return;
        }

        _battleManager.TurnOrderChanged +=
            Refresh;

        _battleManager.TurnActionsChanged +=
            Refresh;

        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed ||
            _battleManager == null)
        {
            return;
        }

        _battleManager.TurnOrderChanged -=
            Refresh;

        _battleManager.TurnActionsChanged -=
            Refresh;

        _subscribed = false;
    }

    private void Refresh()
    {
        EnsureMarkerObject();

        if (_battleManager == null ||
            _markerObject == null ||
            _markerRenderer == null)
        {
            return;
        }

        Unit currentUnit =
            _battleManager.CurrentTurnUnit;

        bool shouldShow =
            currentUnit != null &&
            !currentUnit.IsDead &&
            _battleManager.CurrentState !=
                BattleState.BattleFinished;

        _trackedUnit = shouldShow
            ? currentUnit
            : null;

        _markerObject.SetActive(
            shouldShow);

        if (!shouldShow ||
            _runtimeMaterial == null)
        {
            return;
        }

        bool isExecuting =
            _battleManager.CurrentState ==
                BattleState.ExecutingAction ||
            _battleManager.CurrentState ==
                BattleState.EnemyTurn;

        _runtimeMaterial.SetFloat(
            RotationSpeedId,
            isExecuting
                ? _executingRotationSpeed
                : _normalRotationSpeed);

        _runtimeMaterial.SetFloat(
            StateIntensityId,
            isExecuting ? 1.25f : 1f);

        _markerObject.transform.position =
            currentUnit.transform.position +
            Vector3.up * _heightOffset;
    }

    private void EnsureMarkerObject()
    {
        if (_markerObject != null)
            return;

        Shader markerShader =
            Resources.Load<Shader>(
                ShaderResourcePath);

        if (markerShader == null)
        {
            markerShader = Shader.Find(
                "Dungeon/ActiveUnitMagicCircle");
        }

        if (markerShader == null)
        {
            Debug.LogError(
                "行動中ユニット用の魔法陣シェーダーが見つかりません。",
                this);

            return;
        }

        _runtimeMaterial =
            new Material(markerShader)
            {
                name =
                    "Active Unit Magic Circle (Runtime)",
                hideFlags = HideFlags.DontSave
            };

        _runtimeMaterial.SetColor(
            ColorId,
            _color);

        _runtimeMaterial.SetFloat(
            RotationSpeedId,
            _normalRotationSpeed);

        _markerObject =
            GameObject.CreatePrimitive(
                PrimitiveType.Quad);

        _markerObject.name =
            "Active Unit Magic Circle";

        _markerObject.hideFlags =
            HideFlags.DontSave;

        Collider markerCollider =
            _markerObject.GetComponent<Collider>();

        if (markerCollider != null)
            Destroy(markerCollider);

        _markerObject.transform.rotation =
            Quaternion.Euler(90f, 0f, 0f);

        _markerObject.transform.localScale =
            new Vector3(
                _diameter,
                _diameter,
                1f);

        _markerRenderer =
            _markerObject.GetComponent<MeshRenderer>();

        _markerRenderer.sharedMaterial =
            _runtimeMaterial;

        _markerRenderer.shadowCastingMode =
            ShadowCastingMode.Off;

        _markerRenderer.receiveShadows = false;
        _markerRenderer.lightProbeUsage =
            LightProbeUsage.Off;

        _markerRenderer.reflectionProbeUsage =
            ReflectionProbeUsage.Off;

        _markerObject.SetActive(false);
    }
}
