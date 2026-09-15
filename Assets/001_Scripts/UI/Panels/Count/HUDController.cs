using UnityEngine;
using UnityEngine.UIElements;

namespace SSW
{
    public class HUDController : MonoBehaviour
    {
        private Label coinText;
        private Label livesText;

        private VisualElement resultPanel;
        private Label resultText;
        private Button restartButton;

        private VisualElement pausePanel;
        private Button volumeButton;
        private bool isMuted = false;

        void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            coinText = root.Q<Label>("coin-text");
            livesText = root.Q<Label>("lives-text");

            resultPanel = root.Q<VisualElement>("result-panel");
            resultText = root.Q<Label>("result-text");
            restartButton = root.Q<Button>("restart-button");
            restartButton.clicked += RestartGame;

            pausePanel = root.Q<VisualElement>("pause-panel");
            volumeButton = root.Q<Button>("volume-button");
            volumeButton.clicked += ToggleVolume;

            resultPanel.style.display = DisplayStyle.None;
            pausePanel.style.display = DisplayStyle.None;
        }

        // 다른 스크립트에서 일시정지 될 때 호출
        public void ShowPauseScreen()
        {
            pausePanel.style.display = DisplayStyle.Flex;
        }

        // 다른 스크립트에서 일시정지 풀릴 때 호출
        public void HidePauseScreen()
        {
            pausePanel.style.display = DisplayStyle.None;
        }

        private void ToggleVolume()
        {
            isMuted = !isMuted;
            AudioListener.volume = isMuted ? 0f : 1f;
            volumeButton.text = isMuted ? "Volume: OFF" : "Volume: ON";
        }

        public void ShowWinScreen()
        {
            resultText.text = "YOU WIN";
            restartButton.style.display = DisplayStyle.None;
            resultPanel.style.display = DisplayStyle.Flex;
        }

        public void ShowGameOverScreen(int remainingLives)
        {
            resultText.text = "GAME OVER";
            restartButton.style.display = remainingLives > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            resultPanel.style.display = DisplayStyle.Flex;
        }

        private void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}
