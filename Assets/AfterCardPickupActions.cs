using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterCardPickupActions : AfterCutsceneActions
{
    public TutorialManager tutManager;

    public override void TakeActionsAfterCutscene()
    {
        Debug.LogError("MAKE CHANGES AND SET VARIABLES AFTER CARD CUTSCENE");
        tutManager.StartTutorial();
        AfterCutsceneActions.HasCards = true;
        PlayerPrefs.SetInt("hasCards", 1);
    }
}
