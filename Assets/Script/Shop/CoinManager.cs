using System;
using UnityEngine;

/// <summary>
/// ゲーム全体のコインを管理するクラス。
/// </summary>
[DefaultExecutionOrder(-1000)]
public sealed class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    public int Coin => _coin;

    public event Action<int> CoinChanged;

    public void AddCoin(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "追加するコインは0以上にしてください。");
            return;
        }

        _coin += amount;
        CoinChanged?.Invoke(_coin);
    }

    public bool TrySpendCoin(int amount)
    {
        if (amount < 0 || _coin < amount)
            return false;

        _coin -= amount;
        CoinChanged?.Invoke(_coin);
        return true;
    }

    [SerializeField, Min(0)]
    private int _initialCoin;

    private int _coin;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _coin = _initialCoin;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
