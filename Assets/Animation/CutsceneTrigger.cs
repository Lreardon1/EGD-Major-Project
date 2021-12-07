using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{

    public enum CutsceneTriggerConditions
    {
        GameStart,
        CardsGained, 
        Etc
    }

    public CutsceneTriggerConditions triggerCondition;
    public AnimationController animController;

    private void TriggerCutscene()
    {
        print("TRIGGER");
        animController.PlayCutscene();
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (triggerCondition)
        {
            case CutsceneTriggerConditions.GameStart:
                TriggerCutscene();
                break;
            case CutsceneTriggerConditions.CardsGained:
                if (AfterCutsceneActions.HasCards)
                {
                    TriggerCutscene();
                }
                break;
            case CutsceneTriggerConditions.Etc:
                break;
        }
    }
}
