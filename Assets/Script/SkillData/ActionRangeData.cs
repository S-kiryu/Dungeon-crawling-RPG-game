using UnityEngine;

[CreateAssetMenu(menuName = "Range/RangeData")]
public class ActionRangeData : ScriptableObject
{
    [SerializeField] private ActionRangeOrigin origin = ActionRangeOrigin.Target;
    [SerializeField] private Vector2Int[] offsets;

    public ActionRangeOrigin Origin => origin;
    public Vector2Int[] Offsets => offsets;
}