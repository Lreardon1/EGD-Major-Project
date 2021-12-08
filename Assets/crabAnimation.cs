using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crabAnimation : MonoBehaviour
{
    public string state;
    public string direction = "left";
    SpriteRenderer crabRenderer;
    public Animator crab_animator;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(10);
        NewState();
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            crab_animator.Play("crab_slash");
            crab_animator.SetBool("Slash", false);
        }
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
        else if (animationNum == 1 && state == "sleep")
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
            crab_animator.SetBool("Sleep", false);
            crab_animator.SetBool("Walk", false);
        }
        if (animationNum == 1)
        {
            //sleep
            state = "sleep";
            crab_animator.SetBool("Sleep", true);
            crab_animator.SetBool("Walk", false);
        }
        if (animationNum == 2)
        {
            //walk
            state = "walk";
            crab_animator.SetBool("Sleep", false);
            crab_animator.SetBool("Walk", true);
        }
        if (directionNum == 0)
        {
            direction = "right";
            crabRenderer.flipX = true;
        }
        else if (directionNum == 1)
        {
            direction = "left";
            crabRenderer.flipX = false;
        }
        //Debug.Log(string.Format("Now facing {0} while {1}", direction, state));
        StartCoroutine(waiter());
    }

    // Start is called before the first frame update
    void Start()
    {
        crabRenderer = GetComponent<SpriteRenderer>();
        crabRenderer.flipX = false;
        state = "idle";
        StartCoroutine(waiter());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
