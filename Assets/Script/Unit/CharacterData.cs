using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("基本情報")]
    public string CharacterName;

    [Header("見た目")]
    public Unit Prefab;
    public Sprite Icon;

    [Header("ステータス")]
    public StatusBase Status;
}