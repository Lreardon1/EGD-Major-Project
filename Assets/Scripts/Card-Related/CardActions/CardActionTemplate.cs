using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionTemplate : MonoBehaviour, ICardAction
{
    public virtual void OnPlay(Card c)
    {
        print("Template ON PLAY");
    }
}
