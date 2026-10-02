using UnityEngine;

/// <summary>
/// セル単体が持っている情報を管理するクラス
/// </summary>
public class GridCell : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;

    [SerializeField]
    private MeshRenderer _outlineRenderer;

    private MaterialPropertyBlock
        _outlinePropertyBlock;

    private bool _hasOutline;
    private Color _outlineColor;
    private bool _isHovered;
    private Color _hoverColor;

    private static readonly int
        OutlineColorId =
            Shader.PropertyToID(
                "_OutlineColor");

    public Vector2Int Position { get; private set; }

    public Unit CurrentUnit { get; private set; }

    public TerrainType Terrain { get; private set; }

    public bool IsOccupied => CurrentUnit != null;

    private void Awake()
    {
        _outlinePropertyBlock =
            new MaterialPropertyBlock();

        HideOutline();
    }

    public void Initialize(Vector2Int position)
    {
        Position = position;
    }

    /// <summary>
    /// セルにユニットを配置する
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool TrySetUnit(Unit unit)
    {
        if (unit == null || IsOccupied)
            return false;

        CurrentUnit = unit;
        return true;
    }

    /// <summary>
    /// セルからユニットを削除する
    /// </summary>
    public void RemoveUnit()
    {
        CurrentUnit = null;
    }

    /// <summary>
    /// セルのマテリアルを設定する
    /// </summary>
    /// <param name="material"></param>
    public void SetMaterial(Material material)
    {
        if (_renderer != null)
        {
            _renderer.sharedMaterial = material;
        }
    }

    /// <summary>
    /// セルの枠を指定色で表示する。
    /// </summary>
    public void ShowOutline(Color color)
    {
        _outlineColor = color;
        _hasOutline = true;

        RefreshOutline();
    }

    /// <summary>
    /// セルの枠を非表示にする。
    /// </summary>
    public void HideOutline()
    {
        _hasOutline = false;

        RefreshOutline();
    }

    /// <summary>
    /// カーソルが乗っている間だけ専用色で枠を表示する。
    /// </summary>
    public void ShowHover(Color color)
    {
        _hoverColor = color;
        _isHovered = true;

        RefreshOutline();
    }

    /// <summary>
    /// ホバー表示を解除し、元の範囲色へ戻す。
    /// </summary>
    public void HideHover()
    {
        _isHovered = false;

        RefreshOutline();
    }

    private void RefreshOutline()
    {
        if (_outlineRenderer == null)
            return;

        if (_isHovered)
        {
            ApplyOutlineColor(
                _hoverColor);

            _outlineRenderer.enabled = true;
            return;
        }

        if (_hasOutline)
        {
            ApplyOutlineColor(
                _outlineColor);

            _outlineRenderer.enabled = true;
            return;
        }

        _outlineRenderer.enabled = false;
    }

    private void ApplyOutlineColor(
        Color color)
    {
        if (_outlinePropertyBlock == null)
        {
            _outlinePropertyBlock =
                new MaterialPropertyBlock();
        }

        _outlineRenderer.GetPropertyBlock(
            _outlinePropertyBlock);

        _outlinePropertyBlock.SetColor(
            OutlineColorId,
            color);

        _outlineRenderer.SetPropertyBlock(
            _outlinePropertyBlock);
    }
}
