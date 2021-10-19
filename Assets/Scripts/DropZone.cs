using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public int allowedChildren;

    public bool CheckAllowDrop()
    {
        if (transform.childCount < allowedChildren)
        {
            return true;
        }
        return false;
    }
}
