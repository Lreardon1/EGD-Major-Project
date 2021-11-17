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
    }

    public void SetCard(RPS_Card card)
    {
        cardType = card.type;
        print("Play card set to " + card.type);
        sr.sprite = GetCardSprite(cardType);
    }

    public void SetRevealed(bool r)
    {
        revealed = r;
    }
}
