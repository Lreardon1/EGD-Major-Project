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
        if (targets.Length == 0 || targets[0] == null)
            this.targets = new OneOnTurnActor[0];
        else
            this.targets = targets;
    }

    public virtual void TakeAction(OneOnTurnActor actor)
    {

    }

    public override string ToString()
    {
        if (targets.Length > 0)
            return "Base Action. Target: " + targets[0];
        else
            return "Base Action";
    }
}
