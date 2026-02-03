using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using OverTheSky.Core;

namespace OverTheSky.Systems
{
    // [AddComponentMenu("")] : 유니티 에디터 'Add Component' 메뉴에서 검색 안 되게 숨김
    [System.Obsolete("이 클래스는 더 이상 사용되지 않습니다. OverTheSky.Core.SceneController를 대신 사용하세요.")]
    [AddComponentMenu("")]
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void ConvertScene(string sceneName)
        {
            switch (sceneName)
            {
                case "GameStart":
                    SceneController.Instance.LoadScene(Define.Scene.Game);
                    break;
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}