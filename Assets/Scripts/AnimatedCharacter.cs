using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedCharacter : MonoBehaviour
{
    public bool isWalking;
    public bool isBack;
    public Camera cam;

    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
        // enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Walking", isWalking);
        Vector2 camForward = new Vector2(cam.transform.forward.x, cam.transform.forward.z).normalized;
        Vector2 myForward = new Vector2(transform.forward.x, transform.forward.z).normalized;

        anim.SetBool("Back", Vector2.Dot(camForward, myForward) < 0.0f);
        print($"{name} : {Vector2.Dot(camForward, myForward)}");
    }
}
