using UnityEngine;
[CreateAssetMenu(menuName = "Battle/Character Status")]
public class StatusBase : ScriptableObject
{
    [Min(1)]
    public int Level = 0;
    public int HP = 0;
    public int Attack = 0;
    public int Deffens = 0;
    public int Speed = 0;
    public int MoveLength = 0;
    public int Weight = 0;
}
