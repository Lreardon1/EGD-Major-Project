using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanseCardAction : CardActionTemplate
{
    public override void OnPlay(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        base.OnPlay(c, combatant, otherCombatants);

        Card.AoE aoe = c.targetting;

        switch (aoe)
        {
            case Card.AoE.Single:
                ApplyCard(c, combatant);
                break;

            case Card.AoE.Adjascent:
                int pos = otherCombatants.IndexOf(combatant);
                if (pos < otherCombatants.Count - 1)
                {
                    ApplyCard(c, otherCombatants[pos + 1]);
                }
                ApplyCard(c, otherCombatants[pos]);
                if (pos > 0)
                {
                    ApplyCard(c, otherCombatants[pos - 1]);
                }
                break;

            case Card.AoE.All:
                for (int i = 0; i < otherCombatants.Count; i++)
                {
                    ApplyCard(c, otherCombatants[i]);
                }
                break;
        }
    }

    public override void ApplyCard(Card c, GameObject combatant)
    {
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        if (cb.isEnemy) //removes elements from action if enemy
        {
            cb.nextActionPrimaryElem = Card.Element.None;
            cb.nextActionSecondaryElem = Card.Element.None;
        }
        else //otherwise, cleanses status effects on party member
        {
            cb.statusCondition = CombatantBasis.Status.None;
        }

        if (givePriority)
        {
            CombatManager cm = FindObjectOfType<CombatManager>();
            cm.GivePriority(combatant);
        }
    }
}