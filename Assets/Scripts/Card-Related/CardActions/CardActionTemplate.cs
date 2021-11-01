using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardActionTemplate : MonoBehaviour, ICardAction
{
    public virtual void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        
    }

    public virtual void ApplyCard(Card c, GameObject combatant)
    {

    }
}
