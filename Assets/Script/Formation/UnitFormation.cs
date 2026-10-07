using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ユニットの編成を管理するクラス
/// </summary>
public class UnitFormation : MonoBehaviour
{
    [SerializeField]
    private Image _iconPrefab;

    [SerializeField]
    private Transform _iconParent;

    private UnitSettingData[] _settingData = null;
    private readonly List<Image> _icons = new();

    /// <summary>
    /// 持っているユニットを取得
    /// </summary>
    /// <param name="unit"></param>
    private void GetUnit(UnitSettingData[] unit)
    {
        _settingData = unit;
    }

    /// <summary>
    /// 持っているユニットのアイコンを生成する
    /// </summary>
    private void GeneretIcon()
    {
        if (_settingData == null)
        {
            Debug.LogWarning("先にユニットを取得してください");
            return;
        }
        for (int i = 0; i < _settingData.Length; i++)
        {
            Image generatedItem = Instantiate(_iconPrefab, _iconParent);
            _icons.Add(generatedItem);
        }
    }
}
