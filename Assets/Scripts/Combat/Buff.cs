using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : MonoBehaviour
{
    public enum Stat { Attack, Defense, Resistance, Speed };

    public List<Stat> affectedValues = new List<Stat>();
    public float value;
    public int duration;

    public void StartBuff()
    {
        CombatantBasis cb = gameObject.GetComponent<CombatantBasis>();
        foreach (Stat stat in affectedValues)
        {
            switch (stat)
            {
                case Stat.Attack:
                    cb.attackMultiplier += value;
                    break;

                case Stat.Defense:
                    cb.defenseMultiplier += value;
                    break;

                case Stat.Resistance:
                    cb.resistance += value;
                    break;

                case Stat.Speed:
                    cb.speedMultiplier += value;
                    break;
            }
        }
    }

    public void TickDuration()
    {
        duration--;
        //if duration over, remove buffs
        if (duration == 0)
        {
            CombatantBasis cb = gameObject.GetComponent<CombatantBasis>();
            foreach (Stat stat in affectedValues)
            {
                switch (stat)
                {
                    case Stat.Attack:
                        cb.attackMultiplier -= value;
                        break;

                    case Stat.Defense:
                        cb.defenseMultiplier -= value;
                        break;

                    case Stat.Resistance:
                        cb.resistance -= value;
                        break;

                    case Stat.Speed:
                        cb.speedMultiplier -= value;
                        break;
                }
            }
            cb.attachedBuffs.Remove(this);
            Destroy(this);
        }
    }
}
