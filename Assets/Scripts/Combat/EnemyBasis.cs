using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasis : MonoBehaviour
{
    public enum EnemyAction { Attack, Block, Special };

    public EnemyAction nextAction;
    public string enemyName;
    public int totalHitPoints;
    public int currentHitPoints;
    public int attack;
    public int speed;

    public TMPro.TextMeshPro text;
    
    public GameObject appliedCard = null;

    public GameObject target = null;

    public void ExecuteAction()
    {
        if (nextAction == EnemyAction.Attack)
            Attack();
        else if (nextAction == EnemyAction.Block)
            Block();
        else if (nextAction == EnemyAction.Special)
            Special();
    }

    public virtual void Attack()
    {
        Debug.Log("Enemy Attack");
    }

    public virtual void Block()
    {
        Debug.Log("Enemy Block");
    }

    public virtual void Special()
    {
        Debug.Log("Enemy Special");
    }
}
