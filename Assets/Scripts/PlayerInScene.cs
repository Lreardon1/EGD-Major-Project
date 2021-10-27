using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInScene : MonoBehaviour
{
    public GameObject player;
    public List<UnityEngine.Vector3> record = new List<UnityEngine.Vector3>();
    int sceneNum = 0;
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        while (i < SceneManager.sceneCountInBuildSettings)
        {
            record.Add(Vector3.zero);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangePlayerPosition()
    {
        sceneNum = SceneManager.GetActiveScene().buildIndex;
        player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = record[sceneNum];
    }

    void RecordPosition()
    {
        sceneNum = SceneManager.GetActiveScene().buildIndex;
        player = GameObject.FindGameObjectWithTag("Player");
        record[sceneNum] = player.transform.position;
    }
}
