[System.Serializable]
public class CurrentStatus
{
    public int MaxHP;
    public int CurrentHP;

    public int Attack;
    public int Defense;
    public int Speed;
    public int MoveSpeed;
    public int Weight;

    public CurrentStatus(StatusBase status)
    {
        MaxHP = status.HP;
        CurrentHP = status.HP;

        Attack = status.Attack;
        Defense = status.Deffens;
        Speed = status.Speed;
        MoveSpeed = status.MoveSpeed;
        Weight = status.Weight;
    }
}