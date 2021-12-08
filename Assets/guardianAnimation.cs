using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class guardianAnimation : MonoBehaviour
{
    public string state;
    public string direction = "left";
    SpriteRenderer guardianRenderer;
    public Animator guardian_animator;

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
            guardian_animator.Play("guardian_fistbump");
            guardian_animator.SetBool("Walking", false);
            guardian_animator.SetBool("Schlump", false);
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
        else if (animationNum == 1 && state == "schlump")
        {
            animationNum = 2;
        }
        else if (animationNum == 2 && state == "walking")
        {
            animationNum = 0;
        }
        if (animationNum == 0)
        {
            //idle
            state = "idle";
            guardian_animator.SetBool("Walking", false);
            guardian_animator.SetBool("Schlump", false);
        }
        if (animationNum == 1)
        {
            //sleep
            state = "schlump";
            guardian_animator.SetBool("Walking", false);
            guardian_animator.SetBool("Schlump", true);
        }
        if (animationNum == 2)
        {
            //walking
            state = "walking";
            guardian_animator.SetBool("Walking", true);
            guardian_animator.SetBool("Schlump", false);
        }
        if (directionNum == 0)
        {
            direction = "right";
            guardianRenderer.flipX = true;
        }
        else if (directionNum == 1)
        {
            direction = "left";
            guardianRenderer.flipX = false;
        }
        //Debug.Log(string.Format("Now facing {0} while {1}", direction, state));
        StartCoroutine(waiter());
    }

    // Start is called before the first frame update
    void Start()
    {
        guardianRenderer = GetComponent<SpriteRenderer>();
        guardianRenderer.flipX = false;
        state = "idle";
        StartCoroutine(waiter());
    }

    // Update is called once per frame
    void Update()
    {

    }
}

