using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterHunterActions : AfterCutsceneActions
{
    public OverworldMovement player;
    public MinimapManager minimap;

    // BASE CLASS TO PERFORM AFTER CUTSCENE ACTIONS
    public override void TakeActionsAfterCutscene()
    {
        player.AddPartyMember("hunter");
        minimap.UpdateTargetDestination();
    }
}