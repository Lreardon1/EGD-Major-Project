using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemorySceneLoader : MonoBehaviour
{

    public static void LoadFromOverworld(string scene, Vector3 pos)
    {
        SetPlayerPosPref(pos);
        SceneManager.LoadScene(scene);
    }

    public static void LoadToOverworld(string oldScene)
    {
        PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "_LoadPos1");
        PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "_LoadPos2");
        PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "_LoadPos3");

        SceneManager.LoadScene("ForestAndVillage");
    }
    

    public static void SetPlayerPosPref(Vector3 pos)
    {
        PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "_LoadPos1", pos.x);
        PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "_LoadPos2", pos.y);
        PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "_LoadPos3", pos.z);
    }

    public static Vector3 GetPlayerPosPrefForScene()
    {
        float x = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "_LoadPos1", 0.0f);
        float y = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "_LoadPos2", 0.0f);
        float z = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "_LoadPos3", 0.0f);

        return new Vector3(x, y, z);
    }

    internal static bool HasScenePos()
    {
        return PlayerPrefs.HasKey(SceneManager.GetActiveScene().name + "_LoadPos1");
    }
}
