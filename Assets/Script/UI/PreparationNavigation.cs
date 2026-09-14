using UnityEngine;
using UnityEngine.SceneManagement;

public class PreparationNavigation : MonoBehaviour
{
    public void StartDungeon()
    {
        FormationManager formation =
            FormationManager.Instance;

        if (formation == null ||
            !formation.CanStartBattle)
        {
            Debug.LogWarning(
                "出撃キャラクターを1人以上編成してください。");
            return;
        }

        SceneManager.LoadScene("TreeMapScene");
    }
}