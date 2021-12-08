using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnteringBuildings : MonoBehaviour
{
    public int sceneToLoad;
    public Transform savedLoc;

    void OnTriggerEnter(Collider other)
    {
        //other.name should equal the root of your Player object
        if (other.name == "Player")
        {
            MemorySceneLoader.SetPlayerPosPref(savedLoc.position);

            //The scene number to load (in File->Build Settings)
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
