using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToWorldCollider : MonoBehaviour
{
    public GameObject colliderGO;
    public BoxCollider2D bc;
    public Rigidbody2D rb;

    public RectTransform rTransform;

    public Camera mainCam;

    public bool inHand = false;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = FindObjectOfType<Camera>();
    }

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(mainCam != null && inHand)
        {
            Vector3 colliderPos = mainCam.ScreenToWorldPoint(rTransform.position);
            Vector3 buttonPosWOffset = rTransform.position;
            Vector3 buttonPosHOffset = rTransform.position;

            buttonPosWOffset.x += rTransform.rect.width;
            buttonPosHOffset.y += rTransform.rect.height;


            float colliderWidth = (Vector2.Distance(colliderPos, mainCam.ScreenToWorldPoint(buttonPosWOffset)));
            float colliderHeight = (Vector2.Distance(colliderPos, mainCam.ScreenToWorldPoint(buttonPosHOffset)));

            colliderPos.z = 0;
            rb.MovePosition(colliderPos);
            bc.size = new Vector2(colliderWidth, colliderHeight);
        }
    }
}
