using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPopupSpawner : MonoBehaviour
{

    public Texture2D image;
    public string text;
    public GameObject prefab;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameObject popup = Instantiate(prefab, transform.position, Quaternion.identity);
            CombatPopup cPop = popup.GetComponent<CombatPopup>();
            cPop.Init(text, image, Color.white);
        }
    }
}
