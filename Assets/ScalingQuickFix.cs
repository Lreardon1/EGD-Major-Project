using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingQuickFix : MonoBehaviour
{
    bool isShrunk = false;
    // Update is called once per frame
    void Update()
    {
        if (transform.parent.gameObject.name == "ModifierSlot" && !isShrunk)
        {
            Shrink();
        }
        else if (transform.parent.gameObject.name != "ModifierSlot" && isShrunk)
        {
            Grow();
        }
    }

    void Shrink()
    {
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        isShrunk = true;
    }

    void Grow()
    {
        transform.localScale = new Vector3(1, 1, 1);
        isShrunk = false;
    }
}
