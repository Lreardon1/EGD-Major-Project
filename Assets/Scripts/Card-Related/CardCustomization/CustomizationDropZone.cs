using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationDropZone : MonoBehaviour
{
    [SerializeField]
    List<GameObject> subDropZones;

    //returns the first valid subDropZone obj can be dropped onto
    public GameObject DroppedOnto(GameObject obj)
    {
        foreach (GameObject subDZ in subDropZones)
        {
            DropZone dz = subDZ.GetComponent<DropZone>();
            if (dz.CheckAllowDrop(obj))
            {
                return subDZ;
            }
        }
        return null;
    }
}
