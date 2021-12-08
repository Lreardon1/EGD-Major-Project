using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterChurchActions : AfterCutsceneActions
{
    public OverworldMovement player;
    public MinimapManager minimap;
    public Transform nextDestination;

    // BASE CLASS TO PERFORM AFTER CUTSCENE ACTIONS
    public override void TakeActionsAfterCutscene()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("ADDING THE PRIEST TO YOUR PARTY HERE AFTER THIS CUTSCENE");
        player.AddPartyMember("priest");
        minimap.SetTargetDestination(nextDestination.position);
    }
}