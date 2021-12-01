using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfoPopUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    float timeHovered = 0.0f;
    public float hoverToShowTime = 1.0f;
    public string title;
    [TextArea(15,20)]
    public string description;
    bool isHovered = false;
    bool isShown = false;

    [SerializeField]
    public GameObject popupPrefab;
    [SerializeField]
    public GameObject spawnLocation;

    private GameObject popup;

    // Update is called once per frame
    void Update()
    {
        //print("hovered is " + isHovered);
        if (isHovered)
        {
            timeHovered += Time.deltaTime;
            //display popup
            if (timeHovered > 1.0f && !isShown)
            {
                isShown = true;
                SpawnPopUp();
            }
        }
        else
        {
            if (isShown)
            {
                DespawnPopUp();
                isShown = false;
            }
            timeHovered = 0.0f;
        }
    }

    public void SpawnPopUp()
    {
        popup = Instantiate(popupPrefab, spawnLocation.transform);
        popup.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = title;
        popup.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = description;
        Animator anim = popup.GetComponent<Animator>();
        anim.SetBool("Show", true);
    }

    public void DespawnPopUp()
    {
        Animator anim = popup.GetComponent<Animator>();
        anim.SetBool("Show", false);
        DestroyOnVanish();
    }

    IEnumerator DestroyOnVanish()
    {
        yield return new WaitForSeconds(.4f);
        Destroy(popup);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
