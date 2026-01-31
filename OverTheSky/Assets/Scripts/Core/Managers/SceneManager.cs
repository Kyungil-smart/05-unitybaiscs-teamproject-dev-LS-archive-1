using UnityEngine;
using UnityEngine.SceneManagement;

namespace OverTheSky.Core
{
    public class SceneController : Singleton<SceneController>
    {
        protected override void Awake()
        {
            base.Awake();
        }
        
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }


        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
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
