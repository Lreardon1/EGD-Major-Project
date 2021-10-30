using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCardAction : CardActionTemplate
{
    public override void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        base.OnPlay(c, combatant, otherCombatants);

        Card.Element type;
        int numModifier;
        Card.Element secondaryElement;
        Card.AoE aoe;
        bool givePriority;

    }
}
