using UnityEditor;
using UnityEngine;

/// <summary>
/// アクションの範囲を編集するためのカスタムエディタ
/// </summary>
[CustomEditor(typeof(ActionRangeData))]
public class ActionRangeDataEditor : Editor
{
    private const int MinRadius = 1;
    private const int MaxRadius = 15;
    private const float MinCellSize = 18f;
    private const float MaxCellSize = 30f;

    private SerializedProperty originProperty;
    private SerializedProperty offsetsProperty;
    private int gridRadius = 3;
    private bool showRawOffsets;

    private void OnEnable()
    {
        originProperty = serializedObject.FindProperty("origin");
        offsetsProperty = serializedObject.FindProperty("offsets");

        // アセットを選択したとき、既存のすべての座標が見える大きさにする。
        gridRadius = Mathf.Clamp(GetRequiredRadius(), MinRadius, MaxRadius);
    }

    /// <summary>
    /// ボタンなどのGUIを描画する
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(originProperty);
        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField("範囲ペインター", EditorStyles.boldLabel);
        gridRadius = EditorGUILayout.IntSlider("表示半径", gridRadius, MinRadius, MaxRadius);

        int requiredRadius = GetRequiredRadius();
        if (requiredRadius > gridRadius)
        {
            EditorGUILayout.HelpBox(
                "保存済みのマスが表示範囲の外にあります。編集するには表示半径を大きくしてください。",
                MessageType.Warning);
        }

        EditorGUILayout.HelpBox(
            "マスをクリックすると範囲を追加・削除できます。Cは中心座標 (0, 0)、上方向が +Y です。",
            MessageType.Info);

        DrawGrid();
        EditorGUILayout.Space(4f);
        DrawToolbar();

        EditorGUILayout.Space(6f);
        showRawOffsets = EditorGUILayout.Foldout(showRawOffsets, "座標データを直接編集", true);
        if (showRawOffsets)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(offsetsProperty, true);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// グリットの生成
    /// </summary>
    private void DrawGrid()
    {
        int diameter = gridRadius * 2 + 1;
        float availableWidth = Mathf.Max(0f, EditorGUIUtility.currentViewWidth - 42f);
        float cellSize = Mathf.Clamp(availableWidth / diameter, MinCellSize, MaxCellSize);

        GUIStyle cellStyle = new GUIStyle(GUI.skin.button)
        {
            margin = new RectOffset(1, 1, 1, 1),
            padding = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical(GUILayout.Width(cellSize * diameter));
        for (int y = gridRadius; y >= -gridRadius; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                Vector2Int offset = new Vector2Int(x, y);
                bool selected = ContainsOffset(offset);
                bool isCenter = offset == Vector2Int.zero;

                Color previousColor = GUI.backgroundColor;
                if (selected)
                    GUI.backgroundColor = isCenter
                        ? new Color(1f, 0.65f, 0.2f)
                        : new Color(0.25f, 0.75f, 1f);
                else if (isCenter)
                    GUI.backgroundColor = new Color(0.75f, 0.55f, 0.25f);

                GUIContent content = new GUIContent(
                    isCenter ? "C" : string.Empty,
                    $"座標 ({x}, {y})");

                if (GUILayout.Button(content, cellStyle,
                        GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                {
                    ToggleOffset(offset);
                }

                GUI.backgroundColor = previousColor;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("すべて消去"))
        {
            Undo.RecordObject(target, "行動範囲をすべて消去");
            offsetsProperty.arraySize = 0;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("表示範囲を合わせる"))
            gridRadius = Mathf.Clamp(GetRequiredRadius(), MinRadius, MaxRadius);

        EditorGUILayout.EndHorizontal();
    }

    private bool ContainsOffset(Vector2Int offset)
    {
        for (int i = 0; i < offsetsProperty.arraySize; i++)
        {
            if (offsetsProperty.GetArrayElementAtIndex(i).vector2IntValue == offset)
                return true;
        }

        return false;
    }

    private void ToggleOffset(Vector2Int offset)
    {
        Undo.RecordObject(target, "行動範囲を編集");

        // 過去のデータに重複があった場合も整理できるよう、一致する座標をすべて削除する。
        bool removed = false;
        for (int i = offsetsProperty.arraySize - 1; i >= 0; i--)
        {
            if (offsetsProperty.GetArrayElementAtIndex(i).vector2IntValue != offset)
                continue;

            offsetsProperty.DeleteArrayElementAtIndex(i);
            removed = true;
        }

        if (!removed)
        {
            int newIndex = offsetsProperty.arraySize;
            offsetsProperty.InsertArrayElementAtIndex(newIndex);
            offsetsProperty.GetArrayElementAtIndex(newIndex).vector2IntValue = offset;
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private int GetRequiredRadius()
    {
        int requiredRadius = MinRadius;

        if (offsetsProperty == null)
            return requiredRadius;

        for (int i = 0; i < offsetsProperty.arraySize; i++)
        {
            Vector2Int offset = offsetsProperty.GetArrayElementAtIndex(i).vector2IntValue;
            requiredRadius = Mathf.Max(requiredRadius, Mathf.Abs(offset.x), Mathf.Abs(offset.y));
        }

        return requiredRadius;
    }
}
