using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ノードデータ
/// </summary>
public class MapNode
{
    public MapEventType EventType;

    /// <summary>
    /// マップ生成時に決定された、このノードで使用する戦闘シナリオ。
    /// </summary>
    public BattleScenarioData BattleScenario;

    [Header("マップ情報")]
    [Tooltip("ホバー情報に表示する名前。空ならイベント種別から自動設定します。")]
    public string DisplayName;

    [TextArea(2, 4)]
    [Tooltip("ホバー情報に表示する説明文。")]
    public string Description;

    [Tooltip("ホバー情報のイメージ。未設定ならノードのイベントアイコンを使います。")]
    public Sprite PreviewImage;

    [Min(0)]
    public int EnemyCount;

    public MapNodeDifficulty Difficulty;

    [Min(0)]
    public int RewardGold;

    [Tooltip("このノードだけに使う本体画像。未設定ならMapNodeUIの種別設定を使います。")]
    public Sprite NodeSprite;

    [Tooltip("このノードだけに使うイベントアイコン。未設定ならMapNodeUIの種別設定を使います。")]
    public Sprite EventIcon;

    public List<MapNode> NextNodes { get; } = new();

    /// <summary>
    /// マップのタイプといける次のノードを設定する
    /// </summary>
    /// <param name="eventType">タイプ</param>
    /// <param name="nextNodes">次のノード</param>
    public void AddNextNode(MapNode nextNodes)
    {
        NextNodes.Add(nextNodes);
    }
}

/// <summary>
/// マップ上で事前に伝える、おおまかな戦闘難易度。
/// </summary>
public enum MapNodeDifficulty
{
    None,
    Easy,
    Normal,
    Hard,
    Boss
}
