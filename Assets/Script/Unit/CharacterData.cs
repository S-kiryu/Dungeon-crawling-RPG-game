using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("基本情報")]
    public string CharacterName;
    public TeamType TeamType;

    [Header("見た目")]
    public Unit Prefab;
    public Sprite Icon;

    [Tooltip("Unit生成時に子オブジェクトとして追加するキャラクターの外観Prefab")]
    public GameObject VisualPrefab;

    [Tooltip("外観PrefabのAnimatorへ設定するController")]
    public RuntimeAnimatorController AnimatorController;

    public Vector3 VisualLocalPosition = Vector3.zero;
    public Vector3 VisualLocalEulerAngles = Vector3.zero;
    public Vector3 VisualLocalScale = Vector3.one;

    [Header("ステータス")]
    public StatusBase Status;

    [Header("攻撃の長さ")]
    public ActionRangeData RangeData;

    [Header("初期スキル")]
    public List<SkillData> Skills = new();
}
