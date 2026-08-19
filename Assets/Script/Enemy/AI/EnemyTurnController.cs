using System.Collections;
using UnityEngine;

/// <summary>
/// 敵のターンを制御するクラス
/// </summary>
public class EnemyTurnController : MonoBehaviour
{
    public IEnumerator ExecuteTurn()
    {
        Debug.Log("敵が行動します");

        // 仮の敵行動
        yield return new WaitForSeconds(1f);

        Debug.Log("敵の行動が終わりました");
    }
}