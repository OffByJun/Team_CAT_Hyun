using _001_Scripts.Manager;
using TMPro;
using UnityEngine;

namespace _001_Scripts.UI.Component
{
    public class Goal : GameBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        public GameObject goalPanel;          // 골 도달 시 뜨는 패널
        public Button restartButton;          // 재시작 버튼

        void Awake()
        {
            restartButton.onClick.AddListener(RestartGame);
            goalPanel.SetActive(false);
        }

        public void UpdateInfo(bool isArrived)
        {
            text.text = isArrived ? "Goal!" : "";
            Time.timeScale = 0f;
            goalPanel.SetActive(true);
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;      // 반드시 복구 (안 하면 재시작 후에도 멈춰 있음)
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }

    }
}
