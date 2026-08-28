using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("基本情報")]
    public string itemId;
    public string itemName;

    [TextArea]
    public string description;

    public Sprite icon;

    [Header("ショップ情報")]
    [Min(0)]
    public int buyPrice;

    [Min(0)]
    public int sellPrice;

    public bool canBuy = true;
    public bool canSell = true;

    [Header("所持情報")]
    public bool canStack;

    [Min(1)]
    public int maxStack = 1;
}