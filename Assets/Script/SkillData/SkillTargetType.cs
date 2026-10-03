/// <summary>
/// スキルで選択できる対象。
/// </summary>
public enum SkillTargetType
{
    Enemy = 0,// 敵ユニット
    Ally = 1,// 味方ユニット
    Self = 2,// 自分自身
    AnyUnit = 3,// 生存している全ユニット
    EmptyCell = 4// 空のセル
}
