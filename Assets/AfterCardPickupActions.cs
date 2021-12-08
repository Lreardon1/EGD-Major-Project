using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterCardPickupActions : AfterCutsceneActions
{
    public override void TakeActionsAfterCutscene()
    {
        Debug.LogError("MAKE CHANGES AND SET VARIABLES AFTER CARD CUTSCENE");
        AfterCutsceneActions.HasCards = true;
        PlayerPrefs.SetInt("hasCards", 1);
    }
}
