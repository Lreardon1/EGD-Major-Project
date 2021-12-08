using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemBodyCombatController : CombatantBasis
{
    public float golemBuff = 0.3f;
    public override void Special()
    {
        MakePopup("Using Special Reinforce", null, Color.white);

        CombatManager cm = FindObjectOfType<CombatManager>();
        List<GameObject> adjacentArms = cm.GetAdjacentCombatants(this.gameObject);

        Buff b = this.gameObject.AddComponent<Buff>();
        b.affectedValues.Add(Buff.Stat.Attack);
        b.value = golemBuff;
        b.duration = 2;
        b.StartBuff();
        attachedBuffs.Add(b);

        Buff b2 = this.gameObject.AddComponent<Buff>();
        b2.affectedValues.Add(Buff.Stat.Defense);
        b2.value = golemBuff;
        b2.duration = 2;
        b2.StartBuff();
        attachedBuffs.Add(b2);

        foreach (GameObject arm in adjacentArms)
        {
            Buff b3 = arm.AddComponent<Buff>();
            b3.affectedValues.Add(Buff.Stat.Attack);
            b3.value = golemBuff;
            b3.duration = 2;
            b3.StartBuff();
            arm.GetComponent<CombatantBasis>().attachedBuffs.Add(b3);

            Buff b4 = arm.AddComponent<Buff>();
            b4.affectedValues.Add(Buff.Stat.Defense);
            b4.value = golemBuff;
            b4.duration = 2;
            b4.StartBuff();
            arm.GetComponent<CombatantBasis>().attachedBuffs.Add(b4);
        }
    }
}
