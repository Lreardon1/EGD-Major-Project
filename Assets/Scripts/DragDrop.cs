using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Jay note : same note as on CombatHandController.
//   this class (and the other) should have been the one to control ALL movement of cards, 
//   putting it in CombatManager is not only awful code practice, it is explicitly what I requested you NOT do.
public class DragDrop : MonoBehaviour
{
    public bool isDraggable = true;
    public GameObject dragger;
    public bool isOverDropZone = false;
    private GameObject previousParent;
    [SerializeField]
    public List<GameObject> allowedDropZones = new List<GameObject>();
    public List<GameObject> dropZones = new List<GameObject>();
    private Vector2 startPosition;
    private RectTransform trans;
    public Modifier.ModifierEnum dropType = Modifier.ModifierEnum.None;

    void Start()
    {
        trans = GetComponent<RectTransform>();
        Dragger d = FindObjectOfType<Dragger>();
        if (d != null)
        {
            dragger = d.gameObject;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOverDropZone = true;
        dropZones.Add(collision.gameObject);
        //print(collision.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        dropZones.Remove(collision.gameObject);
        if (dropZones.Count == 0)
        {
            isOverDropZone = false;
        }
    }

    public void StartDrag()
    {
        dropZones.Clear();
        if (isDraggable)
        {
            startPosition = trans.localPosition;
            previousParent = trans.parent.gameObject;
            dragger.GetComponent<Dragger>().isDragging = true;
            trans.localPosition = new Vector3(0, 0, 0);
            trans.SetParent(dragger.transform, false);
            centerOnDragger();
        }
    }

    private void centerOnDragger()
    {
        trans.anchorMax = new Vector2(0.5f, 0.5f);
        trans.anchorMin = new Vector2(0.5f, 0.5f);
        trans.anchoredPosition = new Vector2(0.5f, 0.5f);
    }

    private GameObject GetClosestValidDropZone()
    {
        List<GameObject> valid = new List<GameObject>();
        GameObject retVal = null;
        //first performing validation checks

        foreach (GameObject dropZone in dropZones)
        {
            CustomizationDropZone cdz = dropZone.GetComponent<CustomizationDropZone>();
            if (cdz != null)
            {
                GameObject subDropZone = cdz.DroppedOnto(gameObject);
                if (subDropZone != null)
                {
                    DropZone dz = subDropZone.GetComponent<DropZone>();
                    if (subDropZone != previousParent && (dz == null || dz.CheckAllowDrop(gameObject))) //prevents dropping onto same parent and check is the DropZone script is present, asking it if drop is valid)
                    {
                        if (allowedDropZones.Count == 0 || allowedDropZones.Contains(subDropZone)) //if no specific drop zones are specified, goes to any, otherwise only to specified
                        {
                            valid.Add(subDropZone);
                        }
                    }
                }
            }
            else
            {
                DropZone dz = dropZone.GetComponent<DropZone>();
                if (dropZone != previousParent && (dz == null || dz.CheckAllowDrop(gameObject))) //prevents dropping onto same parent and check is the DropZone script is present, asking it if drop is valid)
                {
                    if (allowedDropZones.Count == 0 || allowedDropZones.Contains(dropZone)) //if no specific drop zones are specified, goes to any, otherwise only to specified
                    {
                        valid.Add(dropZone);
                    }
                }
            }
        }

        if (valid.Count == 0)
        {
            return retVal;
        }

        //then picking minimum distance from valid collision
        retVal = valid[0];
        float minDist = Vector3.Distance(gameObject.transform.position, retVal.transform.position);
        foreach (GameObject dropZone in valid)
        {
            float dist = Vector3.Distance(gameObject.transform.position, dropZone.transform.position);
            if (dist < minDist)
            {
                retVal = dropZone;
                minDist = dist;
            }
        }

        return retVal;
    }

    public void EndDrag()
    {
        if (isDraggable)
        {
            dragger.GetComponent<Dragger>().isDragging = false;
            GameObject dropZone = null;
            if (isOverDropZone)
            {
                dropZone = GetClosestValidDropZone();
            }
            //print(dropZone);
            if (isOverDropZone && dropZone != null) //prevents dropping onto same parent and check is the DropZone script is present, asking it if drop is valid
            {
                //print(dropZone);
                ScrollRect scrollRectZone = dropZone.GetComponent<ScrollRect>();
                if (scrollRectZone != null && scrollRectZone.content != previousParent)
                {
                    trans.SetParent(scrollRectZone.content.transform, false);
                }
                else
                {
                    trans.SetParent(dropZone.transform, false);
                    if(dropZone.GetComponent<WorldToUICollider>() != null)
                    {
                        FindObjectOfType<CombatManager>().ApplyCard(this.gameObject, dropZone.GetComponent<WorldToUICollider>().combatant.gameObject);
                        trans.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    } if(dropZone.CompareTag("DiscardPile"))
                    {
                        FindObjectOfType<CombatManager>().DiscardCard(this.gameObject);
                        trans.localScale = new Vector3(1f, 1f, 1f);
                    }
                }
            }
            else
            {
                trans.SetParent(previousParent.transform, false);
                trans.localPosition = startPosition;
            }
        }
    }
}
