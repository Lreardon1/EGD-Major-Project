using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterMeetupActions : AfterCutsceneActions
{
    public override void TakeActionsAfterCutscene()
    {
        base.TakeActionsAfterCutscene();
        Debug.LogError("ADD MORE PARTY MEMBERS");
    }
}
