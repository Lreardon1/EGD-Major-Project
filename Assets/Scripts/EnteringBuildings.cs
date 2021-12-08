using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnteringBuildings : MonoBehaviour
{
    public int sceneToLoad;
    public Transform savedLoc;

    public bool requiresWholeParty = false;

    void OnTriggerEnter(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            if (requiresWholeParty && (PlayerPrefs.GetInt("warrior", 0) == 0 || PlayerPrefs.GetInt("hunter", 0) == 0))
                return;

            MemorySceneLoader.SetPlayerPosPref(savedLoc.position);

            //The scene number to load (in File->Build Settings)
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
