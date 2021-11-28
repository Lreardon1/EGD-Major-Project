using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelCardAction : CardActionTemplate
{
    List<Buff> appliedBuffs = new List<Buff>();

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
        Card.Element secondaryElement = c.secondaryElem;
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        cb.nextActionSecondaryElems.Add(secondaryElement);

        //handle hard coding force attack + damage multiplier
        cb.isChanneling = true;
        //instantiate and apply Buff Component
        Buff b = combatant.AddComponent(typeof(Buff)) as Buff;
        b.affectedValues.Add(Buff.Stat.Attack);
        b.value = 1.5f;
        b.duration = 2;
        b.StartBuff();
        cb.attachedBuffs.Add(b);
        appliedBuffs.Add(b);

        if (givePriority)
        {
            CombatManager cm = FindObjectOfType<CombatManager>();
            cm.GivePriority(combatant);
        }
    }

    public override void UnapplyCard(Card c, GameObject combatant)
    {
        Card.Element secondaryElement = c.secondaryElem;
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        cb.RemoveElements(Card.Element.None, secondaryElement);

        cb.isChanneling = false;
        //should always be true but just in case
        if (appliedBuffs.Count > 0)
        {
            Buff tmp = appliedBuffs[0];
            appliedBuffs.RemoveAt(0);
            tmp.duration = 0;
            tmp.TickDuration();
        }

        if (givePriority)
        {
            CombatManager cm = FindObjectOfType<CombatManager>();
            cm.RemovePriority(combatant);
        }
    }
}
