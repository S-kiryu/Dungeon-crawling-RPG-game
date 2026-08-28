using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("ショップアイテムの設定")]
    [SerializeField]
    private GearItemData[] _gearItems;
    [SerializeField]
    private Item _itemPrefab;
    [SerializeField]
    private Transform _itemParent;

    [Header("各アイテムの表示数")]
    private int _gearIndex;
    private int _potionIndex;

    [Header("ショップのリロールコスト")]
    private int _rerollCost;

    //生成したアイテムの参照を持っておく場所
    private List<Item> _items;

    /// <summary>
    /// ショップアイテムを生成する
    /// </summary>
    private void GenerateShopItems() 
    {
        for (int i = 0; i < _gearIndex; i++) 
        {
            GenerateRandomGear();
        }
    }

    /// <summary>
    /// アイテムのギアだけを個数分生成する関数
    /// </summary>
    private void GenerateRandomGear()
    {
        if (_gearItems == null || _gearItems.Length == 0)
        {
            Debug.LogWarning("ショップに装備品が設定されていません。", this);
            return;
        }

        int itemIndex = Random.Range(0, _gearItems.Length);
        GearItemData selectedData = _gearItems[itemIndex];

        Item generatedItem = Instantiate(_itemPrefab, _itemParent);
        generatedItem.Initialize(selectedData);

        _items.Add(generatedItem);
    }

    /// <summary>
    /// ショップを初期化する関数
    /// </summary>
    private void ResetShop() 
    {
        foreach (Item item in _items) 
        {
            Destroy(item);
        }

        _items.Clear();
    }
}
