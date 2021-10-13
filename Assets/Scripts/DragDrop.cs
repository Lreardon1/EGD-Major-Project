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
        }
    }

    public void EndDrag()
    {
        if (isDraggable)
        {
            dragger.GetComponent<Dragger>().isDragging = false;
            if (isOverDropZone && dropZone != previousParent)
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
    }
}
