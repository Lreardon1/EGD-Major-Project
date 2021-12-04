using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockFall : MonoBehaviour
{
    public GameObject rock;

    private void OnTriggerEnter(Collider other)
    {
        rock.SetActive(true);
    }
}
