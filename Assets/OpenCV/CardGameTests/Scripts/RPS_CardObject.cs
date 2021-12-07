using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPS_CardObject : MonoBehaviour
{
    private RPS_Card.CardType cardType;

    public bool revealed = false;
    public Sprite[] cardSprites;
    public SpriteRenderer sr;
    private Quaternion initRot;
    public float revealFoldUpPivot;

    private void Start()
    {
        initRot = transform.rotation;

        //cardType = RPS_Card.CardType.Unknown;
        //sr.enabled = false;
    }

    public Sprite GetCardSprite(RPS_Card.CardType ct)
    {
        if (ct == RPS_Card.CardType.Unknown)
            return cardSprites[cardSprites.Length - 1];

        int c = (int)ct;
        return cardSprites[c];
    }


    public void SetEnabled(bool v)
    {
        gameObject.SetActive(v);
        transform.rotation = initRot;
    }

    public void SetCard(RPS_Card card)
    {
        cardType = card.type;
        print("Play card set to " + card.type);
        sr.enabled = (card.type != RPS_Card.CardType.Unknown);
        sr.sprite = GetCardSprite(cardType);
    }

    IEnumerator RevealBidCard(float time)
    {
        float t = 0; 
        while (t < time)
        {
            transform.Rotate(new Vector3(0, Time.deltaTime * 180.0f / time, 0), Space.Self);
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator RevealPlayCard(float time)
    {
        float t = 0;
        while (t < time)
        {
            transform.RotateAround(transform.position - (transform.up * revealFoldUpPivot), new Vector3(1, 0, 0), Time.deltaTime * 90.0f / time);
            t += Time.deltaTime;
            yield return null;
        }
    }

    public float AnimateBidReveal(bool r)
    {
        if (!r) return 0.0f;
        StartCoroutine(RevealBidCard(0.4f));
        return 0.4f;
    }

    public float AnimatePlayReveal(bool flipUp)
    {
        if (flipUp)
        {
            StartCoroutine(RevealPlayCard(0.4f));
            return 0.4f;
        } else
        {
            StartCoroutine(RevealBidCard(0.4f));
            return 0.4f;
        }
    }
}
