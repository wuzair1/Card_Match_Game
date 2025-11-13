using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Text highScoreText;

    private const string HighScoreKey = "HighScore";
    private const string GameSceneName = "Card Match Module"; // ✅ Change if needed

    private void Start()
    {
        // Assign button listener
        startButton.onClick.AddListener(StartGame);

        // Display saved high score
        highScoreText.text = "High Score: " + PlayerPrefs.GetInt(HighScoreKey, 0);

        // Animate start button
        AnimateStartButton();
    }

    private void AnimateStartButton()
    {
        // Reset scale
        startButton.transform.localScale = Vector3.zero;

        // Scale in with bounce effect
        startButton.transform.DOScale(1f, 0.8f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Subtle pulse loop after appearing
                startButton.transform.DOScale(1.1f, 1.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });
    }

    private void StartGame()
    {
        // Button click feedback animation before scene load
        startButton.transform.DOKill(); // Stop pulse animation
        startButton.transform.DOScale(0.9f, 0.2f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(GameSceneName);
            });
    }
}
