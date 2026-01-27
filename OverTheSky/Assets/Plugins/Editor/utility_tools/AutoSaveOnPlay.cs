using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoSaveOnPlay
{
    static AutoSaveOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 플레이 모드로 들어가기 직전
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            bool saved = EditorSceneManager.SaveOpenScenes();

            if (saved)
            { 
                Debug.Log("[AutoSave] Play 실행 전 씬 자동 저장 완료");
            }
            else
            {
                Debug.LogWarning("[AutoSave] 저장할 씬이 없거나 저장 실패");
            }
        }
    }
}
