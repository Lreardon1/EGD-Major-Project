using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{

    public enum CutsceneTriggerConditions
    {
        NoPriest,
        GameStart,
        CardsGained, 
        Etc
    }

    public void Start()
    {
        if (PlayerPrefs.HasKey("priest") && triggerCondition == CutsceneTriggerConditions.NoPriest)
            animController.gameObject.SetActive(false);
        if (!PlayerPrefs.HasKey("hasCards") && triggerCondition == CutsceneTriggerConditions.CardsGained)
            animController.gameObject.SetActive(false);
    }

    public CutsceneTriggerConditions triggerCondition;
    public AnimationController animController;

    private void TriggerCutscene()
    {
        animController.PlayCutscene();
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerCutscene();

        /*
        switch (triggerCondition)
        {
            case CutsceneTriggerConditions.NoPriest:
                TriggerCutscene();
                break;
            case CutsceneTriggerConditions.GameStart:
                TriggerCutscene();
                break;
            case CutsceneTriggerConditions.CardsGained:
                TriggerCutscene();
                break;
            case CutsceneTriggerConditions.Etc:
                break;
        }
        */
    }
}
