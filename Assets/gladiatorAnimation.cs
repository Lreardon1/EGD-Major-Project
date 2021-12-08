using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gladiatorAnimation : MonoBehaviour
{
    public string state;
    public string direction = "left";
    SpriteRenderer gladiatorRenderer;
    public Animator gladiator_animator;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(25);
        NewState();
    }

    void NewState()
    {
        //every 10 sec pick a new animation to do from idle, schlump, and walking
        //additionally pick which direction to face from left and right
        int directionNum = UnityEngine.Random.Range(0, 2);
        int animationNum = UnityEngine.Random.Range(0, 3);
        if (animationNum == 0 && state == "idle")
        {
            animationNum = 1;
        }
        else if (animationNum == 1 && state == "pushup")
        {
            animationNum = 2;
        }
        else if (animationNum == 2 && state == "walk")
        {
            animationNum = 0;
        }
        if (animationNum == 0)
        {
            //idle
            state = "idle";
            gladiator_animator.SetBool("Pushup", false);
            gladiator_animator.SetBool("Walking", false);
        }
        if (animationNum == 1)
        {
            //pushup
            state = "pushup";
            gladiator_animator.SetBool("Pushup", true);
            gladiator_animator.SetBool("Walking", false);
        }
        if (animationNum == 2)
        {
            //walk
            state = "walk";
            gladiator_animator.SetBool("Pushup", false);
            gladiator_animator.SetBool("Walking", true);
        }
        if (directionNum == 0)
        {
            direction = "right";
            gladiatorRenderer.flipX = true;
        }
        else if (directionNum == 1)
        {
            direction = "left";
            gladiatorRenderer.flipX = false;
        }
        //Debug.Log(string.Format("Now facing {0} while {1}", direction, state));
        StartCoroutine(waiter());
    }

    // Start is called before the first frame update
    void Start()
    {
        gladiatorRenderer = GetComponent<SpriteRenderer>();
        gladiatorRenderer.flipX = false;
        state = "idle";
        StartCoroutine(waiter());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
