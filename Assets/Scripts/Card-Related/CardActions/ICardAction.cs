using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardAction
{
    void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants);
    void OnRemove(Card c, GameObject combatant, List<GameObject> otherCombatants);
    void ApplyCard(Card c, GameObject combatant);
    void UnapplyCard(Card c, GameObject combatant);
}
