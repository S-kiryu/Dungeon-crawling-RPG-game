using UnityEngine;

/// <summary>
/// ユニットを設定するマス
/// </summary>
public class SetFormationGrid : MonoBehaviour
{
    private int _formationNumber;
    private UnitSettingData _unit;

    private void Initialize(int FormationNumber, UnitSettingData Unit) 
    {
        _formationNumber = FormationNumber;
        _unit = Unit;
    }

    /// <summary>
    /// 設置されているユニットを取得
    /// </summary>
    /// <returns></returns>
    private UnitSettingData GetUnit() 
    {
        return _unit;
    }
}
