using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffCardAction : CardActionTemplate
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
        int baseNum = c.baseNum;
        int numModifier = c.numMod;
        bool givePriority = c.givePrio;

        CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
        //instantiate and apply Buff Component
        Buff b = combatant.AddComponent(typeof(Buff)) as Buff;
        b.affectedValues.AddRange(c.buffedStats);
        if (c.buffedStats.Count > 1)
        {
            b.value = .2f;
        }
        else
        {
            b.value = .5f;
        }
        if (cb.isEnemy)
        {
            b.value *= -1;
        }
        b.duration = baseNum + numModifier;
        b.StartBuff();
        cb.attachedBuffs.Add(b);

        appliedBuffs.Add(b);

        CombatManager cm = FindObjectOfType<CombatManager>();
        if (givePriority)
        {
            cm.GivePriority(combatant);
        }
        cm.UpdateActionQueue();
    }

    public override void UnapplyCard(Card c, GameObject combatant)
    {
        int baseNum = c.baseNum;
        int numModifier = c.numMod;
        bool givePriority = c.givePrio;

        //should always be true but just in case, removes one instance of buff applied by this card (not necessarily the exact one of the given combantant, but additional calls from RemoveCard should handle it
        if (appliedBuffs.Count > 0)
        {
            Buff tmp = appliedBuffs[0];
            appliedBuffs.RemoveAt(0);
            tmp.duration = 0;
            tmp.TickDuration();
        }

        CombatManager cm = FindObjectOfType<CombatManager>();
        if (givePriority)
        {
            cm.RemovePriority(combatant);
        }
        cm.UpdateActionQueue();
    }
}
