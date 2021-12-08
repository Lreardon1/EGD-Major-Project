using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterMeetupActions : AfterCutsceneActions
{
    public MinimapManager minimap;
    public OverworldMovement player;

    public override void TakeActionsAfterCutscene()
    {
        Debug.Log("ADDED MORE PARTY MEMBERS");
        player.AddPartyMember("mechanist");
        player.AddPartyMember("warrior");
        minimap.UpdateTargetDestination();
    }
}
