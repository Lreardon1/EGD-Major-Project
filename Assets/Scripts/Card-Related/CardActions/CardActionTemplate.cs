using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionTemplate : MonoBehaviour, ICardAction
{
    public virtual void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        print("Template ON PLAY");
    }
}
