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
    [SerializeField] private Sprite[] images6; 

    [Header("Effects & UI")]
    public AudioSource matchSound;
    public AudioSource mismatchSound;
    public Text scoreLabel;
    public GameObject levelCompletePanel;

    [Header("Timer Settings")]
    public Text timerText;
    public float startTime = 60f;
    private float currentTime;
    private bool timerRunning = true;
    private Color normalColor = Color.green;
    private Color lowTimeColor = Color.red;
    private Coroutine flashRoutine;

    private List<int> numberList = new List<int>();
    private List<int> selectedNumbers = new List<int>();
    private MatchMainCard _firstRevealed;
    private MatchMainCard _secondRevealed;
    private bool isChecking = false;

    [SerializeField] private int _score = 0;
    public bool canReveal => _secondRevealed == null;

    private void Start()
    {
        InitializeCards();
        StartTimer();
    }

    #region Timer
    private void StartTimer()
    {
        currentTime = startTime;
        timerRunning = true;
        UpdateTimerUI();
        StartCoroutine(TimerCountdown());
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
                // Optional: End game when time runs out
                levelCompletePanel.SetActive(true);
            }

            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (currentTime <= 20)
            timerText.color = lowTimeColor;
        else
            timerText.color = normalColor;
    }


    private IEnumerator FlashRedTimer()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
        mismatchSound.Play();
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
        numberList.AddRange(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });

        images6 = new Sprite[6];
        GetRandomNumbers(6);

        for (int i = 0; i < totalSize && i < numbers.Length; i++)
        {
            int id = numbers[i];
            card[i].ChangeSprite(id, images6[id]);
        }
    }

    public void GetRandomNumbers(int count)
    {
        selectedNumbers.Clear();
        count = Mathf.Min(count, numberList.Count);

        while (selectedNumbers.Count < count)
        {
            int randomIndex = Random.Range(0, numberList.Count);
            int selectedNumber = numberList[randomIndex];

            if (!selectedNumbers.Contains(selectedNumber))
                selectedNumbers.Add(selectedNumber);
        }

        for (int i = 0; i < selectedNumbers.Count; i++)
        {
            images6[i] = images[selectedNumbers[i]];
        }
    }

    private int[] ShuffleArray(int[] numbers)
    {
        int[] newArray = numbers.Clone() as int[];
        for (int i = 0; i < newArray.Length; i++)
        {
            int r = Random.Range(i, newArray.Length);
            (newArray[i], newArray[r]) = (newArray[r], newArray[i]);
        }
        return newArray;
    }
    #endregion

    #region Card Reveal & Match
    public void CardRevealed(MatchMainCard cardRevealed)
    {
        if (isChecking || cardRevealed.selected)
            return;

        if (_firstRevealed == null)
        {
            _firstRevealed = cardRevealed;
            _firstRevealed.selected = true;
        }
        else
        {
            _secondRevealed = cardRevealed;
            _secondRevealed.selected = true;
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

        if (_score >= 60)
        {
            StartCoroutine(CompleteGame());
        }

        _firstRevealed = null;
        _secondRevealed = null;
        isChecking = false;
    }

    private void HandleTileMismatch()
    {
        Debug.Log("Tile Not Match");

        currentTime = Mathf.Max(0, currentTime - 5f); // Deduct 10 seconds
        StartCoroutine(FlashRedTimer());

        if (_firstRevealed != null) _firstRevealed.Unreveal();
        if (_secondRevealed != null) _secondRevealed.Unreveal();
    }

    private void HandleTileMatch()
    {
        Debug.Log("Tile Match");
        _score += 10; // Fixed increment
        currentTime = Mathf.Max(0, currentTime + 20f); // Deduct 10 seconds
        scoreLabel.text = "Score: " + _score;

        if (matchSound != null && PlayerPrefs.GetInt("Sound", 1) == 1)
        {
            matchSound.Play();
        }

        // Match animation (disappear)
        _firstRevealed.transform.DOScale(Vector3.zero, 0.3f);
        _secondRevealed.transform.DOScale(Vector3.zero, 0.3f);
    }
    #endregion

    private IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(1);
        levelCompletePanel.SetActive(true);
    }
}
