using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class CardMatchController : MonoBehaviour
{
    [Header("Grid Settings")]
    public int totalSize = 12;
    public int gridRows = 4;
    public int gridCols = 3;
    public float offsetX = 11f;
    public float offsetY = 10.27f;

    [Header("Cards & Images")]
    [SerializeField] private MatchMainCard[] card;
    [SerializeField] private Sprite[] images;
    private Sprite[] images6;

    [Header("Effects & UI")]
    public AudioSource matchSound;
    public AudioSource mismatchSound;
    public AudioSource bonusSound;
    public Text scoreLabel;

    [Header("Panels & Score UI")]
    public GameObject levelCompletePanel;
    public Text levelCompleteScoreText;
    public Text levelCompleteHighScoreText;
    public GameObject levelFailedPanel;
    public Text levelFailedScoreText;
    public Text levelFailedHighScoreText;

    [Header("Timer Settings")]
    public Text timerText;
    public float startTime = 60f;
    private float currentTime;
    private bool timerRunning = true;
    private Color normalColor = Color.green;
    private Color lowTimeColor = Color.red;
    private Coroutine flashRoutine;

    [Header("Bonus Settings")]
    public float bonusTimeWindow = 5f; // 5 seconds for bonus
    private float lastMatchTime = -10f;

    [Header("Bonus UI")]
    public Text bonusText; // Assign in Inspector

    private List<int> numberList = new List<int>();
    private List<int> selectedNumbers = new List<int>();
    private MatchMainCard _firstRevealed;
    private MatchMainCard _secondRevealed;
    private bool isChecking = false;

    [SerializeField] private int _score = 0;
    [SerializeField] private int MatchCard = 0;
    [Header("Preview Settings")]
    public float previewTime = 3f; // Time to show all cards at start
    private const string GameSceneName = "Menu"; // ✅ Change if needed
    public bool canReveal => _secondRevealed == null;

    private void Start()
    {
        InitializeCards();

        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (bonusText != null) bonusText.gameObject.SetActive(false);

        PreviewAllCards(); // Show all cards briefly
    }



    #region Timer
    private void StartTimer()
    {
        currentTime = startTime;
        timerRunning = true;
        UpdateTimerUI();
        StartCoroutine(TimerCountdown());
    }
    private void PreviewAllCards()
    {
        StartCoroutine(PreviewRoutine());
    }

    private IEnumerator PreviewRoutine()
    {
        // Show all cards (front side)
        foreach (var c in card)
        {
            c.cardBack.SetActive(false); // Show front
            //c.transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(previewTime);

        // Hide all cards again (flip back)
        foreach (var c in card)
        {
            c.cardBack.SetActive(true); // Show back
            c.transform.localRotation = Quaternion.identity;
        }

        // Start the game timer after preview
        StartTimer();
    }


    private IEnumerator TimerCountdown()
    {
        while (timerRunning)
        {
            yield return new WaitForSeconds(1f);
            currentTime -= 1f;

            if (currentTime <= 0)
            {
                currentTime = 0;
                timerRunning = false;
                ShowLevelFailed();
            }

            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = currentTime <= 20 ? lowTimeColor : normalColor;
    }

    private IEnumerator FlashRedTimer()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
        mismatchSound?.Play();
        yield return null;
    }

    private IEnumerator FlashRoutine()
    {
        timerText.color = lowTimeColor;
        yield return new WaitForSeconds(1f);
        UpdateTimerUI();
    }
    #endregion

    #region Card Initialization
    private void InitializeCards()
    {
        int[] numbers = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
        numbers = ShuffleArray(numbers);

        numberList.Clear();
        for (int i = 0; i < totalSize; i++) numberList.Add(i);

        images6 = new Sprite[6];
        GetRandomNumbers(6);

        for (int i = 0; i < totalSize && i < numbers.Length; i++)
        {
            int id = numbers[i];
            card[i].ChangeSprite(id, images6[id]);
        }
    }

    private void GetRandomNumbers(int count)
    {
        selectedNumbers.Clear();
        count = Mathf.Min(count, numberList.Count);

        while (selectedNumbers.Count < count)
        {
            int selectedNumber = numberList[Random.Range(0, numberList.Count)];
            if (!selectedNumbers.Contains(selectedNumber))
                selectedNumbers.Add(selectedNumber);
        }

        for (int i = 0; i < selectedNumbers.Count; i++)
            images6[i] = images[selectedNumbers[i]];
    }

    private int[] ShuffleArray(int[] numbers)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            int r = Random.Range(i, numbers.Length);
            (numbers[i], numbers[r]) = (numbers[r], numbers[i]);
        }
        return numbers;
    }
    #endregion

    #region Card Reveal & Match
    public void CardRevealed(MatchMainCard cardRevealed)
    {
        if (isChecking || cardRevealed.selected) return;

        if (_firstRevealed == null)
        {
            _firstRevealed = cardRevealed;
            _firstRevealed.selected = true; // Mark as revealed
        }
        else
        {
            _secondRevealed = cardRevealed;
            _secondRevealed.selected = true; // Mark as revealed
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        isChecking = true;

        if (_firstRevealed.id == _secondRevealed.id)
        {
            HandleTileMatch();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            HandleTileMismatch();
        }

        _firstRevealed = null;
        _secondRevealed = null;
        isChecking = false;
    }

    private void HandleTileMismatch()
    {
        Debug.Log("Tile Not Match");
        currentTime = Mathf.Max(0, currentTime - 5f);
        StartCoroutine(FlashRedTimer());

        _firstRevealed.selected = false;
        _secondRevealed.selected = false;

        _firstRevealed?.Unreveal();
        _secondRevealed?.Unreveal();
    }

    private void HandleTileMatch()
    {
        Debug.Log("Tile Match");
        MatchCard++;
        bool bonus = (Time.time - lastMatchTime) <= bonusTimeWindow;
        _score += 10;

        if (bonus)
        {
            _score += 20;
            bonusSound?.Play();
            ShowBonusText("+20 Bonus!");
        }

        lastMatchTime = Time.time;
        scoreLabel.text = "Score: " + _score;

        if (matchSound != null && PlayerPrefs.GetInt("Sound", 1) == 1)
            matchSound.Play();

        // Animate matched cards
        Sequence seq = DOTween.Sequence();
        seq.Append(_firstRevealed.transform.DOScale(Vector3.zero, 0.3f));
        seq.Join(_secondRevealed.transform.DOScale(Vector3.zero, 0.3f));
        seq.OnComplete(() =>
        {
            _firstRevealed.gameObject.SetActive(false);
            _secondRevealed.gameObject.SetActive(false);

            // Check if all cards selected = true → level complete
            CheckForLevelComplete();
        });

        if (MatchCard == 6)
        {
            StartCoroutine(CompleteGame());
        }
    }
    private void CheckForLevelComplete()
    {
        foreach (var c in card)
        {
            if (!c.selected) return; // If any card not selected → exit
        }

        // All cards selected → Level Complete
        timerRunning = false;
        StartCoroutine(CompleteGame());
    }
    #endregion

    #region Bonus Text Animation
    private void ShowBonusText(string text)
    {
        if (bonusText == null) return;

        bonusText.text = text;
        bonusText.gameObject.SetActive(true);
        bonusText.transform.localScale = Vector3.zero;
        bonusText.color = new Color(bonusText.color.r, bonusText.color.g, bonusText.color.b, 1f);

        Sequence seq = DOTween.Sequence();
        seq.Append(bonusText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        seq.Append(bonusText.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).SetDelay(0.5f));
        seq.Join(bonusText.DOFade(0f, 0.5f).SetDelay(0.5f));
        seq.OnComplete(() => bonusText.gameObject.SetActive(false));
    }
    #endregion

    #region Complete & Fail
    private IEnumerator CompleteGame()
    {
        SaveHighScore();
        yield return new WaitForSeconds(1f);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            levelCompleteScoreText.text = "Score: " + _score;
            levelCompleteHighScoreText.text = "High Score: " + GetHighScore();

            AnimatePanel(levelCompletePanel);
        }
    }

    private void ShowLevelFailed()
    {
        SaveHighScore();

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
            levelFailedScoreText.text = "Score: " + _score;
            levelFailedHighScoreText.text = "High Score: " + GetHighScore();

            AnimatePanel(levelFailedPanel);
        }
    }

    private void AnimatePanel(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        panel.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(panel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(1f, 0.5f));
    }

    private void SaveHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (_score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", _score);
            PlayerPrefs.Save();
        }
    }

    private int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }
    #endregion

    #region Menu & Retry
    public void RetryLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        LoadingManager.Instance.LoadScene(GameSceneName);
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
    #endregion
}