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
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Walking", isWalking);
        anim.SetBool("Back", Vector3.Dot(cam.transform.forward, transform.forward) < 0.0f);
    }
}
