using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildCardAction : CardActionTemplate
{
    public override void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        base.OnPlay(c, combatant, otherCombatants);

        //implement Wild Card mirroring
        CombatManager cm = FindObjectOfType<CombatManager>();
        cm.ApplyCard(cm.lastPlayedCard, combatant);
    }

    public override void ApplyCard(Card c, GameObject combatant)
    {
        
    }
}
