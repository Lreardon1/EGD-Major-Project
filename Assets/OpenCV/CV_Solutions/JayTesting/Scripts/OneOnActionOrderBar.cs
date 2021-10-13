using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneOnActionOrderBar : MonoBehaviour
{
    public GameObject indicatorPrefab;

    protected OneOnCombatManager manager;
    protected List<OneOnTurnActor> turnOrder;
    protected Dictionary<OneOnTurnActor, GameObject> indicators;
    protected bool bInit = false;
    protected RectTransform rect;

    public void Init(List<OneOnTurnActor> turnOrderActors, OneOnCombatManager cManager)
    {
        rect = GetComponent<RectTransform>();
        indicators = new Dictionary<OneOnTurnActor, GameObject>();

        manager = cManager;
        turnOrder = turnOrderActors; // REFERENCE!!!

        foreach (OneOnTurnActor actor in turnOrder)
        {
            GameObject i = Instantiate(indicatorPrefab, transform.parent, false);
            i.GetComponent<Image>().color = actor.GetColor();
            i.GetComponent<Image>().sprite = actor.actorOrderSprite;

            indicators.Add(actor, i);
        }

        bInit = true;
    }

    private void Update()
    {
        if (!bInit)
            return;

        foreach (var indicPair in indicators)
        {
            float turnStatus = indicPair.Key.GetTurnStatus();
            GameObject indic = indicPair.Value;

            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(rect.localPosition.y - rect.rect.height / 2, rect.localPosition.y + rect.rect.height / 2, turnStatus);
            pos.x += indicPair.Key.IsPlayerAlly() ? (-rect.rect.width / 3) : (rect.rect.width / 3);
            indic.GetComponent<RectTransform>().localPosition = pos;
            indic.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.rect.width * 1.4f, rect.rect.width * 1.4f);
        }
    }
}
