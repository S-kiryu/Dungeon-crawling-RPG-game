using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;
using JetBrains.Annotations;

/// <summary>
/// ユニットの編成を管理するクラス
/// </summary>
public class UnitFormation : MonoBehaviour
{
    [Header("編成画面の設定項目")]
    private int _formationNums = 4;

    private UnitSettingData[] _settingData = null;
    private Image[] Icons;
    private Image _iconPrefab;
    private Transform _iconParent;

    private void Start()
    {
        
    }

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
            Icons[i] = generatedItem;
        }
    }
}
