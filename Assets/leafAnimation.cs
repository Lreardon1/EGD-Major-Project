using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leafAnimation : MonoBehaviour
{
    public string state;
    public string direction = "left";
    SpriteRenderer leafRenderer;
    public Animator leaf_animator;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(20);
        NewState();
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            leaf_animator.Play("leaf_suprise");
            //leaf_animator.SetBool("Poof", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            leaf_animator.Play("leaf_sadge");
            //leaf_animator.SetBool("Poof", false);
        }
    }

    void NewState()
    {
        //every 10 sec pick a new animation to do from idle, schlump, and walking
        //additionally pick which direction to face from left and right
        int directionNum = UnityEngine.Random.Range(0, 2);
        int animationNum = UnityEngine.Random.Range(0, 2);
        if (animationNum == 0 && state == "idle")
        {
            animationNum = 1;
        }
        else if (animationNum == 1 && state == "poof")
        {
            animationNum = 0;
        }
        if (animationNum == 0)
        {
            //idle
            state = "idle";
            leaf_animator.SetBool("Poof", false);
        }
        if (animationNum == 1)
        {
            //poof
            state = "poof";
            leaf_animator.SetBool("Poof", true);
        }
        if (directionNum == 0)
        {
            direction = "right";
            leafRenderer.flipX = true;
        }
        else if (directionNum == 1)
        {
            direction = "left";
            leafRenderer.flipX = false;
        }
        //Debug.Log(string.Format("Now facing {0} while {1}", direction, state));
        StartCoroutine(waiter());
    }

    // Start is called before the first frame update
    void Start()
    {
        leafRenderer = GetComponent<SpriteRenderer>();
        leafRenderer.flipX = false;
        state = "idle";
        StartCoroutine(waiter());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
