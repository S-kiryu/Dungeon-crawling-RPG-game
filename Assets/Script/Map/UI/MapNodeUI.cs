using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// マップ上のノードを描画するUIクラス
/// </summary>
public class MapNodeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _eventText;
    [SerializeField] private Button _button;
    
    /// <summary>
    /// UIを初期化する
    /// </summary>
    /// <param name="node">関連するマップノード</param>
    /// <param name="mapManager">マップマネージャー</param>
    public void Setup(MapNode node, MapManager mapManager)
    {
        _eventText.text = node.EventType.ToString();

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => mapManager.SelectNode(node));
    }
}