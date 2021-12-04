using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierDropZoneDetector : MonoBehaviour
{
    [SerializeField]
    public GameObject popupLoc;

    private int previousChildrenNum = 0;

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount != previousChildrenNum)
        {
            if (previousChildrenNum < transform.childCount)
            {
                GameObject newChild = transform.GetChild(previousChildrenNum).gameObject;
                newChild.GetComponent<ModifierPopUp>().popup.spawnLocation = popupLoc;
            }
            previousChildrenNum = transform.childCount;
        }
    }
}
