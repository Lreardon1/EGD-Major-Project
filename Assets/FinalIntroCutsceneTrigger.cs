using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalIntroCutsceneTrigger : CutsceneTrigger
{

    public GameObject warrior;
    public GameObject mechanist;


    // Start is called before the first frame update
    void Start()
    {
        if (AfterCutsceneActions.HasCards)
        {
            warrior.SetActive(true);
            mechanist.SetActive(true);
        }
    }
}
