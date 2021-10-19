using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour
{
    public bool isDraggable = true;
    public GameObject dragger;
    private bool isOverDropZone = false;
    private GameObject previousParent;
    [SerializeField]
    public List<GameObject> allowedDropZones = new List<GameObject>();
    private GameObject dropZone;
    private Vector2 startPosition;
    private RectTransform trans;

    void Start()
    {
        dragger = FindObjectOfType<Dragger>().gameObject;
        trans = GetComponent<RectTransform>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOverDropZone = true;
        dropZone = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone = false;
        dropZone = null;
    }

    public void StartDrag()
    {
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

    public void EndDrag()
    {
        if (isDraggable)
        {
            dragger.GetComponent<Dragger>().isDragging = false;
            DropZone dz = null;
            if (isOverDropZone)
            {
                dz = dropZone.GetComponent<DropZone>();
            }
            if (isOverDropZone && dropZone != previousParent && (dz == null || dz.CheckAllowDrop())) //prevents dropping onto same parent and check is the DropZone script is present, asking it if drop is valid
            {
                print(dropZone);
                if (allowedDropZones.Count == 0 || allowedDropZones.Contains(dropZone)) //if no specific drop zones are specified, goes to any, otherwise only to specified
                {
                    ScrollRect scrollRectZone = dropZone.GetComponent<ScrollRect>();
                    if (scrollRectZone != null && scrollRectZone.content != previousParent)
                    {
                        trans.SetParent(scrollRectZone.content.transform, false);
                    }
                    else
                    {
                        trans.SetParent(dropZone.transform, false);
                    }
                }
                else
                {
                    trans.SetParent(previousParent.transform, false);
                    trans.localPosition = startPosition;
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
