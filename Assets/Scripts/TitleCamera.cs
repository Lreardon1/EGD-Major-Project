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
    public GameObject cameraRef;

    bool completed = false;

    IEnumerator waiter(float timeLerp, float timeWait)
    {
        Vector3 originalPos = mainCamera.transform.position;
        Quaternion originalRot = mainCamera.transform.rotation;
        Vector3 endPos = cameraRef.transform.position;
        Quaternion endRot = cameraRef.transform.rotation;
        cameraRef.transform.position = originalPos;
        cameraRef.transform.rotation = originalRot;

        mainCamera.SetActive(false);
        cameraRef.SetActive(true);

        float t = 0;
        while (t < timeLerp)
        {
            float l = t / timeLerp;
            cameraRef.transform.position = Vector3.Lerp(originalPos, endPos, l);
            cameraRef.transform.rotation = Quaternion.Slerp(originalRot, endRot, l);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }


        cameraRef.transform.position = Vector3.Lerp(originalPos, endPos, 1);
        cameraRef.transform.rotation = Quaternion.Slerp(originalRot, endRot, 1);
        yield return new WaitForSeconds(timeWait);

        t = 0;
        while (t < timeLerp)
        {
            float l = 1.0f - (t / timeLerp);
            cameraRef.transform.position = Vector3.Lerp(originalPos, endPos, l);
            cameraRef.transform.rotation = Quaternion.Slerp(originalRot, endRot, l);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        cameraRef.transform.position = Vector3.Lerp(originalPos, endPos, 1.0f);
        cameraRef.transform.rotation = Quaternion.Slerp(originalRot, endRot, 1.0f);
        mainCamera.transform.position = originalPos;
        mainCamera.transform.rotation = originalRot;
        
        completed = true;

        mainCamera.SetActive(true);
        cameraRef.SetActive(false);
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
            // anim.Play();
            player.GetComponent<OverworldMovement>().SetCanMove(false);
            StartCoroutine(waiter(1.2f, 3.4f));
        }
    }
}
