using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public bool canEditCards = false;

    public CanvasManager cm;

    void Start()
    {
        if (cm == null)
        {
            cm = FindObjectOfType<CanvasManager>();
            cm.player = gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canEditCards)
            {
                cm.OpenCustomization();
            }
        }
    }
}
