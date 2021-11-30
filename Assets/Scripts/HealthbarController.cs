using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarController : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public TMPro.TextMeshProUGUI text;

    public Transform cam;

    public bool isPartyView = false;

    public void Start()
    {
        if (!isPartyView)
        {
            cam = FindObjectOfType<Camera>().transform;
        }
    }

    public void SetMaxHealth(int maxHealth, int currentHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;

        SetHealth(currentHealth);
    }

    public void SetHealth(int health)
    {
        if (health == 1) // For some reason when health is 0 the healthbar appears empty
            slider.value = 1.5f;
        else
            slider.value = health;

        text.text = health + "/" + (int)slider.maxValue;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void LateUpdate()
    {
        if (!isPartyView)
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
