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

    public bool canDraw = false;
    public bool canPlay = true;
    private bool enoughMana = true;    

    
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
            CombatantBasis memberScript = member.GetComponent<CombatantBasis>();
            memberScript.SelectAction();
            memberScript.SelectTarget(enemies);

            if (memberScript.nextAction == CombatantBasis.Action.Attack)
            {
                memberScript.text.text = "Attack " + memberScript.target.name;
            }
            else if (memberScript.nextAction == CombatantBasis.Action.Block)
            {
                memberScript.text.text = "Block";
            }
            else if (memberScript.nextAction == CombatantBasis.Action.Special)
            {
                memberScript.text.text = "Special " + memberScript.target.name;
            }
        }

        foreach (GameObject enemy in enemies)
        {
            CombatantBasis enemyScript = enemy.GetComponent<CombatantBasis>();
            enemyScript.SelectAction(); 
            enemyScript.SelectTarget(partyMembers);
            if (enemyScript.nextAction ==  CombatantBasis.Action.Attack)
            {
                enemyScript.text.text = "Attack " + enemyScript.target.name;
            }
            else if (enemyScript.nextAction == CombatantBasis.Action.Block)
            {
                enemyScript.text.text = "Block";
            }
            else if (enemyScript.nextAction == CombatantBasis.Action.Special)
            {
                enemyScript.text.text = "Special " + enemyScript.target.name;
            }

        }

        CreateActionQueue();
        foreach(GameObject combatant in actionOrder)
        {
            Debug.Log(combatant.name);
        }

        ActivatePlayPhase();
        // Allies and enemies select actions to perform, Player selects number of cards to draw, transition to Play Phase
    }

    public void ActivatePlayPhase()
    {
        currentPhase = CombatPhase.PlayPhase;
        // Allow player to move cards to play on allies/enemies, update action order accordingly, ends when player clicks done or something, transition to Discard Phase
        ActivateDiscardPhase();
    }

    public void ActivateDiscardPhase()
    {
        currentPhase = CombatPhase.DiscardPhase;
        // Player can drag cards to discard pile to discard them, ends when player clicks done or something, transition to Action Phase
        ActivateActionPhase();
    }

    public void ActivateActionPhase()
    {
        currentPhase = CombatPhase.ActionPhase;
        // Party members and enemies take turns attacking in action order, death prevents attacking, transition to Draw Phase
        StartCoroutine("StartActions");
        
    }

    public IEnumerator StartActions()
    {
        for(int i = 0; i < actionOrder.Count; i++)
        {
            CombatantBasis cb = actionOrder[i].GetComponent<CombatantBasis>();
            bool cardAlreadyPlayed = false;

            if (cb.appliedCard != null)
                cardAlreadyPlayed = true;

            if (enoughMana && !cardAlreadyPlayed)
            {
                Debug.Log("Play card on " + actionOrder[i].name);

                bool done = false;
                while(!done)
                {
                    // skips when space is hit
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        done = true;
                    }

                    // Need code to detect if card has been applied

                    if (cb.appliedCard != null)
                        done = true;
                    yield return null;
                }
            }

            cb.ExecuteAction();
            // Check if any combatant was killed and update the action queue
        }

        currentPhase = CombatPhase.DrawPhase;
        ActivateDrawPhase();
        yield return null;
    }
   

    public void CheckEnoughMana()
    {
        // Check if player has enough mana to play any cards in their hand, if not the action order can procede without input
    }

    public void CreateActionQueue()
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
                CombatantBasis cb = combatant.GetComponent<CombatantBasis>();
                if(cb != null)
                {
                    if(cb.speed > maxSpeed)
                    {
                        maxSpeed = cb.speed;
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
