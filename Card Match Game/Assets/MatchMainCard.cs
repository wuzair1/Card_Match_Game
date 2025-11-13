using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MatchMainCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardMatchController controller;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private Animator cardAnimation;

    public Image img_Back;
    public bool selected;
    public AudioSource clickSound;

    private int _id;
    public int id => _id;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!cardBack.activeSelf || !controller.canReveal)
            return;

        clickSound?.Play();

        cardBack.transform
            .DORotate(new Vector3(0, 90, 0), 0.2f)
            .OnComplete(() =>
            {
                cardBack.SetActive(false);
                controller.CardRevealed(this);
            });
    }

    public void ChangeSprite(int id, Sprite image)
    {
        _id = id;
        img_Back.sprite = image;
    }

    public void Unreveal()
    {
        selected = false;
        cardBack.SetActive(true);
        cardBack.transform.DORotate(Vector3.zero, 0.2f);
    }
}
