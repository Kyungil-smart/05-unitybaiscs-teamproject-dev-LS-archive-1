using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using OverTheSky.Core;

namespace OverTheSky.Systems
{
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
                    SceneManager.Instance.LoadScene(SceneManager.GAME_SCENE);
                    break;
            }
        }

        public void QuitGame()
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }
}