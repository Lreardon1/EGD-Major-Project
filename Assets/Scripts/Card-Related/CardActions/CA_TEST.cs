using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_TEST : CardActionTemplate
{
    public override void OnPlay(Card c)
    {
        base.OnPlay(c);
        print("CA_TEST ON PLAY");
        //do other things
    }
}
