using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhoulCombat : CombatantBasis
{
    public float ghoulDefenseBuff = 0.3f;

    public override void Special()
    {
        MakePopup("Using Special Boslter", null, Color.white);

        Buff b = gameObject.AddComponent<Buff>();
        b.affectedValues.Add(Buff.Stat.Defense);
        b.value = ghoulDefenseBuff;
        b.duration = 3;
        b.StartBuff();
        attachedBuffs.Add(b);
        Debug.Log(combatantName + " Special");
    }
}
