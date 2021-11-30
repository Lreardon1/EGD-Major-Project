using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject player;
    public Image fade;
    float time = 0.0f;
    float interpolationPeriod = .15f;
    public TMPro.TextMeshProUGUI uitext;
    public RawImage text_back;
    public RawImage speaker;
    public TMPro.TextMeshProUGUI objective;

    IEnumerator waiter2()
    {
        yield return new WaitForSeconds(5);
        objective.text = "";
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(5);
        objective.text = "Find a way out";
        StartCoroutine(waiter2());
    }

    // Start is called before the first frame update
    void Start()
    {
        objective.text = "";
        player.GetComponent<OverworldMovement>().SetCanMove(false);
        StartCoroutine(waiter());

    }

    // Update is called once per frame
    void Update()
    {
        //fade in from black at start of tutorial
        Color c = fade.color;
        if(c.a > 0)
        {
            time += Time.deltaTime;
            if (time >= interpolationPeriod)
            {
                time = 0.0f;
                c.a = c.a - .20f;
                fade.color = c;
            }
        }
        else
        {
            player.GetComponent<OverworldMovement>().SetCanMove(true);
        }
    }
}
