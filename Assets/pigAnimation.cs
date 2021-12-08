using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pigAnimation : MonoBehaviour
{
    public string state;
    public string direction = "left";
    SpriteRenderer pigRenderer;
    public Animator pig_animator;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(25);
        NewState();
    }

    void NewState()
    {
        //every minute pick a new animation to do from idle, sleep, and walking
        //additionally pick which direction to face from left and right
        int directionNum = UnityEngine.Random.Range(0, 2);
        int animationNum = UnityEngine.Random.Range(0, 3);
        if (animationNum == 0)
        {
            //idle
            state = "idle";
            pig_animator.SetBool("Walk", false);
            pig_animator.SetBool("Sleep", false);
        }
        if (animationNum == 1)
        {
            //sleep
            state = "sleeping";
            pig_animator.SetBool("Walk", false);
            pig_animator.SetBool("Sleep", true);
        }
        if (animationNum == 2)
        {
            //walking
            state = "walking";
            pig_animator.SetBool("Walk", true);
            pig_animator.SetBool("Sleep", false);
        }
        if (directionNum == 0)
        {
            direction = "right";
            pigRenderer.flipX = true;
        }
        else if (directionNum == 1)
        {
            direction = "left";
            pigRenderer.flipX = false;
        }
        Debug.Log(string.Format("Now facing {0} while {1}", direction, state));
        StartCoroutine(waiter());
    }

    // Start is called before the first frame update
    void Start()
    {
        pigRenderer = GetComponent<SpriteRenderer>();
        pigRenderer.flipX = false;
        state = "idle";
        StartCoroutine(waiter());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
