using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TreeMap上のショップ進行と商品在庫を管理する。
/// UIはシーン上に事前配置されたShopPanelUIを使用する。
/// </summary>
public sealed class ShopManager : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    public event Action Closed;

    public void OpenShop()
    {
        if (IsOpen)
            return;

        if (_shopPanel == null)
        {
            Debug.LogError(
                "ShopManagerにShopPanelUIが設定されていません。",
                this);
            Closed?.Invoke();
            return;
        }

        IsOpen = true;
        GenerateShopItems();
        _shopPanel.SetVisible(true);
        RefreshView();
    }

    public void CloseShop()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        _shopPanel?.SetVisible(false);
        Closed?.Invoke();
    }

    public bool TryReroll()
    {
        if (!IsOpen || CoinManager.Instance == null)
            return false;

        if (!CoinManager.Instance.TrySpendCoin(_rerollCost))
            return false;

        GenerateShopItems();
        RefreshView();
        return true;
    }

    public bool TryPurchase(ItemData itemData)
    {
        if (!CanPurchase(itemData))
            return false;

        CoinManager coinManager = CoinManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;

        if (!coinManager.TrySpendCoin(itemData.buyPrice))
            return false;

        if (!inventory.TryAddItem(itemData))
        {
            coinManager.AddCoin(itemData.buyPrice);
            return false;
        }

        _stock.Remove(itemData);
        RefreshView();
        return true;
    }

    [Header("ショップアイテムの設定")]
    [SerializeField]
    private GearItemData[] _gearItems;

    [SerializeField, Min(1)]
    private int _gearCount = 3;

    [SerializeField, Min(0)]
    private int _rerollCost = 10;

    [Header("シーン上に配置したUI")]
    [SerializeField]
    private ShopPanelUI _shopPanel;

    private readonly List<ItemData> _stock = new();

    private void Awake()
    {
        if (_shopPanel == null)
            return;

        _shopPanel.CloseRequested += CloseShop;
        _shopPanel.RerollRequested += HandleRerollRequested;
        _shopPanel.PurchaseRequested += HandlePurchaseRequested;
        _shopPanel.SetVisible(false);
    }

    private void OnDestroy()
    {
        if (_shopPanel == null)
            return;

        _shopPanel.CloseRequested -= CloseShop;
        _shopPanel.RerollRequested -= HandleRerollRequested;
        _shopPanel.PurchaseRequested -= HandlePurchaseRequested;
    }

    private void HandleRerollRequested()
    {
        TryReroll();
    }

    private void HandlePurchaseRequested(ItemData itemData)
    {
        TryPurchase(itemData);
    }

    private void GenerateShopItems()
    {
        _stock.Clear();

        if (_gearItems == null || _gearItems.Length == 0)
        {
            Debug.LogWarning(
                "ショップに装備品が設定されていません。",
                this);
            return;
        }

        List<GearItemData> candidates = new();

        foreach (GearItemData item in _gearItems)
        {
            if (item != null)
                candidates.Add(item);
        }

        int count = Mathf.Min(_gearCount, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            _stock.Add(candidates[index]);
            candidates.RemoveAt(index);
        }
    }

    private bool CanPurchase(ItemData itemData)
    {
        return IsOpen &&
               itemData != null &&
               itemData.canBuy &&
               _stock.Contains(itemData) &&
               CoinManager.Instance != null &&
               CoinManager.Instance.Coin >= itemData.buyPrice &&
               InventoryManager.Instance != null &&
               InventoryManager.Instance.CanAddItem(itemData);
    }

    private void RefreshView()
    {
        if (_shopPanel == null)
            return;

        int coin = CoinManager.Instance != null
            ? CoinManager.Instance.Coin
            : 0;

        _shopPanel.Render(
            coin,
            _rerollCost,
            _stock,
            CanPurchase);
    }
}
