using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMovement : MonoBehaviour
{
    public GameObject ground;
    public int movementspeed = 100;
    string direction;
    SpriteRenderer characterRenderer;
    public Animator animator;
    public CharacterController cc;
    // Start is called before the first frame update
    void Start()
    {
        direction = "left";
        //Fetch the SpriteRenderer from the GameObject
        characterRenderer = GetComponent<SpriteRenderer>();
        characterRenderer.flipX = false;
        cc = GetComponent<CharacterController>();
    }

    public Vector3 velocity = Vector3.zero;
    public float turnSpeed = 30.0f;
    // Update is called once per frame
    void Update()
    {
        float rightTurn = Input.GetAxisRaw("Rotate");
        transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);

        Vector3 right = Input.GetAxisRaw("Horizontal") * transform.right;
        Vector3 up = Input.GetAxisRaw("Vertical") * transform.forward;

        // could just use gravity, character controller will handle not going thru the floor but this gives me more control
        bool isGround = Physics.Raycast(transform.position,
            (ground.transform.position - transform.position).normalized,
            (ground.transform.position - transform.position).magnitude, LayerMask.GetMask("Ground"));

        velocity += Vector3.down * 9.8f * Time.deltaTime;
        velocity = isGround ? Vector3.zero : velocity;


        cc.Move((((right + up).normalized * movementspeed) + velocity) * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
        {
            animator.SetBool("Walking", true);
            //transform.Translate(Vector3.right * movementspeed * Time.deltaTime);
            if (direction != "right" || direction != "forward")
            {
                characterRenderer.flipX = true;
                direction = "right";
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            animator.SetBool("Walking", true);
            //transform.Translate(Vector3.left * movementspeed * Time.deltaTime);
            if (direction != "left" || direction != "back")
            {
                characterRenderer.flipX = false;
                direction = "left";
            }
        }
        else if (Input.GetKey(KeyCode.W))
        {
            animator.SetBool("Walking", true);
            //transform.Translate(Vector3.forward * movementspeed * Time.deltaTime);
          
        }
        else if (Input.GetKey(KeyCode.S))
        {
            animator.SetBool("Walking", true);
            //transform.Translate(Vector3.back * movementspeed * Time.deltaTime);
            
        }
        else
        {
            animator.SetBool("Walking", false);
        }
        //Debug.Log(direction);
    }
}
