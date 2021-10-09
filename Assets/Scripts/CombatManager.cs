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
                member.GetComponent<PartyMember>().nextAction = PartyMember.PartyMemberAction.Attack;
                member.GetComponent<PartyMember>().text.text = "Attack";
                member.GetComponent<PartyMember>().Attack();
            }
            else if (rand == 1)
            {
                member.GetComponent<PartyMember>().nextAction = PartyMember.PartyMemberAction.Block;
                member.GetComponent<PartyMember>().text.text = "Block";
                member.GetComponent<PartyMember>().Block();
            }
            else if (rand == 2)
            {
                member.GetComponent<PartyMember>().nextAction = PartyMember.PartyMemberAction.Special;
                member.GetComponent<PartyMember>().text.text = "Special";
                member.GetComponent<PartyMember>().Special();
            }
                
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
    }

    public void CheckWinCondition()
    {
        if (enemies.Count == 0)
            Debug.Log("You Win!");
        else if (partyMembers.Count == 0)
            Debug.Log("You Lose...");
    }
}
