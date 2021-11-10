using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public Canvas canvas;
    public Canvas options;
    public Slider soundVolume;
    public AudioSource sound;

    public bool isPaused;

    private void Start()
    {
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        sound.volume = soundVolume.value;
        
    }

    public void PauseGame()
    {
        isPaused = !isPaused;
        canvas.gameObject.SetActive(!canvas.gameObject.active);
        options.gameObject.SetActive(false);
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void loadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
