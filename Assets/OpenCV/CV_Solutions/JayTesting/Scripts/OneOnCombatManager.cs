using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneOnCombatManager : MonoBehaviour
{
    public enum PlayMode
    {
        CardAction,
        ActorAction,
        TimeProgress,
        End
    }
    public float deltaTimeMultipler = 0.2f;

    public int CountStillUp<T>(List<T> lis) where T : OneOnTurnActor
    {
        int c = 0;
        foreach (T actor in lis)
        {
            if (!actor.GetIsDefeated())
                c += 1;
        }
        return c;
    } 

    public T IndexByStillUp<T>(List<T> lis, int ind) where T : OneOnTurnActor
    {
        int i = 0; 
        foreach (T actor in lis)
        {
            if (!actor.GetIsDefeated())
            {
                if (i == ind)
                    return actor;
                else
                    i++;
            }
        }
        return null;
    }

    public OneOnTurnActor GetRandomPartyMember()
    {
        int c = CountStillUp(party);
        int r = Random.Range(0, c);
        return IndexByStillUp(party, r);
    }

    public OneOnTurnActor GetRandomEnemy()
    {
        int c = CountStillUp(enemies);
        int r = Random.Range(0, c);
        return IndexByStillUp(enemies, r);
    }

    public PlayMode currentPlayMode;
    public List<OneOnPartyMember> party;
    public List<OneOnEnemy> enemies;
    public OneOnPlayerController player;

    public OneOnActionOrderBar actionBar;
    public List<OneOnTurnActor> turnOrderActors;

    private void Awake()
    {
        StartCombat();
    }

    public void InsertIntoTurnOrder(OneOnTurnActor actor)
    {
        int i = 0;
        for (i = 0; i < turnOrderActors.Count; ++i)
        {
            if (turnOrderActors[i].GetTurnStatus() >= actor.GetTurnStatus())
                break;
        }
        turnOrderActors.Insert(i, actor);
    }

    public void StartCombat()
    {
        // init action order and players
        foreach (OneOnPartyMember p in party)
        {
            p.InitActor(this);
            InsertIntoTurnOrder(p);
        }
        foreach (OneOnEnemy enemy in enemies)
        {
            enemy.InitActor(this);
            InsertIntoTurnOrder(enemy);
        }
        // give the list to the action bar
        actionBar.Init(turnOrderActors, this);
        // current mode is play
        currentPlayMode = PlayMode.TimeProgress;
    }
    
    public void Update()
    {
        switch(currentPlayMode)
        {
            case PlayMode.TimeProgress:
                HandleTimeProgress();
                break;
            case PlayMode.ActorAction:
                HandleActorAction();
                break;
            case PlayMode.CardAction:
                HandleCardAction();
                break;
            case PlayMode.End:
                HandleEnd();
                break;
        }
    }

    public void PlayCard(OneOnPlayCard card)
    {
        card.ModifyAction(turnOrderActors[0].nextAction);
        turnOrderActors[0].nextAction.modificationCard = card;
        turnOrderActors[0].UpdateActionDisplay();
    }

    private void HandleTimeProgress()
    {
        float delta = Time.deltaTime * deltaTimeMultipler;
        if (turnOrderActors[0].GetTurnStatus() <= 0.0f)
        {
            currentPlayMode = PlayMode.CardAction;
            player.RequestCardAction(this);
            // TODO : highlight current actor or something
        } else {
            foreach (OneOnTurnActor actor in turnOrderActors)
                actor.AddToTurnStatus(-delta);
        }
    }

    private bool CheckForLoss()
    {
        bool loss = true;
        foreach (OneOnPartyMember member in party)
            loss &= member.GetIsDefeated();
        return loss;
    }

    private bool CheckForWin()
    {
        bool victory = true;
        foreach (OneOnEnemy enemy in enemies)
            victory &= enemy.GetIsDefeated();
        return victory;
    }

    public bool RequestCardTurnOver(OneOnPlayerController player)
    {
        if (currentPlayMode != PlayMode.CardAction)
            throw new System.Exception("INVALID STATE TO CALL THIS FUNCTION");

        OneOnTurnActor actor = turnOrderActors[0];
        currentPlayMode = PlayMode.ActorAction;
        actor.TakeAction();

        return true;
    }

    public bool RequestActorTurnOver(OneOnTurnActor returningActor)
    {
        // these should never be false when this is called
        if (turnOrderActors[0] != returningActor || currentPlayMode != PlayMode.ActorAction)
            throw new System.Exception("INVALID STATE TO CALL THIS FUNCTION");
        // remove and readd the actor that just took an action
        turnOrderActors.RemoveAt(0);
        InsertIntoTurnOrder(returningActor);

        if (CheckForLoss() || CheckForWin())
            currentPlayMode = PlayMode.End;
        else
            currentPlayMode = PlayMode.TimeProgress;

        // inform all actors that the scene has changed, in particular characters may have died.
        foreach (OneOnTurnActor actor in turnOrderActors)
            actor.UpdateScene(this);

        return true;
    }

    private void HandleEnd()
    {
        print("THIS IS THE END, THERE IS NOTHING ELSE");
    }

    private void HandleCardAction()
    {

    }

    private void HandleActorAction()
    {

    }

}
