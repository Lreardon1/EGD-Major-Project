using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneOnAction
{
    public OneOnPlayCard modificationCard = null;
    public OneOnTurnActor[] targets;

    public virtual void SetModCard(OneOnPlayCard card)
    {
        modificationCard = card;
    }

    public virtual void SetTargets(params OneOnTurnActor[] targets)
    {
        this.targets = targets;
    }

    public virtual void TakeAction(OneOnTurnActor actor)
    {

    }

    public override string ToString()
    {
        return "Base Action. Target: " + targets[0];
    }
}
