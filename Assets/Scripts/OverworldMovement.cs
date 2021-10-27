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
    bool rotated;
    string rotation_way;
    public bool canMove = true;
    public List<UnityEngine.Vector3> movements = new List<UnityEngine.Vector3>();
    private LinkedList<TimePairTransform> walkLine = new LinkedList<TimePairTransform>();
    private float walkTime = 0;

    float elapsed = 0f;
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
        direction = "left";
        //Fetch the SpriteRenderer from the GameObject
        characterRenderer = GetComponent<SpriteRenderer>();
        characterRenderer.flipX = false;
        cc = GetComponent<CharacterController>();
        rotated = false;
        rotation_way = "";
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
            //float rightTurn = Input.GetAxisRaw("Rotate");
            //transform.Rotate(Vector3.up, rightTurn * turnSpeed * Time.deltaTime);

            //player camera rotation
            if (Input.GetKeyDown(KeyCode.E) && rotated != true)
            {
                transform.Rotate(0, 90, 0);
                rotated = true;
                rotation_way = "E";
            }
            else if (Input.GetKeyDown(KeyCode.Q) && rotated != true)
            {
                transform.Rotate(0, -90, 0);
                rotated = true;
                rotation_way = "Q";
            }
            else if (Input.GetKeyUp(KeyCode.E) && rotation_way == "E")
            {
                rotated = false;
            }
            else if (Input.GetKeyUp(KeyCode.Q) && rotation_way == "Q")
            {
                rotated = false;
            }

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
            if (Input.GetKey(KeyCode.D))
            {
                animator.SetBool("Walking", true);
                //transform.Translate(Vector3.right * movementspeed * Time.fixedDeltaTime);
                if (direction != "right" || direction != "forward")
                {
                    characterRenderer.flipX = true;
                    direction = "right";
                }
            }
            else if (Input.GetKey(KeyCode.A))
            {
                animator.SetBool("Walking", true);
                //transform.Translate(Vector3.left * movementspeed * Time.fixedDeltaTime);
                if (direction != "left" || direction != "back")
                {
                    characterRenderer.flipX = false;
                    direction = "left";
                }
            }
            else if (Input.GetKey(KeyCode.W))
            {
                animator.SetBool("Walking", true);
                //transform.Translate(Vector3.forward * movementspeed * Time.fixedDeltaTime);

            }
            else if (Input.GetKey(KeyCode.S))
            {
                animator.SetBool("Walking", true);
                //transform.Translate(Vector3.back * movementspeed * Time.fixedDeltaTime);

            }
            else
            {
                animator.SetBool("Walking", false);
            }
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

