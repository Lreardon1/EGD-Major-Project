using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionOrderUI : MonoBehaviour
{
    public GameObject indicatorPrefab;

    protected CombatManager manager;
    protected List<GameObject> turnOrder;
    protected Dictionary<GameObject, GameObject> indicators;
    protected bool bInit = false;
    protected RectTransform rect;

    public void Init(List<GameObject> turnOrderActors, CombatManager cManager)
    {
        rect = GetComponent<RectTransform>();
        indicators = new Dictionary<GameObject, GameObject>();

        manager = cManager;
        turnOrder = turnOrderActors; // REFERENCE!!!

        foreach (GameObject actor in turnOrder)
        {
            GameObject i = Instantiate(indicatorPrefab, transform.parent, false);
            i.GetComponent<Image>().color = actor.GetComponent<SpriteRenderer>().color;
            i.GetComponent<Image>().sprite = actor.GetComponent<SpriteRenderer>().sprite;
            i.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            indicators.Add(actor, i);
        }

        bInit = true;
    }

    public void SetAllActive(List<GameObject> turnOrderActors)
    {
        foreach (var indicPair in indicators)
        {
            if (manager.actionOrder.Contains(indicPair.Key))
            {
                indicPair.Value.SetActive(true);
            }
        }
    }

    public void UpdateOrder(List<GameObject> turnOrderActors)
    {
        if (!bInit)
            return;

        foreach (var indicPair in indicators)
        {
            if(!manager.actionOrder.Contains(indicPair.Key))
            {
                indicPair.Value.SetActive(false);
            }
            int turnStatus = manager.actionOrder.IndexOf(indicPair.Key);
            GameObject indic = indicPair.Value;

            Vector3 pos = transform.localPosition;
            pos.y = (rect.localPosition.y - rect.rect.height / 2f) + turnStatus * (rect.rect.height / 7f);
            //pos.y = Mathf.Lerp(rect.localPosition.y - rect.rect.height / 2, rect.localPosition.y + rect.rect.height / 2, turnStatus);
            pos.x += indicPair.Key.GetComponent<CombatantBasis>().isEnemy ? (-rect.rect.width / 3) : (rect.rect.width / 3);
            indic.GetComponent<RectTransform>().localPosition = pos;
            indic.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.rect.width * 1.4f, rect.rect.width * 1.4f);
            indic.GetComponent<Image>().SetNativeSize();
        }
    }
}
