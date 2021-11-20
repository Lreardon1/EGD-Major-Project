using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterChurchActions : AfterCutsceneActions
{
    // BASE CLASS TO PERFORM AFTER CUTSCENE ACTIONS
    public override void TakeActionsAfterCutscene()
    {
        Debug.LogError("ERROR: ADD THE PRIEST TO YOUR PARTY AFTER THIS CUTSCENE");
    }
}