using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCardAction : CardActionTemplate
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

    public override void OnRemove(Card c, GameObject combatant, List<GameObject> otherCombatants)
    {
        base.OnRemove(c, combatant, otherCombatants);

        Card.AoE aoe = c.targetting;

        switch (aoe)
        {
            case Card.AoE.Single:
                UnapplyCard(c, combatant);
                break;

            case Card.AoE.Adjascent:
                int pos = otherCombatants.IndexOf(combatant);
                if (pos < otherCombatants.Count - 1)
                {
                    UnapplyCard(c, otherCombatants[pos + 1]);
                }
                UnapplyCard(c, otherCombatants[pos]);
                if (pos > 0)
                {
                    UnapplyCard(c, otherCombatants[pos - 1]);
                }
                break;

            case Card.AoE.All:
                for (int i = 0; i < otherCombatants.Count; i++)
                {
                    UnapplyCard(c, otherCombatants[i]);
                }
                break;
        }
    }

    public override void ApplyCard(Card c, GameObject combatant)
    {
        int baseNum = c.baseNum;
        int numModifier = c.numMod;
        Card.Element secondaryElement = c.secondaryElem;
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        if (c.shieldWithThorns)
        {
            cb.temporaryHitPoints += baseNum;
            cb.shieldReturnDmg += numModifier;
        }
        else
        {
            cb.temporaryHitPoints += baseNum + numModifier;
        }
        if (secondaryElement != Card.Element.None)
        {
            cb.shieldResistance += .2f;
        }

        if (givePriority)
        {
            CombatManager cm = FindObjectOfType<CombatManager>();
            cm.GivePriority(combatant);
        }
    }

    public override void UnapplyCard(Card c, GameObject combatant)
    {
        int baseNum = c.baseNum;
        int numModifier = c.numMod;
        Card.Element secondaryElement = c.secondaryElem;
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        if (c.shieldWithThorns)
        {
            cb.temporaryHitPoints -= baseNum;
            cb.shieldReturnDmg -= numModifier;
        }
        else
        {
            cb.temporaryHitPoints -= baseNum + numModifier;
        }
        if (secondaryElement != Card.Element.None)
        {
            cb.shieldResistance -= .2f;
        }

        if (givePriority)
        {
            CombatManager cm = FindObjectOfType<CombatManager>();
            cm.RemovePriority(combatant);
        }
    }
}
