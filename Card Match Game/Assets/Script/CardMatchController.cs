using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class CardMatchController : MonoBehaviour
{
    private List<int> numberList = new List<int>();
    private List<int> selectedNumbers = new List<int>();
    
    [SerializeField] private Sprite[] images;
    [SerializeField] private Sprite[] images6;
    private void InitializeCards()
    {
        int[] numbers = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
        numbers = ShuffleArray(numbers);
        numberList.AddRange(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });

        images6 = new Sprite[6];

       

       
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
}
