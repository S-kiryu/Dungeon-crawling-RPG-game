[System.Serializable]
public class CurrentStatus
{
    public int Level;

    public int MaxHP;
    public int CurrentHP;

    public int Attack;
    public int Defense;
    public int Speed;
    public int MoveLength;
    public int Weight;

    /// <summary>
    /// CharacterDataの基礎ステータスから生成する。
    /// 敵や初期キャラの生成で使用する。
    /// </summary>
    public CurrentStatus(StatusBase status)
    {
        Level = status.Level;

        MaxHP = status.HP;
        CurrentHP = status.HP;

        Attack = status.Attack;
        Defense = status.Deffens;
        Speed = status.Speed;
        MoveLength = status.MoveLength;
        Weight = status.Weight;
    }

    /// <summary>
    /// 所持キャラのステータスを戦闘用にコピーする。
    /// </summary>
    public CurrentStatus(CurrentStatus source)
    {
        Level = source.Level;

        MaxHP = source.MaxHP;
        CurrentHP = source.CurrentHP;

        Attack = source.Attack;
        Defense = source.Defense;
        Speed = source.Speed;
        MoveLength = source.MoveLength;
        Weight = source.Weight;
    }
}