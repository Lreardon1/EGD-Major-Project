using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanistCombat : CombatantBasis
{
    public int mechanistHealAmount = 5;

    public override void Special()
    {
        CombatManager cm = FindObjectOfType<CombatManager>();

        audioSource.PlayOneShot((AudioClip)Resources.Load("Sound/SFX/Attack_Heavy", typeof(AudioClip)), 0.7f);

        int randStatus = Random.Range(1, 15);
        Status statusToApply = (Status)randStatus;

        if (statusToApply == Status.Burn || statusToApply == Status.Wet || statusToApply == Status.Holy || statusToApply == Status.Fallen || statusToApply == Status.Molten ||
            statusToApply == Status.Vaporise || statusToApply == Status.Lightning || statusToApply == Status.Ice || statusToApply == Status.Blight || statusToApply == Status.Corruption)
        {
            foreach (GameObject activeEnemy in cm.activeEnemies)
            {
                activeEnemy.GetComponent<CombatantBasis>().statusScript.ApplyNewStatus(statusToApply, this.gameObject);
            }
        } else if(statusToApply == Status.Gust || statusToApply == Status.Earthbound)
        {
            foreach (GameObject activeMember in cm.activePartyMembers)
            {
                activeMember.GetComponent<CombatantBasis>().statusScript.ApplyBuff(statusToApply);
            }
        } else if(statusToApply == Status.Hellfire)
        {
            foreach (GameObject activeEnemy in cm.activeEnemies)
            {
                activeEnemy.GetComponent<CombatantBasis>().TakeStatusDamage((int)StatusScript.hellFireBurstDamage, statusToApply);
            }
        } else if(statusToApply == Status.HolyWater)
        {
            foreach (GameObject activeMember in cm.activePartyMembers)
            {
                activeMember.GetComponent<CombatantBasis>().Heal(StatusScript.holyWaterHealAmount);
            }
        }
       

        Debug.Log(combatantName + " Special");
    }
}
