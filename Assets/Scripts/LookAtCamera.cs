using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Camera cameraToLookAt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
       transform.LookAt(cameraToLookAt.transform);
    }
}
