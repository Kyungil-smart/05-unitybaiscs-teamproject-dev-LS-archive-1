using System.Collections;
using System.Collections.Generic;
using OverTheSky.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OverTheSky.Core
{
    public class SceneManager : Singleton<SceneManager>
    {
        public const string TITLE_SCENE = "TitleScene";
        public const string GAME_SCENE = "KDT_TESTScene";

        protected override void Awake()
        {
            base.Awake();
        }
        
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }


        public string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }
}
