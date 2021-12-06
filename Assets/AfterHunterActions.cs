using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterHunterActions : AfterCutsceneActions
{
    public OverworldMovement player;
    // BASE CLASS TO PERFORM AFTER CUTSCENE ACTIONS
    public override void TakeActionsAfterCutscene()
    {
        Debug.Log("ADDED THE HUNTER TO YOUR PARTY HERE AFTER THIS CUTSCENE");
        player.AddPartyMember("hunter");
    }
}