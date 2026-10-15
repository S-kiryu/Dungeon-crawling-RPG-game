using TMPro;
using UnityEngine;

/// <summary>
/// 戦闘終了時のUI表示を管理する。
/// </summary>
public class BattleResultController : MonoBehaviour
{
    [SerializeField]
    private BattleManager _battleManager;

    [Header("戦闘中UI")]
    [SerializeField]
    private GameObject _commandView;

    [Header("戦闘結果UI")]
    [SerializeField]
    private GameObject _resultView;

    [SerializeField]
    private TMP_Text _resultText;

    private bool _resultCommitted;

    private void Awake()
    {
        if (_resultView != null)
            _resultView.SetActive(false);
    }

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
    /// 戦闘終了時にコマンドを閉じ、
    /// 勝敗結果を表示する。
    /// </summary>
    private void HandleBattleEnded(
        bool playerWon)
    {
        CommitDungeonResult(playerWon);

        if (_commandView != null)
            _commandView.SetActive(false);

        if (_resultView != null)
            _resultView.SetActive(true);

        if (_resultText != null)
        {
            _resultText.text =
                playerWon
                    ? "勝利"
                    : "敗北";
        }
    }

    private void CommitDungeonResult(bool playerWon)
    {
        if (_resultCommitted)
            return;

        _resultCommitted = true;

        DungeonRunSession session = DungeonRunSession.Instance;

        if (session == null)
            return;

        if (playerWon)
        {
            int rewardGold =
                session.PendingNode?.RewardGold ?? 0;

            bool completed =
                session.CompletePendingNode(out _);

            if (completed && CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoin(rewardGold);
            }
        }
        else
        {
            session.CancelPendingNode();
        }
    }
}
