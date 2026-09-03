using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 戦闘終了時の結果処理を行うコントローラー
/// </summary>
public class BattleResultController :
    MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    private void OnEnable()
    {
        if (_battleManager != null)
        {
            _battleManager.BattleEnded +=
                HandleBattleEnded;
        }
    }

    private void OnDisable()
    {
        if (_battleManager != null)
        {
            _battleManager.BattleEnded -=
                HandleBattleEnded;
        }
    }

    /// <summary>
    /// 戦闘終了時の処理を行う
    /// </summary>
    /// <param name="playerWon"></param>
    private void HandleBattleEnded(
        bool playerWon)
    {
        DungeonRunSession runSession =
            DungeonRunSession.Instance;

        FormationManager.Instance
            ?.PruneInvalidSlots();

        if (runSession == null)
        {
            SceneManager.LoadScene(
                "PreparationScene");
            return;
        }

        if (!playerWon)
        {
            Debug.Log("探索失敗");

            runSession.EndRun();

            SceneManager.LoadScene(
                "PreparationScene");

            return;
        }

        if (!runSession.CompletePendingNode(
                out MapEventType completedType))
        {
            Debug.LogWarning(
                "攻略中のノードがありません。");

            SceneManager.LoadScene(
                "PreparationScene");

            return;
        }

        if (completedType ==
            MapEventType.Boss)
        {
            Debug.Log("階層クリア");

            runSession.EndRun();

            SceneManager.LoadScene(
                "PreparationScene");

            return;
        }

        // 通常戦闘勝利
        SceneManager.LoadScene(
            "TreeMapScene");
    }
}