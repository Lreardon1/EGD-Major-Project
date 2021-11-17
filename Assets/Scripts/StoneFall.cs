using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneFall : MonoBehaviour
{
    bool steppedOn = false;
    float time = 0.0f;
    float interpolationPeriod = 0.1f;

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(1);
        steppedOn = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player is the collision
        if (other.gameObject.name == "Player" && !steppedOn)
        {
            StartCoroutine(waiter());
        }
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time >= interpolationPeriod)
        {
            time = 0.0f;
            if (steppedOn && this.transform.position.y >= 25)
            {
                this.transform.Translate(0f, -2f, 0f);
            }
        }
        else if (steppedOn && this.transform.position.y < 25)
        {
            this.gameObject.SetActive(false);
        }
    }
}
