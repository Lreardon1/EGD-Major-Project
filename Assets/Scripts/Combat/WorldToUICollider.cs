using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldToUICollider : MonoBehaviour
{
    public Transform combatant;
    public BoxCollider worldSpaceBC;

    public RectTransform combatantUITransform;
    public BoxCollider2D uiBC;

    public float cardLocationOffset = 100f;
    
    Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        //uiOffset = new Vector2((float)canvas.sizeDelta.x / 2f, (float)Canvas.sizeDelta.y / 2f);
        mainCam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 uiPos = mainCam.WorldToScreenPoint(combatant.position);
        Vector3 colliderWUIOffset = combatant.position;
        Vector3 colliderHUIOffset = combatant.position;

        colliderWUIOffset.x += worldSpaceBC.bounds.size.x;
        colliderHUIOffset.y += worldSpaceBC.bounds.size.y;

        float colliderWidth = (Vector2.Distance(uiPos, mainCam.WorldToScreenPoint(colliderWUIOffset)));
        float colliderHeight = (Vector2.Distance(uiPos, mainCam.WorldToScreenPoint(colliderHUIOffset)));

        if(combatant.gameObject.GetComponent<CombatantBasis>().isEnemy)
        {
            uiPos.x += cardLocationOffset;
            combatantUITransform.position = uiPos;
            uiBC.size = new Vector2(colliderWidth, colliderHeight);
            uiBC.offset = new Vector2(-cardLocationOffset, 0);
        } else
        {
            uiPos.x -= cardLocationOffset;
            combatantUITransform.position = uiPos;
            uiBC.size = new Vector2(colliderWidth, colliderHeight);
            uiBC.offset = new Vector2(cardLocationOffset, 0);
        }
        
    }
}
