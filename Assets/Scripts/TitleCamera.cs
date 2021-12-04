using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    public GameObject mainCamera;
    //public GameObject topCamera;
    public Animation anim;
    public GameObject player;
    bool completed = false;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
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
            //mainCamera.SetActive(false);
            //topCamera.SetActive(true);
            mainCamera.transform.position = new Vector3(-424.5829162597656f, 8.026437759399414f, 397.9913635253906f);
            anim.Play();
            player.GetComponent<OverworldMovement>().SetCanMove(false);
            StartCoroutine(waiter());
        }
    }
}
