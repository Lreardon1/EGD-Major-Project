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
        animController.PlayCutscene();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (triggerCondition)
        {
            case CutsceneTriggerConditions.GameStart:
                TriggerCutscene();
                break;
            case CutsceneTriggerConditions.CardsGained:
                break;
            case CutsceneTriggerConditions.Etc:
                break;
        }
    }
}
