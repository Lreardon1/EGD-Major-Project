using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMember : MonoBehaviour
{
    public enum PartyMemberAction { Attack, Block, Special };

    public PartyMemberAction nextAction;
    public string characterName;
    public int totalHitPoints;
    public int currentHitPoints;
    public int attack;
    public int speed;
    public float defenseMultiplier = 1f;
    public int temporaryHitPoints = 0;

    public TMPro.TextMeshPro text;

    public GameObject heldItem = null;
    public GameObject appliedCard = null;

    public GameObject target = null;

    private PartyMemberAction previousAction;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ExecuteAction()
    {
        if (nextAction == PartyMemberAction.Attack)
            Attack();
        else if (nextAction == PartyMemberAction.Block)
            Block();
        else if (nextAction == PartyMemberAction.Special)
            Special();
    }

    public virtual void TakeDamage(int damageAmount, string damageType)
    {
        Debug.Log("Took " + damageAmount + " of " + damageType + " type");
    }

    public virtual void SelectTarget(List<GameObject> enemies)
    {
        Debug.Log("Enemy Selected");
    }

    public virtual void Attack()
    {
        Debug.Log("Attack");
    }

    public virtual void Block()
    {
        Debug.Log("Block");
    }

    public virtual void Special()
    {
        Debug.Log("Special");
    }
}
