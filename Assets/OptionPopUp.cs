using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionPopUp : MonoBehaviour
{
    public Image p1;
    public Image p2;
    public TMPro.TextMeshProUGUI txt;

    public void SetVisibility(bool state)
    {
        if (!state)
        {
            var p1Color = p1.color;
            p1Color.a = 0f;
            p1.color = p1Color;
            p1Color = p2.color;
            p1Color.a = 0f;
            p2.color = p1Color;
            p1Color = txt.color;
            p1Color.a = 0f;
            txt.color = p1Color;
        }
        else
        {
            var p1Color = p1.color;
            p1Color.a = 1f;
            p1.color = p1Color;
            p1Color = p2.color;
            p1Color.a = 1f;
            p2.color = p1Color;
            p1Color = txt.color;
            p1Color.a = 1f;
            txt.color = p1Color;
        }
    }
}
