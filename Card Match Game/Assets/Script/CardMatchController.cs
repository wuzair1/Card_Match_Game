using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class CardMatchController : MonoBehaviour
{
    public int totalSize;
    public int gridRows = 4;
    public int gridCols = 3;
    public float offsetX = 11f;
    public float offsetY = 10.27f;

    private List<int> numberList = new List<int>();
    private List<int> selectedNumbers = new List<int>();
    [SerializeField] private MatchMainCard[] card;
    [SerializeField] private Sprite[] images;
    [SerializeField] private Sprite[] images6;
    public MatchMainCard dummyLeft;
    public MatchMainCard dummyRight;
    public Transform leftPos;
    public Transform rightPos;
    public float moveDuration = 1.5f;

    public GameObject collisionParticle;
    public GameObject blockPanel;
    public GameObject levelCompletePanel;
    private MatchMainCard _firstRevealed;
    private MatchMainCard _secondRevealed;
    public AudioSource matchSound;
    // Declare firstMov and secondMov
    private Vector3 firstMov = Vector3.zero;
    private Vector3 secondMov = Vector3.zero;
    public bool canReveal => _secondRevealed == null;
    private void InitializeCards()
    {
        int[] numbers = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
        numbers = ShuffleArray(numbers);
        numberList.AddRange(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });

        images6 = new Sprite[6];

       

       
    }
    private void Start()
    {
        InitializeCards();
        
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
    public void CardRevealed(MatchMainCard card)
    {
        if (_firstRevealed == null)
        {
            _firstRevealed = card;
            card.selected = true;
        }
        else if (!card.selected)
        {
            _secondRevealed = card;
            StartCoroutine(CheckMatch());
            card.selected = true;
        }
    }
    private IEnumerator CheckMatch()
    {
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

     
    }
    private void HandleTileMismatch()
    {
        print("Tile Not Match");
        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            //AudioManager.Instance.PlaySfx("Miss_Match_Tile");
        }
        _firstRevealed.Unreveal();
        _secondRevealed.Unreveal();
    }
    private void HandleTileMatch()
    {
        print("Tile Match");
      
   

   
    }
}
