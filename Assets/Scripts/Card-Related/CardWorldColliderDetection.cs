using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardWorldColliderDetection : MonoBehaviour
{
    public DragDrop dragDrop;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        dragDrop.isOverDropZone = true;
        dragDrop.dropZones.Add(collision.gameObject);
        //print(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        dragDrop.dropZones.Remove(collision.gameObject);
        if (dragDrop.dropZones.Count == 0)
        {
            dragDrop.isOverDropZone = false;
        }
    }
}
