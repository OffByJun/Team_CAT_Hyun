using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUI : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject goalPanel;          // 골 도달 시 뜨는 패널
    public TextMeshProUGUI goalText;      // "GOAL!" 텍스트
    public Button restartButton;          // 재시작 버튼

    private bool isGoalReached = false;

    void Awake()
    {
        restartButton.onClick.AddListener(RestartGame);
        goalPanel.SetActive(false);
    }

    // 이 스크립트를 골 오브젝트(트리거 콜라이더)에 붙이면 자동 감지
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGoalReached) return;
        if (!other.CompareTag("Player")) return;

        ReachGoal();
    }

    private void ReachGoal()
    {
        isGoalReached = true;
        Time.timeScale = 0f;      // 화면 멈춤
        goalPanel.SetActive(true);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;      // 반드시 복구 (안 하면 재시작 후에도 멈춰 있음)
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
    }
}