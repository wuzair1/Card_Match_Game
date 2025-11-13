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
    [SerializeField] private Sprite[] images;   // All available images
    [SerializeField] private Sprite[] images6;  // Selected for this level

    [Header("Effects & UI")]
    public AudioSource matchSound;
    public GameObject levelCompletePanel;
    [SerializeField] private Text scoreLabel;
    [SerializeField] private Text timerLabel;

    private List<int> numberList = new List<int>();
    private List<int> selectedNumbers = new List<int>();
    private MatchMainCard _firstRevealed;
    private MatchMainCard _secondRevealed;
    private bool isChecking = false;

    private int _score = 0;
    private float _timeRemaining = 60f; // Initial timer duration
    private bool _isTimerRunning = true;

    public bool canReveal => _secondRevealed == null;

    private void Start()
    {
        InitializeCards();
        UpdateTimerLabel();
    }

    private void Update()
    {
        if (!_isTimerRunning) return;

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0)
        {
            _timeRemaining = 0;
            _isTimerRunning = false;
            StartCoroutine(CompleteGame());
        }

        UpdateTimerLabel();
    }

    #region Card Initialization
    private void InitializeCards()
    {
        int[] numbers = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
        numbers = ShuffleArray(numbers);

        numberList.Clear();
        for (int i = 0; i < totalSize; i++)
            numberList.Add(i);

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
            int randomIndex = Random.Range(0, numberList.Count);
            int selectedNumber = numberList[randomIndex];
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
            StartCoroutine(CompleteGame());

        _firstRevealed = null;
        _secondRevealed = null;
        isChecking = false;
    }

    private void HandleTileMismatch()
    {
        Debug.Log("Tile Not Match");
        _timeRemaining -= 10f;
        if (_timeRemaining < 0) _timeRemaining = 0;

        UpdateTimerLabel();

        _firstRevealed?.Unreveal();
        _secondRevealed?.Unreveal();
    }

    private void HandleTileMatch()
    {
        Debug.Log("Tile Match");

        if (PlayerPrefs.GetInt("Sound", 1) == 1)
            matchSound?.Play();

        Transform card1 = _firstRevealed.transform;
        Transform card2 = _secondRevealed.transform;
        Vector3 scorePos = scoreLabel.transform.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(card1.DOMove(scorePos, 0.25f).SetEase(Ease.InOutQuad));
        seq.Join(card2.DOMove(scorePos, 0.25f).SetEase(Ease.InOutQuad));

        seq.Join(card1.DOScale(0.5f, 0.2f));
        seq.Join(card2.DOScale(0.5f, 0.2f));

        seq.AppendInterval(0.1f);

        seq.AppendCallback(() =>
        {
            card1.gameObject.SetActive(false);
            card2.gameObject.SetActive(false);

            _score += 10;
            scoreLabel.text = $"Score: {_score}";
        });
    }
    #endregion

    private void UpdateTimerLabel()
    {
        int minutes = Mathf.FloorToInt(_timeRemaining / 60);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60);
        timerLabel.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private IEnumerator CompleteGame()
    {
        yield return new WaitForSeconds(1f);
        levelCompletePanel.SetActive(true);
    }
}
