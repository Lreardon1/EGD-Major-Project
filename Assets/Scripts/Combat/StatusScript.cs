using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusScript : MonoBehaviour
{
    public Buff statusBuff = null;
    public GameObject combatant;
    public CombatantBasis cb;

    public static int burnDamage = 3;
    public static int fallenBonusDamage = 5;
    public static int holyHealAmount = 5;
    public static int hellFireBurstDamage = 3;
    public static int holyWaterHealAmount = 3;

    public static float wetSpeedReduction = -0.2f;
    public static float earthBoundDefenseIncrease = 0.1f;
    public static float gustResistanceIncrease = 0.1f;
    public static float moltenDefenseReduction = -0.2f;
    public static float corruptionAttackReduction = -0.2f;

    public static float lightiningDamagePercent = 0.2f;
    

    public void OnTakeDamageStatusHandler(CombatantBasis.Status status, GameObject attacker, int damageAmount)
    {
        switch(status)
        {
            case CombatantBasis.Status.Holy:
                attacker.GetComponent<CombatantBasis>().Heal(holyHealAmount);
                break;
            case CombatantBasis.Status.Fallen:
                cb.TakeStatusDamage(fallenBonusDamage, status);
                break;
            case CombatantBasis.Status.Lightning:
                CombatManager cm = FindObjectOfType<CombatManager>();
                foreach(GameObject adjacent in cm.GetAdjacentCombatants(this.gameObject))
                {
                    adjacent.GetComponent<CombatantBasis>().TakeStatusDamage((int)(damageAmount * lightiningDamagePercent), status);
                }
                break;
        }
    }

    public void OnStatusApply(CombatantBasis.Status status, GameObject attacker)
    {
        switch(status)
        {
            case CombatantBasis.Status.Hellfire:
                {
                    CombatManager cm = FindObjectOfType<CombatManager>();
                    cb.TakeStatusDamage(hellFireBurstDamage, status);
                    foreach (GameObject adjacent in cm.GetAdjacentCombatants(this.gameObject))
                    {
                        adjacent.GetComponent<CombatantBasis>().TakeStatusDamage(hellFireBurstDamage, status);
                    }
                    break;
                }
            case CombatantBasis.Status.HolyWater:
                {
                    CombatManager cm = FindObjectOfType<CombatManager>();
                    cb.Heal(holyWaterHealAmount);
                    foreach (GameObject adjacent in cm.GetAdjacentCombatants(attacker))
                    {
                        adjacent.GetComponent<CombatantBasis>().Heal(holyWaterHealAmount);
                    }
                    break;
                }
            case CombatantBasis.Status.Blight:
                cb.SelectAction();
                break;
        }
    }

    public void ApplyNewStatus(CombatantBasis.Status status, GameObject attacker)
    {
        float rand = Random.Range(0f, 1f);
        if(rand < cb.resistance + cb.shieldResistance)
        {
            return;
        }

        CombatManager cm = FindObjectOfType<CombatManager>();
        switch (status)
        {
            case CombatantBasis.Status.Burn:
                cb.statusCondition = status;
                break;
            case CombatantBasis.Status.Wet:
                cb.statusCondition = status;
                ApplyBuff(status);
                break;
            case CombatantBasis.Status.Earthbound:
                if(attacker.GetComponent<CombatantBasis>().isEnemy)
                {
                    foreach(GameObject enemy in cm.activeEnemies)
                    {
                        enemy.GetComponent<CombatantBasis>().statusCondition = status;
                        enemy.GetComponent<StatusScript>().ApplyBuff(status);
                    }
                } else
                {
                    foreach (GameObject partyMember in cm.activePartyMembers)
                    {
                        partyMember.GetComponent<CombatantBasis>().statusCondition = status;
                        partyMember.GetComponent<StatusScript>().ApplyBuff(status);
                    }
                }
                break;
            case CombatantBasis.Status.Gust:
                if (attacker.GetComponent<CombatantBasis>().isEnemy)
                {
                    foreach (GameObject enemy in cm.activeEnemies)
                    {
                        enemy.GetComponent<CombatantBasis>().statusCondition = status;
                        enemy.GetComponent<StatusScript>().ApplyBuff(status);
                    }
                }
                else
                {
                    foreach (GameObject partyMember in cm.activePartyMembers)
                    {
                        partyMember.GetComponent<CombatantBasis>().statusCondition = status;
                        partyMember.GetComponent<StatusScript>().ApplyBuff(status);
                    }
                }
                break;
            case CombatantBasis.Status.Holy:
                cb.statusCondition = status;
                break;
            case CombatantBasis.Status.Fallen:
                cb.statusCondition = status;
                break;
            case CombatantBasis.Status.Molten:
                cb.statusCondition = status;
                ApplyBuff(status);
                break;
            case CombatantBasis.Status.Vaporise:
                cb.statusCondition = status;
                break;
            case CombatantBasis.Status.Lightning:
                cb.statusCondition = status;
                break;
            case CombatantBasis.Status.Hellfire:
                OnStatusApply(status, attacker);
                break;
            case CombatantBasis.Status.Ice:
                cb.statusCondition = status;
                break;
            case CombatantBasis.Status.HolyWater:
                OnStatusApply(status, attacker);
                break;
            case CombatantBasis.Status.Blight:
                cb.statusCondition = status;
                OnStatusApply(status, attacker);
                break;
            case CombatantBasis.Status.Corruption:
                cb.Heal(holyWaterHealAmount);
                ApplyBuff(status);
                foreach (GameObject adjacent in cm.GetAdjacentCombatants(this.gameObject))
                {
                    adjacent.GetComponent<CombatantBasis>().statusCondition = status;
                    adjacent.GetComponent<StatusScript>().ApplyBuff(status);
                }
                break;
        }
    }

    public void OnAttackStatusHandler(CombatantBasis.Status status)
    {

    }

    public void ApplyBuff(CombatantBasis.Status status)
    {
        if(statusBuff != null)
        {
            statusBuff.duration = 1;
            statusBuff.TickDuration();
            statusBuff = null;
        }
        switch(status)
        {
            case CombatantBasis.Status.Wet:
                {
                    Buff b = combatant.AddComponent<Buff>();
                    b.affectedValues.Add(Buff.Stat.Speed);
                    b.value = wetSpeedReduction;
                    b.duration = 5;
                    b.StartBuff();
                    cb.attachedBuffs.Add(b);
                    break;
                }
            case CombatantBasis.Status.Earthbound:
                {
                    Buff b = combatant.AddComponent<Buff>();
                    b.affectedValues.Add(Buff.Stat.Defense);
                    b.value = earthBoundDefenseIncrease;
                    b.duration = 5;
                    b.StartBuff();
                    cb.attachedBuffs.Add(b);
                    break;
                }
            case CombatantBasis.Status.Gust:
                {
                    Buff b = combatant.AddComponent<Buff>();
                    b.affectedValues.Add(Buff.Stat.Resistance);
                    b.value = gustResistanceIncrease;
                    b.duration = 5;
                    b.StartBuff();
                    cb.attachedBuffs.Add(b);
                    break;
                }
            case CombatantBasis.Status.Molten:
                {
                    Buff b = combatant.AddComponent<Buff>();
                    b.affectedValues.Add(Buff.Stat.Defense);
                    b.value = moltenDefenseReduction;
                    b.duration = 5;
                    b.StartBuff();
                    cb.attachedBuffs.Add(b);
                    break;
                }
            case CombatantBasis.Status.Corruption:
                {
                    Buff b = combatant.AddComponent<Buff>();
                    b.affectedValues.Add(Buff.Stat.Attack);
                    b.value = corruptionAttackReduction;
                    b.duration = 5;
                    b.StartBuff();
                    cb.attachedBuffs.Add(b);
                    break;
                }
        }
    }

    public CombatantBasis.Status GetStatusResult(Card.Element primaryType, Card.Element secondaryType)
    {
        if (primaryType == Card.Element.None && secondaryType == Card.Element.None)
        {
            return CombatantBasis.Status.None;
        }

        switch (primaryType)
        {
            case Card.Element.Fire:
                switch(secondaryType)
                {
                    case Card.Element.Earth:
                        return CombatantBasis.Status.Molten;
                    case Card.Element.Water:
                        return CombatantBasis.Status.Vaporise;
                    case Card.Element.Air:
                        return CombatantBasis.Status.Lightning;
                    case Card.Element.Dark:
                        return CombatantBasis.Status.Hellfire;
                    default:
                        return CombatantBasis.Status.Burn;
                }
            case Card.Element.Water:
                switch (secondaryType)
                {
                    case Card.Element.Light:
                        return CombatantBasis.Status.HolyWater;
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Vaporise;
                    case Card.Element.Air:
                        return CombatantBasis.Status.Ice;
                    default:
                        return CombatantBasis.Status.Wet;
                }
            case Card.Element.Air:
                switch (secondaryType)
                {
                    case Card.Element.Water:
                        return CombatantBasis.Status.Ice;
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Lightning;
                    default:
                        return CombatantBasis.Status.Gust;
                }
            case Card.Element.Earth:
                switch (secondaryType)
                {
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Molten;
                    case Card.Element.Dark:
                        return CombatantBasis.Status.Corruption;
                    default:
                        return CombatantBasis.Status.Earthbound;
                }
            case Card.Element.Light:
                switch (secondaryType)
                {
                    case Card.Element.Water:
                        return CombatantBasis.Status.HolyWater;
                    case Card.Element.Dark:
                        return CombatantBasis.Status.Blight;
                    default:
                        return CombatantBasis.Status.Holy;
                }
            case Card.Element.Dark:
                switch (secondaryType)
                {
                    case Card.Element.Earth:
                        return CombatantBasis.Status.Corruption;
                    case Card.Element.Light:
                        return CombatantBasis.Status.Blight;
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Hellfire;
                    default:
                        return CombatantBasis.Status.Fallen;
                }
        }

        return CombatantBasis.Status.None;
    }
}
