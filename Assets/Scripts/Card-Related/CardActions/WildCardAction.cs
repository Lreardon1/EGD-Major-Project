using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildCardActiion : CardActionTemplate
{
    public override void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        base.OnPlay(c, combatant, otherCombatants);

        //implement Wild Card mirroring
    }

    public override void ApplyCard(Card c, GameObject combatant)
    {
        
    }
}
