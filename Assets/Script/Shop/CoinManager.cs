using UnityEngine;

/// <summary>
/// コインを管理するクラス
/// </summary>
public class CoinManager : MonoBehaviour
{
    public int Coin => coin;

    public void AddCoin(int num) 
    {
        if (num < 0) 
        {
            Debug.LogWarning("正の数だけを入れてね");
            return;
        }
        coin += num;
    }

    public void RemoveCoin(int num) 
    {
        if (num < 0) 
        {
            Debug.Log("お金が足りないよ");
            return;
        }
        coin -= num;
    }

    [SerializeField]
    private int coin = 0;
}
