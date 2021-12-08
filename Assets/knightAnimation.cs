using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class knightAnimation : MonoBehaviour
{
    public string state;
    public string direction = "left";
    SpriteRenderer knightRenderer;
    public Animator knight_animator;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(15);
        NewState();
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
        else if (animationNum == 1 && state == "walk")
        {
            animationNum = 0;
        }
        if (animationNum == 0)
        {
            //idle
            state = "idle";
            knight_animator.SetBool("Walking", false);
        }
        if (animationNum == 1)
        {
            //walk
            state = "walk";
            knight_animator.SetBool("Walking", true);
        }
        if (directionNum == 0)
        {
            direction = "right";
            knightRenderer.flipX = true;
        }
        else if (directionNum == 1)
        {
            direction = "left";
            knightRenderer.flipX = false;
        }
        //Debug.Log(string.Format("Now facing {0} while {1}", direction, state));
        StartCoroutine(waiter());
    }

    // Start is called before the first frame update
    void Start()
    {
        knightRenderer = GetComponent<SpriteRenderer>();
        knightRenderer.flipX = false;
        state = "idle";
        StartCoroutine(waiter());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
