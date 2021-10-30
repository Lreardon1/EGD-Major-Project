using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCardAction : CardActionTemplate
{
    public override void OnPlay(Card c)
    {
        base.OnPlay(c);

        Card.Element type;
        int numModifier;
        Card.Element secondaryElement;
        Card.AoE aoe;
        bool givePriority;

    }
}
