using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMovement : MonoBehaviour
{
    public GameObject ground;
    public int movementspeed = 100;
    string directionY;
    string directionX;
    SpriteRenderer playerRenderer;
    SpriteRenderer godfatherRenderer;
    SpriteRenderer warriorRenderer;
    SpriteRenderer hunterRenderer;
    SpriteRenderer mechanistRenderer;
    public Animator player_animator;
    public Animator warrior_animator;
    public Animator godfather_animator;
    public Animator hunter_animator;
    public Animator mechanist_animator;
    public CharacterController cc;
    public bool canMove = true;
    private LinkedList<TimePairTransform> walkLine = new LinkedList<TimePairTransform>();
    private float walkTime = 0;

    public GameObject[] party_members;

    public class TimePairTransform
    {
        public float time;
        public Vector3 pos;
        public TimePairTransform(float time, Vector3 t)
        {
            this.time = time;
            pos = t;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
        directionY = "left";
        directionX = "forward";
        //Fetch the SpriteRenderer from the GameObject and other party gameobjects
        playerRenderer = GetComponent<SpriteRenderer>();
        playerRenderer.flipX = false;
        godfatherRenderer = party_members[0].GetComponent<SpriteRenderer>();
        godfatherRenderer.flipX = false;
        hunterRenderer = party_members[1].GetComponent<SpriteRenderer>();
        hunterRenderer.flipX = false;
        warriorRenderer = party_members[2].GetComponent<SpriteRenderer>();
        warriorRenderer.flipX = false;
        mechanistRenderer = party_members[3].GetComponent<SpriteRenderer>();
        mechanistRenderer.flipX = false;
        cc = GetComponent<CharacterController>();

        // init movement conga line
        for (float t = walkTime - 2; t < walkTime; t += 0.1f)
        {
            walkLine.AddFirst(new TimePairTransform(t, transform.position));
        }
    }

    public Vector3 velocity = Vector3.zero;
    public float turnSpeed = 30.0f;
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 right = new Vector3(0, 0, 0);
        Vector3 up = new Vector3(0,0,0);

        if (canMove)
        {
            float rightTurn = Input.GetAxisRaw("Rotate");
            transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);
            party_members[0].transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);
            party_members[1].transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);
            party_members[2].transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);
            party_members[3].transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);

            right = Input.GetAxisRaw("Horizontal") * transform.right;
            up = Input.GetAxisRaw("Vertical") * transform.forward;
        }

        // could just use gravity, character controller will handle not going thru the floor but this gives me more control
        bool isGround = Physics.Raycast(transform.position,
            (ground.transform.position - transform.position).normalized,
            (ground.transform.position - transform.position).magnitude, LayerMask.GetMask("Ground"));
        velocity = Vector3.down * 9.8f;
        velocity = isGround ? Vector3.zero : velocity;

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            walkTime += Time.fixedDeltaTime;
            //print(Time.fixedDeltaTime);
            Vector3 pos = transform.position;
            walkLine.AddFirst(new TimePairTransform(walkTime, pos));
        }
        if (party_members.Length > 0 && canMove)
        {
            MovePartyMembers();
        }
        cc.Move((((right + up).normalized * movementspeed) + velocity) * Time.fixedDeltaTime);


        if (canMove)
        {
            // Keegan, I touched your code cause this deeply upset me, sorry. 
            // Your code is commented out, not delete if I broke anything. 

            // set vars that stay solid when not moving
            if (Input.GetAxisRaw("Horizontal") > 0.0f)
                directionX = "right";
            else if (Input.GetAxisRaw("Horizontal") < 0.0f)
                directionX = "left";
            if (Input.GetAxisRaw("Vertical") > 0.0f)
                directionY = "forward";
            else if ((right + up).sqrMagnitude > 0.0001) // face the camera if moving in any direction but away
                directionY = "backward";

            // Moving Animation
            bool isMoving = ((right + up).sqrMagnitude > 0.0001);
            player_animator.SetBool("Walking", isMoving);
            godfather_animator.SetBool("Walking", isMoving);
            hunter_animator.SetBool("Walking", isMoving);
            warrior_animator.SetBool("Walking", isMoving);
            mechanist_animator.SetBool("Walking", isMoving);

            // Left Right Flip
            bool movingRight = directionX == "right";
            playerRenderer.flipX = movingRight;
            godfatherRenderer.flipX = movingRight;
            hunterRenderer.flipX = movingRight;
            warriorRenderer.flipX = movingRight;
            mechanistRenderer.flipX = movingRight;

            // Changes to back facing animations
            bool backToCamera = directionY == "forward";
            player_animator.SetBool("Back", backToCamera);
            godfather_animator.SetBool("Back", backToCamera);
            hunter_animator.SetBool("Back", backToCamera);
            warrior_animator.SetBool("Back", backToCamera);
            mechanist_animator.SetBool("Back", backToCamera);
        }
    }

    public void SetCanMove(bool c)
    {
        canMove = c;
        if(!c)
        {
            player_animator.SetBool("Walking", c);
            godfather_animator.SetBool("Walking", c);
            hunter_animator.SetBool("Walking", c);
            warrior_animator.SetBool("Walking", c);
            mechanist_animator.SetBool("Walking", c);
        }
    }

    public float backoff = 0.1f;
    private void MovePartyMembers()
    {
        for (int i = 0; i < party_members.Length; ++i)
        {
            float walkoff = walkTime - (backoff * (1 + i));
            int l = 0;
            foreach (TimePairTransform tpt in walkLine)
            {
                l++;
                if (walkoff >= tpt.time)
                {
                    party_members[i].transform.position = tpt.pos;
                    //print("Took " + l);
                    break;
                }
            }
        }
        
        
        while (walkLine.Count != 0 && walkLine.Last.Value.time < walkTime - 2.0f)
        {
            walkLine.RemoveLast();
        }
    }
}

