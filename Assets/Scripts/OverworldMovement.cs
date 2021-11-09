using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMovement : MonoBehaviour
{
    public GameObject ground;
    //public GameObject warriorBack;
    public GameObject playerBack;
    public GameObject godfatherBack;
    public int movementspeed = 100;
    string directionY;
    string directionX;
    SpriteRenderer playerRenderer;
    SpriteRenderer godfatherRenderer;
    SpriteRenderer warriorRenderer;
    SpriteRenderer playerBackRenderer;
    SpriteRenderer godfatherBackRenderer;
    //SpriteRenderer warriorBackRenderer;
    public Animator player_animator;
    public Animator warrior_animator;
    public Animator godfather_animator;
    public Animator player_back_animator;
    //public Animator warrior_back_animator;
    public Animator godfather_back_animator;
    public CharacterController cc;
    public bool canMove = true;
    public List<UnityEngine.Vector3> movements = new List<UnityEngine.Vector3>();
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
        godfatherRenderer = party_members[1].GetComponent<SpriteRenderer>();
        godfatherRenderer.flipX = false;
        warriorRenderer = party_members[0].GetComponent<SpriteRenderer>();
        warriorRenderer.flipX = false;
        playerBackRenderer = playerBack.GetComponent<SpriteRenderer>();
        playerBackRenderer.flipX = false;
        godfatherBackRenderer = godfatherBack.GetComponent<SpriteRenderer>();
        godfatherBackRenderer.flipX = false;
        //warriorBackRenderer = warriorBack.GetComponent<SpriteRenderer>();
        //warriorBackRenderer.flipX = false;
        cc = GetComponent<CharacterController>();
        movements.Add(Vector3.zero);
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
            party_members[1].transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);
            party_members[0].transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);
           
            right = Input.GetAxisRaw("Horizontal") * transform.right;
            up = Input.GetAxisRaw("Vertical") * transform.forward;
        }

        // could just use gravity, character controller will handle not going thru the floor but this gives me more control
        bool isGround = Physics.Raycast(transform.position,
            (ground.transform.position - transform.position).normalized,
            (ground.transform.position - transform.position).magnitude, LayerMask.GetMask("Ground"));
        velocity += Vector3.down * 9.8f * Time.deltaTime;
        velocity = isGround ? Vector3.zero : velocity;

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            walkTime += Time.fixedDeltaTime;
            //print(Time.fixedDeltaTime);
            Vector3 pos = transform.position;
            walkLine.AddFirst(new TimePairTransform(walkTime, pos));
        }
        if (party_members.Length > 0)
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
            else if (Input.GetAxisRaw("Vertical") < 0.0f)
                directionY = "backward";
            else
                directionY = "backward"; // TODO : so when you stop moving in Y, characters face you again

            // Moving Animation
            bool isMoving = (right + up).sqrMagnitude > 0.0001;
            player_animator.SetBool("Walking", isMoving);
            warrior_animator.SetBool("Walking", isMoving);
            godfather_animator.SetBool("Walking", isMoving);
            player_back_animator.SetBool("Walking", isMoving);
            // warrior_back_animator.SetBool("Walking", (right + up).sqrMagnitude > 0.001);
            godfather_back_animator.SetBool("Walking", isMoving);

            // Left Right Flip
            bool movingRight = directionX == "right";
            playerRenderer.flipX = movingRight;
            godfatherRenderer.flipX = movingRight;
            warriorRenderer.flipX = movingRight;
            playerBackRenderer.flipX = movingRight;
            godfatherBackRenderer.flipX = movingRight;
            warriorRenderer.flipX = movingRight; //needs edit

            // TODO : why are you using a different spriteRenderer? 
            //       Can't you just make a new animation for the animator?
            bool backToCamera = directionY == "forward";
            playerRenderer.enabled = !backToCamera;
            godfatherRenderer.enabled = !backToCamera;
            //warriorRenderer.enabled = !backToCamera;
            playerBackRenderer.enabled = backToCamera;
            godfatherBackRenderer.enabled = backToCamera;
            //warriorRenderer.enabled = backToCamera;
            // * End of code tampering

            /*
            if (Input.GetKey(KeyCode.D))
            {
                player_animator.SetBool("Walking", true);
                warrior_animator.SetBool("Walking", true);
                godfather_animator.SetBool("Walking", true);
                player_back_animator.SetBool("Walking", true);
                //warrior_back_animator.SetBool("Walking", true);
                godfather_back_animator.SetBool("Walking", true);
                //transform.Translate(Vector3.right * movementspeed * Time.fixedDeltaTime);
                if (directionX != "right" && directionY == "forward")
                {
                    playerBackRenderer.flipX = true;
                    godfatherBackRenderer.flipX = true;
                    warriorRenderer.flipX = true; //needs edit
                    directionX = "right";
                }
                else if (directionX != "right" && directionY == "backward")
                {
                    playerRenderer.flipX = true;
                    godfatherRenderer.flipX = true;
                    warriorRenderer.flipX = true;
                    directionX = "right";
                }
            }
            else if (Input.GetKey(KeyCode.A))
            {
                player_animator.SetBool("Walking", true);
                warrior_animator.SetBool("Walking", true);
                godfather_animator.SetBool("Walking", true);
                player_back_animator.SetBool("Walking", true);
                //warrior_back_animator.SetBool("Walking", true);
                godfather_back_animator.SetBool("Walking", true);
                if (directionX != "left" && directionY == "forward")
                {
                    playerBackRenderer.flipX = false;
                    godfatherBackRenderer.flipX = false;
                    warriorRenderer.flipX = false; //needs edit
                    directionX = "left";
                }
                else if (directionX != "left" && directionY == "backward")
                {
                    playerRenderer.flipX = false;
                    godfatherRenderer.flipX = false;
                    warriorRenderer.flipX = false;
                    directionX = "left";
                }
            }
            else if (Input.GetKey(KeyCode.W))
            {
                player_animator.SetBool("Walking", true);
                warrior_animator.SetBool("Walking", true);
                godfather_animator.SetBool("Walking", true);
                player_back_animator.SetBool("Walking", true);
                //warrior_back_animator.SetBool("Walking", true);
                godfather_back_animator.SetBool("Walking", true);
                if (directionY != "forward")
                {
                    playerRenderer.enabled = false;
                    godfatherRenderer.enabled = false;
                    //warriorRenderer.enabled = false;
                    playerBackRenderer.enabled = true;
                    godfatherBackRenderer.enabled = true;
                    //warriorRenderer.enabled = true;
                    playerRenderer.flipX = false;
                    godfatherRenderer.flipX = false;
                    warriorRenderer.flipX = false;
                    directionY = "forward";
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                player_animator.SetBool("Walking", true);
                warrior_animator.SetBool("Walking", true);
                godfather_animator.SetBool("Walking", true);
                player_back_animator.SetBool("Walking", true);
                //warrior_back_animator.SetBool("Walking", true);
                godfather_back_animator.SetBool("Walking", true);
                if (directionY != "backward")
                {
                    playerRenderer.enabled = true;
                    godfatherRenderer.enabled = true;
                    //warriorRenderer.enabled = true;
                    playerBackRenderer.enabled = false;
                    godfatherBackRenderer.enabled = false;
                    //warriorRenderer.enabled = false;
                    playerRenderer.flipX = true;
                    godfatherRenderer.flipX = true;
                    warriorRenderer.flipX = true;
                    directionY = "backward";
                }
            }
            else
            {
                player_animator.SetBool("Walking", false);
                warrior_animator.SetBool("Walking", false);
                godfather_animator.SetBool("Walking", false);
                player_back_animator.SetBool("Walking", false);
                //warrior_back_animator.SetBool("Walking", false);
                godfather_back_animator.SetBool("Walking", false);
            }*/
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

