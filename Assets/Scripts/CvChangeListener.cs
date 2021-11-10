using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CvChangeListener : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            CombatManager.IsInCVMode = !CombatManager.IsInCVMode;
            if (CombatManager.IsInCVMode)
                print("Switched to CV mode");
            else
                print("Switched to regular mode");
        }
    }
}
