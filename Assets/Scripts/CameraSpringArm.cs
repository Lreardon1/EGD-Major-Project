using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpringArm : MonoBehaviour
{
    public Transform origin;
    public Transform cameraPos;
    public float armLength;
    public float sphereRadius;
    private Vector3 dir;
    
    // Update is called once per frame
    void Update()
    {
        dir = (cameraPos.position - origin.position).normalized;

        float desiredDist = armLength;
        if (Physics.SphereCast(origin.position, sphereRadius, dir, out RaycastHit hitInfo, 1000, ~LayerMask.GetMask("Character", "Trigger")))
        {
            if (!hitInfo.collider.isTrigger)
                desiredDist = Mathf.Max(0.2f, Mathf.Min(hitInfo.distance - 0.6f, desiredDist));
        }

        float dist = Mathf.Lerp((cameraPos.position - origin.position).magnitude, desiredDist, Time.deltaTime * 7);

        cameraPos.position = origin.position + dir * dist;
    }
}
