using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class OneOnTurnActor : MonoBehaviour
{
    public enum Element
    {
        Lightning,
        Fire,
        Water, 
        Nature,
        Gust,
        None,
    }
    // TODO : this functionality is more than beats or not but for testing this is it.
    public static bool ElementBeats(Element a, Element b)
    {
        switch (a)
        {
            case Element.Lightning:
                return b == Element.Water;
            case Element.Fire:
                return b == Element.Nature;
            case Element.Water:
                return b == Element.Lightning;
            case Element.Nature:
                return b == Element.Gust;
            case Element.Gust:
                return b == Element.Fire;
            default:
                return false;
        }
    }
    public static bool ElementLosesTo(Element a, Element b)
    {
        switch (a)
        {
            case Element.Lightning:
                return b == Element.Nature;
            case Element.Fire:
                return b == Element.Water;
            case Element.Water:
                return false;
            case Element.Nature:
                return b == Element.Fire;
            case Element.Gust:
                return b == Element.Gust;
            default:
                return false;
        }
    }


    [HideInInspector]
    public OneOnCombatManager manager;
    [Header("Sprites")]
    public Sprite actorSprite;
    public Sprite actorOrderSprite;
    [Header("Action Notif")]
    public TMP_Text actionText;
    public TMP_Text statusText;
    public Image actionImage;
    public OneOnAction nextAction;

    private float turnStatus;
    private bool defeated = false;

    public int health = 10;

    public Element appliedElement = Element.None;

    internal void CheckIfNewlyDefeated()
    {
        // TODO : more may be needed here
        defeated = health <= 0;
        GetComponent<SpriteRenderer>().enabled = !defeated;
        actionText.enabled = !defeated;
    }

    public virtual float AddToTurnStatus(float update)
    {
        return turnStatus += update;
    }

    public virtual float GetTurnStatus()
    {
        return turnStatus;
    }

    public virtual void TakeAction()
    {
        // TODO : we can use update and coroutines to make ANYTHING happen here
        nextAction.TakeAction(this);

        PickNextAction();
        UpdateActionDisplay();
        turnStatus = UnityEngine.Random.Range(0.4f, 1.0f);
        manager.RequestActorTurnOver(this);
    }
    public virtual bool GetIsDefeated()
    {
        return defeated;
    }

    public virtual bool IsPlayerAlly()
    {
        return false;
    }

    // TODO : used when the scene changes, (ie target has died or status effect applied)
    // characters should probably only react to target died?
    public virtual void UpdateScene(OneOnCombatManager manager)
    {

    }

    // TODO : each should override this actually...
    public virtual void PickNextAction()
    {
        Debug.LogError("ERROR: Base PickNextAction function called from " + name);
    }

    public virtual void UpdateActionDisplay()
    {
        if (nextAction != null)
            actionText.text = nextAction.ToString();
        else
            actionText.text = "No action set!";
    }

    private Color color;
    public virtual void InitActor(OneOnCombatManager cManager)
    {
        manager = cManager;
        turnStatus = UnityEngine.Random.Range(0.0f, 1.0f);
        color = GetComponent<SpriteRenderer>().color;
        PickNextAction();
        UpdateActionDisplay();
    }

    public Color GetColor()
    {
        return color;
    }
}
