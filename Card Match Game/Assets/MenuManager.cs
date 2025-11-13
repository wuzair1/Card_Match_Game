using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button Quitbutton;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text MainMenuTitleText;

    private const string HighScoreKey = "HighScore";
    private const string GameSceneName = "Card Match Module"; // ✅ Change if needed

    private void Start()
    {
        // Assign button listener
        startButton.onClick.AddListener(StartGame);
        Quitbutton.onClick.AddListener(ExitGame);

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
            }); startButton.transform.localScale = Vector3.zero;

        // Scale in with bounce effect
        MainMenuTitleText.transform.DOScale(1f, 0.8f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Subtle pulse loop after appearing
                MainMenuTitleText.transform.DOScale(1.1626f, 1.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });
    }

    private void StartGame()
    {
        // Stop button pulse animation
        startButton.transform.DOKill();
        startButton.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // Use LoadingManager to load the scene with loading screen
                LoadingManager.Instance.LoadScene(GameSceneName);
            });
    }

    private void ExitGame()
    {
        Application.Quit(); // Quit the build/game
    }
}