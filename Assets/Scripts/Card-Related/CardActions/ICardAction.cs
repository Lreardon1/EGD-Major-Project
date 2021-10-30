using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardAction
{
    void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants);
}
