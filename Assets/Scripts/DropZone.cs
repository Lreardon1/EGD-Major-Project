using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public int allowedChildren;
    public Modifier.ModifierEnum slotType = Modifier.ModifierEnum.None;

    public bool CheckAllowDrop(GameObject dropped)
    {
        if (dropped == null || !gameObject.activeSelf)
        {
            return false;
        }
        bool isValid = true;
        if (transform.childCount >= allowedChildren && allowedChildren != -1)
        {
            isValid = false;
        }

        if (slotType != Modifier.ModifierEnum.None)
        {
            if (slotType != dropped.GetComponent<DragDrop>().dropType)
            {
                isValid = false;
            }
        }

        return isValid;
    }
}
