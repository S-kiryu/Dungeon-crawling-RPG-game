using UnityEngine;

/// <summary>
/// ユニットを生成するクラス
/// </summary>
public class UnitGenerator : MonoBehaviour
{
    /// <summary>
    /// キャラを生成する関数
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    public Unit Spawn(CharacterData data, GridCell cell)
    {
        if (data == null || data.Prefab == null)
        {
            Debug.LogWarning("CharacterData または Unit Prefab が未設定です。");
            return null;
        }

        if (cell == null || cell.IsOccupied)
        {
            Debug.LogWarning("生成先のセルが無効、または既に埋まっています。");
            return null;
        }

        Unit unit = Instantiate(
            data.Prefab,
            cell.transform.position,
            Quaternion.identity);

        if (!unit.Initialize(data, cell,data.TeamType))
        {
            Destroy(unit.gameObject);
            return null;
        }

        return unit;
    }
}
