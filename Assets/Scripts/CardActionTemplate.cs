using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionTemplate : MonoBehaviour, ICardAction
{
    public virtual void OnPlay()
    {
        print("Template ON PLAY");
    }
}
