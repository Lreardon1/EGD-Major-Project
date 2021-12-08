using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{

    public enum CutsceneTriggerConditions
    {
        GameStart,
        NoPriest,
        NoHunter, 
        NoWarrior
    }

    public void Start()
    {
        if (PlayerPrefs.GetInt("priest", 0) == 1 && triggerCondition == CutsceneTriggerConditions.NoPriest)
            animController.gameObject.SetActive(false);
        if ((!PlayerPrefs.HasKey("hasCards") || PlayerPrefs.GetInt("hunter", 0) == 1) && triggerCondition == CutsceneTriggerConditions.NoHunter)
            animController.gameObject.SetActive(false);
        if ((!PlayerPrefs.HasKey("hasCards") || PlayerPrefs.GetInt("warrior", 0) == 1) && triggerCondition == CutsceneTriggerConditions.NoWarrior)
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
