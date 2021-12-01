using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterHunterActions : AfterCutsceneActions
{
    // BASE CLASS TO PERFORM AFTER CUTSCENE ACTIONS
    public override void TakeActionsAfterCutscene()
    {
        Debug.LogError("ERROR: ADD THE HUNTER TO YOUR PARTY HERE AFTER THIS CUTSCENE");
    }
}