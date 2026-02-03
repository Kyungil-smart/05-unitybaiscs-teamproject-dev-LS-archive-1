using UnityEngine;
using UnityEngine.UI;
using OverTheSky.Core; // SceneController 사용

namespace OverTheSky.UI
{
    public class TitleUIManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        private void Start()
        {
            // 시작 버튼: SceneController를 통해 게임 씬 로드
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(() => 
                {
                    SceneController.Instance.LoadScene(Define.Scene.Game);
                });
            }

            // 종료 버튼: SceneController의 QuitGame 호출
            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(() => 
                {
                    SceneController.Instance.QuitGame();
                });
            }
        }
        
        // 오브젝트 파괴 시 리스너 정리
        private void OnDestroy()
        {
            if (_startButton != null) _startButton.onClick.RemoveAllListeners();
            if (_exitButton != null) _exitButton.onClick.RemoveAllListeners();
        }
    }
}