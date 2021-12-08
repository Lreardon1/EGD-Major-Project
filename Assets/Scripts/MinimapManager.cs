using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public float defaultMapSize = 10f;
    public float innerBoundPercent = 0.9f;
    public float waypointSize = 2;
    public OverworldMovement player;
    public Vector3 destination;
    public float dampenSpeed;
    public GameObject destinationPoint;
    public GameObject playerPoint;
    [SerializeField]
    public GameObject minimapAll;
    public RawImage visualMap;
    private Camera mapCam;

    public static MinimapManager instance;

    [Header("Destinations")]
    public Transform startingDestination;
    public Transform afterChurchDestination;
    public Transform afterCaveDestination;
    public Transform afterMeetupDestination;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        Vector3 playerPos = player.transform.position;
        playerPos.y = transform.position.y;
        this.transform.position = playerPos;
        destinationPoint.transform.localScale = new Vector3(waypointSize, waypointSize, waypointSize);
        mapCam = GetComponent<Camera>();

        if (PlayerPrefs.GetInt("warrior", 0) == 1)
            destination = afterMeetupDestination.position;
        else if (PlayerPrefs.GetInt("hasCards", 0) == 1)
            destination = afterCaveDestination.position;
        else if (PlayerPrefs.GetInt("priest", 0) == 1)
            destination = afterChurchDestination.position;
        else
            destination = startingDestination.position;
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
        minimapAll.SetActive(active);
        //visualMap.enabled = active;
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
