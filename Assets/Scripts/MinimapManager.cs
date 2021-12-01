using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public float defaultMapSize = 10f;
    public float innerBoundPercent = 0.9f;
    public OverworldMovement player;
    public Vector3 destination;
    public float dampenSpeed;
    public GameObject destinationPoint;
    public GameObject playerPoint;
    public RawImage visualMap;
    private Camera mapCam;

    public static MinimapManager instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        Vector3 playerPos = player.transform.position;
        playerPos.y = transform.position.y;
        this.transform.position = playerPos;

        mapCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = Vector3.Lerp(transform.position, player.transform.position, dampenSpeed * Time.deltaTime);
        targetPos.y = transform.position.y;
        transform.position = targetPos;

        Vector3 oob = mapCam.WorldToViewportPoint(destination);
        oob.x = Mathf.Clamp(oob.x, 1.0f - innerBoundPercent, innerBoundPercent);
        oob.y = Mathf.Clamp(oob.y, 1.0f - innerBoundPercent, innerBoundPercent);
        oob.z = Mathf.Clamp(oob.z, 1.0f - innerBoundPercent, innerBoundPercent);
        print(oob);
        destinationPoint.transform.position = mapCam.ViewportToWorldPoint(oob);
        
        Vector3 visualPlayerMark = player.transform.position;
        visualPlayerMark.y = playerPoint.transform.position.y;
        playerPoint.transform.position = visualPlayerMark;

        float e = player.transform.rotation.eulerAngles.y;
        Vector3 myEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(myEuler.x, e, myEuler.z);
    }

    public void SetVisualActive(bool active)
    {
        visualMap.enabled = active;
    }

    public void SetTargetDestination(Vector3 t)
    {
        destination = t;
    }

    public float GetMapSize()
    {
        return mapCam.orthographicSize;
    }

    public void SetMapSize(float size)
    {
        mapCam.orthographicSize = size;
    }
}
