using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    Vector3 original;
    //public GameObject topCamera;
    public GameObject mainCamera;
    public Animation anim;
    public GameObject player;
    bool completed = false;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
        //mainCamera.transform.position = original;
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
            //original = mainCamera.transform.position;
            //mainCamera.SetActive(false);
            //topCamera.SetActive(true);
            //mainCamera.transform.position = new Vector3(-379.284912109375f, 8.02044677734375f, 398.00335693359377f);
            anim.Play();
            player.GetComponent<OverworldMovement>().SetCanMove(false);
            StartCoroutine(waiter());
        }
    }
}
