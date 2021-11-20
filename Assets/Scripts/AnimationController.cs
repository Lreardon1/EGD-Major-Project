using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AnimationController : MonoBehaviour
{
    private List<GameObject> disabledObjects = new List<GameObject>();

    [Header("Objects To Disable")]
    public GameObject[] ObjectsToDisable;
    [Header("Objects To Enable")]
    public GameObject[] ObjectsToEnable;

    [Header("Other items")]
    public Camera cam;
    public Image fadeImage;
    public GameObject dialoguePanel;
    public TMP_Text diaText;
    public RawImage diaImage;

    private int currentIndex = 0;
    private DialogueObject currentDialogue;

    public float fadeInTime = 0.6f;
    public float fadeOutTime = 0.6f;

    IEnumerator FadeToBlack(float time, Action A)
    {
        float t = 0;
        for (; t < time; t += Time.deltaTime)
        {
            fadeImage.color = new Color(0, 0, 0, t / time);
            yield return new WaitForEndOfFrame();
        }
        fadeImage.color = new Color(0, 0, 0, 1);
        A();
    }

    IEnumerator FadeToScene(float time, Action A)
    {
        float t = 0;
        for (; t < time; t += Time.deltaTime)
        {
            fadeImage.color = new Color(0, 0, 0, 1.0f - (t / time));
            print(t / time);
            yield return new WaitForEndOfFrame();
        }
        fadeImage.color = new Color(0, 0, 0, 0);
        A();
    }

    // Start is called before the first frame update
    public void PlayCutscene()
    {
        FindObjectOfType<OverworldMovement>().SetCanMove(false);
        StartCoroutine(FadeToBlack(fadeInTime, StartUp));
    }
    

    public void TriggerCutscene()
    {
        StartCoroutine(FadeToBlack(fadeInTime, StartUp));
    }

    private void StartUp()
    {
        

        disabledObjects.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        disabledObjects.AddRange(GameObject.FindGameObjectsWithTag("Party"));
        disabledObjects.AddRange(ObjectsToDisable);
        disabledObjects.Add(Camera.main.gameObject);

        foreach (GameObject d in disabledObjects)
            d.SetActive(false);
        foreach (GameObject e in ObjectsToEnable)
            e.SetActive(true);

        cam.gameObject.SetActive(true);

        StartCoroutine(FadeToScene(fadeOutTime, Sink));
        print("SENDING NEXT");
        GetComponent<Animator>().SetTrigger("Next");
    }

    public void Sink() { }

    private void DestroySelf() { Destroy(gameObject); }

    public void EndAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToBlack(fadeInTime, FinishUp));
    }

    private void FinishUp()
    {
        GetComponent<Animator>().SetTrigger("Exit");

        foreach (GameObject d in disabledObjects)
            d.SetActive(true);
        foreach (GameObject e in ObjectsToEnable)
            e.SetActive(false);
        for (int i = 0; i < transform.childCount; ++i)
            transform.GetChild(i).gameObject.SetActive(false);
        cam.gameObject.SetActive(false);

        FindObjectOfType<OverworldMovement>().SetCanMove(true);
        StartCoroutine(FadeToScene(fadeOutTime, DestroySelf));


        GetComponent<AfterCutsceneActions>()?.TakeActionsAfterCutscene();
    }


    public void ActivateText(DialogueObject texts)
    {
        print("Activated text");
        dialoguePanel.SetActive(true);
        currentDialogue = texts;
        currentIndex = 0;

        string dia = currentDialogue.dialogue[currentIndex];
        Texture image = currentDialogue.images[currentIndex];
        bool progressAnim = currentDialogue.shouldProgresses[currentIndex];

        diaText.text = dia;
        diaImage.texture = image;
        if (progressAnim)
        {
            GetComponent<Animator>().SetTrigger("Next");
        }
    }

    private void Update()
    {
        dialoguePanel.SetActive(currentDialogue != null);

        if (currentDialogue != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentIndex++;
                if (currentIndex >= currentDialogue.dialogue.Length)
                {
                    GetComponent<Animator>().SetTrigger("Next");
                    currentDialogue = null;
                    dialoguePanel.SetActive(false);
                    return;
                }

                string dia = currentDialogue.dialogue[currentIndex];
                Texture image = currentDialogue.images[currentIndex];
                bool progressAnim = currentDialogue.shouldProgresses[currentIndex];

                diaText.text = dia;
                diaImage.texture = image;
                if (progressAnim)
                {
                    GetComponent<Animator>().SetTrigger("Next");
                }
            }
        }
    }
}
