[System.Serializable]
public class CurrentStatus
{
    public int MaxHP;
    public int CurrentHP;

    public int Attack;
    public int Defense;
    public int Speed;
    public int MoveLength;
    public int Weight;

    public CurrentStatus(StatusBase status)
    {
        MaxHP = status.HP;
        CurrentHP = status.HP;

        Attack = status.Attack;
        Defense = status.Deffens;
        Speed = status.Speed;
        MoveLength = status.MoveLength;
        Weight = status.Weight;
    }
}