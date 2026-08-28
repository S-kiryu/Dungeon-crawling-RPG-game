using UnityEngine;

/// <summary>
/// アイテムの実態クラス
/// </summary>
public class Item : MonoBehaviour
{
    public ItemData Data { get; private set; }

    public void Initialize(ItemData itemData) 
    {
        Data = itemData;
    }
}
