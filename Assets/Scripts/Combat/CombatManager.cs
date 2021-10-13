using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public enum CombatPhase {DrawPhase, PlayPhase, DiscardPhase, ActionPhase};

    public CombatPhase currentPhase = CombatPhase.PlayPhase;

    public List<GameObject> partyMembers = new List<GameObject>();
    public List<GameObject> enemies = new List<GameObject>();

    public List<GameObject> actionOrder = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        // Populate Partymembers and enemies
        ActivateDrawPhase();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateDrawPhase()
    {
        foreach(GameObject member in partyMembers)
        {
            int rand = Random.Range(0, 3);
            if (rand == 0)
            {
                PartyMember memberScript = member.GetComponent<PartyMember>();
                memberScript.nextAction = PartyMember.PartyMemberAction.Attack;
                memberScript.target = enemies[Random.Range(0, enemies.Count)];
                memberScript.text.text = "Attack " + memberScript.target.name;
            }
            else if (rand == 1)
            {
                PartyMember memberScript = member.GetComponent<PartyMember>();
                memberScript.nextAction = PartyMember.PartyMemberAction.Block;
                memberScript.text.text = "Block";
            }
            else if (rand == 2)
            {
                PartyMember memberScript = member.GetComponent<PartyMember>();
                memberScript.nextAction = PartyMember.PartyMemberAction.Special;
                memberScript.target = enemies[Random.Range(0, enemies.Count)];
                memberScript.text.text = "Special " + memberScript.target.name;
            }
        }

        foreach (GameObject enemy in enemies)
        {
            int rand = Random.Range(0, 3);
            if (rand == 0)
            {
                EnemyBasis enemyScript = enemy.GetComponent<EnemyBasis>();
                enemyScript.nextAction = EnemyBasis.EnemyAction.Attack;
                enemyScript.target = partyMembers[Random.Range(0, partyMembers.Count)];
                enemyScript.text.text = "Attack " + enemyScript.target.name;
            }
            else if (rand == 1)
            {
                EnemyBasis enemyScript = enemy.GetComponent<EnemyBasis>();
                enemyScript.nextAction = EnemyBasis.EnemyAction.Block;
                enemyScript.text.text = "Block";
            }
            else if (rand == 2)
            {
                EnemyBasis enemyScript = enemy.GetComponent<EnemyBasis>();
                enemyScript.nextAction = EnemyBasis.EnemyAction.Special;
                enemyScript.target = partyMembers[Random.Range(0, partyMembers.Count)];
                enemyScript.text.text = "Special " + enemyScript.target.name;
            }

        }

        CreatActionQueue();
        foreach(GameObject combatant in actionOrder)
        {
            Debug.Log(combatant.name);
        }
        

        // Allies and enemies select actions to perform, Player selects number of cards to draw, transition to Play Phase
    }

    public void ActivatePlayPhase()
    {
        // Allow player to move cards to play on allies/enemies, update action order accordingly, ends when player clicks done or something, transition to Discard Phase
    }

    public void ActivateDiscardPhase()
    {
        // Player can drag cards to discard pile to discard them, ends when player clicks done or something, transition to Action Phase
    }

    public void ActivateActionPhase()
    {
        // Party members and enemies take turns attacking in action order, death prevents attacking, transition to Draw Phase


        CheckWinCondition();
    }

    public void CreatActionQueue()
    {
        actionOrder.Clear();
        List<GameObject> allCombatants = new List<GameObject>();
        foreach(GameObject member in partyMembers)
        {
            allCombatants.Add(member);
        }
        foreach (GameObject enemy in enemies)
        {
            allCombatants.Add(enemy);
        }

        while(allCombatants.Count != 0)
        {
            GameObject fastestCombatant = null;
            foreach(GameObject combatant in allCombatants)
            {
                int maxSpeed = -1;
                PartyMember pm = combatant.GetComponent<PartyMember>();
                EnemyBasis eb = combatant.GetComponent<EnemyBasis>();
                if(pm != null)
                {
                    if(pm.speed > maxSpeed)
                    {
                        maxSpeed = pm.speed;
                        fastestCombatant = combatant;
                    }
                } else if(eb != null)
                {
                    if (eb.speed > maxSpeed)
                    {
                        maxSpeed = eb.speed;
                        fastestCombatant = combatant;
                    }
                }
            }

            actionOrder.Add(fastestCombatant);
            allCombatants.Remove(fastestCombatant);
        }

    }

    public void CheckWinCondition()
    {
        if (enemies.Count == 0)
            Debug.Log("You Win!");
        else if (partyMembers.Count == 0)
            Debug.Log("You Lose...");
    }
}
