using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class MatchMainCard : MonoBehaviour
{
    [SerializeField] private CardMatchController controller;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private Animator cardAnimation;

    public Image img_Back;
    public bool selected;
    public AudioSource clickSound;

    private int _id;

    public int id => _id; // Property for id

    private void OnMouseDown()
    {
        // Only proceed if the card is active and can be revealed
        if (cardBack.activeSelf && controller.canReveal)
        {
            // Flip the card using DOTween
            cardBack.transform.DORotate(new Vector3(0, 90), 0.2f);
            controller.CardRevealed(this);
            clickSound.Play();
        }
    }

    // Method to change the sprite for the card
    public void ChangeSprite(int id, Sprite image)
    {
        _id = id;
        img_Back.sprite = image;

        //img_Back.SetNativeSize();
    }

    // Method to unreveal the card (flip back)
    public void Unreveal()
    {
        cardBack.transform.DORotate(Vector3.zero, 0.2f); // Flip back to original position
        cardBack.SetActive(true);
        selected = false;
    }
}
