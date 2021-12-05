using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    Vector3 original;
    //public GameObject topCamera;
    public Animation anim;
    public GameObject player;
    bool completed = false;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
        player.transform.position = original;
        //mainCamera.SetActive(true);
        //topCamera.SetActive(false);
        completed = true;
        player.GetComponent<OverworldMovement>().SetCanMove(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player" && !completed)
        {
            original = player.transform.position;
            //mainCamera.SetActive(false);
            //topCamera.SetActive(true);
            //player.transform.position = new Vector3(-420.331f, 5.309f, 398.698f);
            anim.Play();
            player.GetComponent<OverworldMovement>().SetCanMove(false);
            StartCoroutine(waiter());
        }
    }
}
