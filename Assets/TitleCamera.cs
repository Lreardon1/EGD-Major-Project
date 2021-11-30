using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject topCamera;
    public GameObject player;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
        mainCamera.SetActive(true);
        topCamera.SetActive(false);
        player.GetComponent<OverworldMovement>().SetCanMove(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player")
        {
            mainCamera.SetActive(false);
            topCamera.SetActive(true);
            player.GetComponent<OverworldMovement>().SetCanMove(false);

            StartCoroutine(waiter());
        }
    }
}
