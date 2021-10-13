using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMovement : MonoBehaviour
{
    public int movementspeed = 100;
    string direction;
    SpriteRenderer characterRenderer;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        direction = "left";
        //Fetch the SpriteRenderer from the GameObject
        characterRenderer = GetComponent<SpriteRenderer>();
        characterRenderer.flipX = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            animator.SetBool("Walking", true);
            transform.Translate(Vector3.right * movementspeed * Time.deltaTime);
            if (direction != "right" || direction != "forward")
            {
                characterRenderer.flipX = true;
                direction = "right";
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            animator.SetBool("Walking", true);
            transform.Translate(Vector3.left * movementspeed * Time.deltaTime);
            if (direction != "left" || direction != "back")
            {
                characterRenderer.flipX = false;
                direction = "left";
            }
        }
        else if (Input.GetKey(KeyCode.W))
        {
            animator.SetBool("Walking", true);
            transform.Translate(Vector3.forward * movementspeed * Time.deltaTime);
            if (direction != "right" || direction != "forward")
            {
                characterRenderer.flipX = true;
                direction = "forward";
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            animator.SetBool("Walking", true);
            transform.Translate(Vector3.back * movementspeed * Time.deltaTime);
            if (direction != "left" || direction != "back")
            {
                characterRenderer.flipX = false;
                direction = "back";
            }
        }
        else
        {
            animator.SetBool("Walking", false);
        }
        //Debug.Log(direction);
    }
}
