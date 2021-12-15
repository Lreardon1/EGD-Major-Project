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
    public GameObject UI;
    [Header("Actors To Activate")]
    public AnimatedCharacter[] animatedCharacters;

    [Header("Other items")]
    public Camera cam;
    public Image fadeImage;
    public GameObject dialoguePanel;
    public TMP_Text diaText;
    public TMP_Text diaSpeaker;
    public RawImage diaImage;


    private int currentIndex = 0;
    private string[] parsedDialogue;
    public TextAsset dialogue;

    public string[] speakerNames;
    public Texture[] speakerSprites;
    public Dictionary<string, Texture> speakers = new Dictionary<string, Texture>();

    public float startFadeInTime = 0.6f;
    public float startFadeOutTime = 0.6f;
    public float endFadeInTime = 0.6f;
    public float endFadeOutTime = 0.6f;

    // Format:  "line ID", "speaker name", "line", "display sprite A", "display sprite b, unused"
    // OFormat: "anim", "anim ID", "continue without"

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
        parsedDialogue = dialogue.text.Split('\n');
        currentIndex = 0;

        FindObjectOfType<OverworldMovement>().SetCanMove(false);
        UI.SetActive(true);
        StartCoroutine(FadeToBlack(startFadeInTime, StartUp));
    }
    

    public void TriggerCutscene()
    {
        StartCoroutine(FadeToBlack(startFadeInTime, StartUp));
    }

    private void StartUp()
    {
        speakers.Clear();
        for (int i = 0; i < speakerNames.Length; ++i)
        {
            speakers.Add(speakerNames[i], speakerSprites[i]);
        }

        disabledObjects.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        disabledObjects.AddRange(GameObject.FindGameObjectsWithTag("Party"));
        disabledObjects.AddRange(ObjectsToDisable);
        //disabledObjects.Add(Camera.main.gameObject);

        foreach (GameObject d in disabledObjects)
            d.SetActive(false);
        foreach (GameObject e in ObjectsToEnable)
            e.SetActive(true);
        foreach (AnimatedCharacter c in animatedCharacters) c.enabled = true;
        cam.gameObject.SetActive(true);

        StartCoroutine(FadeToScene(startFadeOutTime, () => {
            StartCoroutine(ManageCutscene(dialogue.text.Split('\n')));
        }));
    }




    private void DestroySelf() { Destroy(gameObject); }

    public void EndAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToBlack(endFadeInTime, FinishUp));
    }

    private void FinishUp()
    {
        GetComponent<Animator>().SetTrigger("Exit");

        foreach (GameObject d in disabledObjects)
            d.SetActive(true);
        foreach (GameObject e in ObjectsToEnable)
            e.SetActive(false);
        foreach (AnimatedCharacter c in animatedCharacters) c.enabled = false;

        for (int i = 0; i < transform.childCount; ++i)
            transform.GetChild(i).gameObject.SetActive(false);
        cam.gameObject.SetActive(false);

        GetComponent<AfterCutsceneActions>()?.TakeActionsAfterCutscene();

        StartCoroutine(FadeToScene(endFadeOutTime, () => 
        {
            FindObjectOfType<OverworldMovement>().SetCanMove(true);
            DestroySelf();
        }));
    }









    private class TextAction
    {
        public int lineID;
        public string speakerName;
        public string line;
        public Texture displayTexture;
    }

    private class AnimAction
    {
        public string animName;
        public bool continueDuring;
    }

    private string[] CVS_Split(string stringToSplit)
    {
        int i = 0;
        bool insideQuotes = false;

        List<string> collected = new List<string>();

        while (i < stringToSplit.Length)
        {
            if (stringToSplit[i] == ',' && !insideQuotes)
            {
                collected.Add(stringToSplit.Substring(0, i));
                stringToSplit = stringToSplit.Substring(i + 1);
                i = -1;
            } else if (stringToSplit[i] == '"')
            {
                insideQuotes = !insideQuotes;
            }

            ++i;
        }

        if (stringToSplit.Length > 0)
            collected.Add(stringToSplit);
        return collected.ToArray();
    }

    private bool ParseTextAction(string action, out TextAction textAction)
    {
        textAction = new TextAction();

        string[] actionElements = CVS_Split(action);
        print(actionElements);
        foreach (string s in actionElements)
            print(s);

        for (int i = 0; i < actionElements.Length; ++i)
            actionElements[i] = actionElements[i].Trim(' ', '"', '\'', '\r', '\n');

        if (!int.TryParse(actionElements[0], out textAction.lineID))
            return false;

        textAction.speakerName = actionElements[1];
        textAction.line = actionElements[2];

        if (!speakers.TryGetValue(actionElements[3], out textAction.displayTexture))
            return false;

        return true;
    }

    private bool ParseAnimAction(string action, out AnimAction animAction)
    {
        animAction = new AnimAction();
        string[] actionElements = CVS_Split(action);
        for (int i = 0; i < actionElements.Length; ++i)
            actionElements[i] = actionElements[i].Trim(' ', '"', '\'', '\r', '\n');


        if (actionElements[0].ToLower() != "anim")
            return false;

        animAction.animName = actionElements[1];

        animAction.continueDuring = actionElements[2].ToLower() == "true";

        return true;
    }

    IEnumerator ManageCutscene(string[] actions)
    {
        currentIndex = 0;
        TextAction textAction;
        AnimAction animAction;


        while (currentIndex < actions.Length)
        {
            string currentAction = actions[currentIndex];

            bool isOnText = ParseTextAction(currentAction, out textAction);
            if (!ParseAnimAction(currentAction, out animAction) && !isOnText)
                throw new Exception("ERROR: INVALID TEXT PROVIDED.");

            if (isOnText)
            {
                dialoguePanel.SetActive(true);

                diaSpeaker.text = textAction.speakerName;
                diaText.text = textAction.line;
                diaImage.texture = textAction.displayTexture;

                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(ContinueFromTextInput);
            }
            else
            {
                dialoguePanel.SetActive(false);
                GetComponent<Animator>().Play(animAction.animName);

                yield return new WaitForSeconds(0.05f);
                if (!animAction.continueDuring)
                    yield return new WaitUntil(ContinueFromAnim);
            }

            currentIndex++;

            foreach (AnimatedCharacter c in animatedCharacters) c.enabled = true;
        }
        // Format:  "line ID", "speaker name", "line", "display sprite A", "display sprite b, unused"
        // OFormat: "anim", "anim ID", "continue without"

        EndAnimation();

        yield break;
    }

    private bool ContinueFromAnim()
    {
        Animator animator = GetComponent<Animator>();
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.1f;
    }

    private bool ContinueFromTextInput()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }
}
