using UnityEngine;
[CreateAssetMenu(menuName = "Item/GearItemData")]
public class GearItemData : ItemData
{
    public GearItemType gearType;

    [Header("ステータス補正")]
    public int attackBonus;
    public int defenseBonus;
    public int maxHpBonus;
    public int SpeedBonus;
    public int WeightBonus;
}
