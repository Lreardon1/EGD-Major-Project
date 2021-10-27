using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyFollow : MonoBehaviour
{
    public List<UnityEngine.Vector3> movements = new List<UnityEngine.Vector3>();
    GameObject[] party_members;
    float elapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        movements.Add(GameObject.FindGameObjectWithTag("Player").transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        party_members = GameObject.FindGameObjectsWithTag("Party");
        elapsed += Time.deltaTime;
        if (elapsed >= 0.0166f)
        {
            Follow();
            elapsed = elapsed % 0.0166f;
            //need to do this because position vector has some wierd y values
            Vector3 tmp = new Vector3(GameObject.FindGameObjectWithTag("Player").transform.position.x, 0.200105f, GameObject.FindGameObjectWithTag("Player").transform.position.z);
            movements.Add(tmp);
        }
        Debug.Log(elapsed);
    }

    //for each object with the tag player, this function updates its position to be the same as the player after 1 second
    //unless the player has stopped
    void Follow()
    {
        party_members[0].transform.position = movements[0];
        movements.RemoveAt(0);
    }
}
