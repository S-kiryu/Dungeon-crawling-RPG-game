using UnityEngine;

public class ShopEvent : EventBase
{
    public override MapEventType EventType => MapEventType.Shop;

    public override void Execute(MapNode node)
    {
        if (_shopManager == null || _mapManager == null)
        {
            Debug.LogError(
                "ShopEventにShopManagerまたはMapManagerが設定されていません。",
                this);
            return;
        }

        if (_waitingForClose)
            return;

        _waitingForClose = true;
        _shopManager.Closed += HandleShopClosed;
        _shopManager.OpenShop();
    }

    [SerializeField]
    private ShopManager _shopManager;

    [SerializeField]
    private MapManager _mapManager;

    private bool _waitingForClose;

    private void OnDisable()
    {
        UnsubscribeFromShop();
    }

    private void HandleShopClosed()
    {
        UnsubscribeFromShop();

        if (!_mapManager.CompletePendingNode())
        {
            Debug.LogWarning(
                "ショップノードの完了処理に失敗しました。",
                this);
        }
    }

    private void UnsubscribeFromShop()
    {
        if (_shopManager != null)
            _shopManager.Closed -= HandleShopClosed;

        _waitingForClose = false;
    }
}
