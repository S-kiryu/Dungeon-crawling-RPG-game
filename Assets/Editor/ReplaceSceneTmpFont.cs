using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class ReplaceSceneTmpFont
{
    private const string FontAssetPath =
        "Assets/Noto_Sans_JP/NotoSansJP-VariableFont_wght SDF.asset";

    [MenuItem("Tools/TMP/開いているシーンのフォントをNoto Sans JPに変更")]
    private static void ReplaceFont()
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog(
                "TMPフォント一括変更",
                $"フォントアセットが見つかりません。\n{FontAssetPath}",
                "OK");
            return;
        }

        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        var changedScenes = new HashSet<Scene>();
        int changedCount = 0;

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("TMPフォントをNoto Sans JPに一括変更");

        foreach (TMP_Text text in texts)
        {
            Scene scene = text.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || EditorUtility.IsPersistent(text))
            {
                continue;
            }

            if (text.font == fontAsset)
            {
                continue;
            }

            Undo.RecordObject(text, "TMPフォントを変更");
            text.font = fontAsset;
            EditorUtility.SetDirty(text);
            changedScenes.Add(scene);
            changedCount++;
        }

        foreach (Scene changedScene in changedScenes)
        {
            EditorSceneManager.MarkSceneDirty(changedScene);
        }

        Undo.CollapseUndoOperations(undoGroup);

        EditorUtility.DisplayDialog(
            "TMPフォント一括変更",
            $"{changedCount}個のTMPフォントを変更しました。",
            "OK");
    }
}
