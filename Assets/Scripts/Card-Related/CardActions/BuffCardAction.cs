using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffCardAction : CardActionTemplate
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

        CombatManager cm = FindObjectOfType<CombatManager>();
        if (givePriority)
        {
            cm.GivePriority(combatant);
        }
        cm.UpdateActionQueue();
    }
}
