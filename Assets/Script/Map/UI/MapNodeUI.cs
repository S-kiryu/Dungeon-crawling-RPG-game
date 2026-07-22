using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _eventText;
    [SerializeField] private Button _button;

    public void Setup(MapNode node, MapManager mapManager)
    {
        _eventText.text = node.EventType.ToString();

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => mapManager.SelectNode(node));
    }
}