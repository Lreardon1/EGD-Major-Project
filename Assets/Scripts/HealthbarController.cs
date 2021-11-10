using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarController : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public Transform cam;

    public void Start()
    {
        cam = FindObjectOfType<Camera>().transform;
    }

    public void SetMaxHealth(int maxHealth, int currentHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
