using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMember : MonoBehaviour
{
    public enum PartyMemberAction {Attack, Block, Special};

    public PartyMemberAction nextAction;
    public string characterName;
    public int hitPoints;
    public int attack;
    public int speed;

    public TMPro.TextMeshPro text;

    public GameObject heldItem = null;
    public GameObject appliedCard = null;

    public GameObject target = null;


    // Start is called before the first frame update
    void Start()
    {
        text.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
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
