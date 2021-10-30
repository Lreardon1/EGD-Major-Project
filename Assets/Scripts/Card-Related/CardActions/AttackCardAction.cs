using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCardAction : CardActionTemplate
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
                if (pos < otherCombatants.Count-1)
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

    private void ApplyCard(Card c, GameObject combatant)
    {
        int baseNum = c.baseNum;
        Card.Element type = c.element;
        int numModifier = c.numMod;
        Card.Element secondaryElement = c.secondaryElem;
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        cb.attackCardBonus = baseNum + numModifier;
        cb.nextActionPrimaryElem = type;
        cb.nextActionSecondaryElem = secondaryElement;
        CombatManager cm = FindObjectOfType<CombatManager>();
        if (givePriority)
        {
            cm.GivePriority(combatant);
        }
    }
}
