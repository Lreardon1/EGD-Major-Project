using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DeckSaver : MonoBehaviour
{
    public DeckCustomizer deckCustomizer;
    [SerializeField]
    public GameObject saveButton;
    [SerializeField]
    public TMPro.TMP_InputField playerInput;
    [SerializeField]
    public GameObject errorText;
    [SerializeField]
    public GameObject overwriteWindow;

    public void SetUp()
    {
        saveButton.GetComponent<Button>().interactable = false;
    }

    public void UpdateSaveButton()
    {
        if (DetermineValidName(playerInput.text))
        {
            saveButton.GetComponent<Button>().interactable = true;
            errorText.SetActive(false);
        }
        else
        {
            saveButton.GetComponent<Button>().interactable = false;
            errorText.SetActive(true);
        }
    }

    private bool DetermineValidName(string name)
    {
        if (name.Length > 30 || name.Length == 0)
        {
            return false;
        }
        foreach (char c in name)
        {
            //list of illegal characters
            if (c == '#') { return false; }
            else if (c == '%') { return false; }
            else if (c == '&') { return false; }
            else if (c == '{') { return false; }
            else if (c == '}') { return false; }
            else if (c == '\\') { return false; }
            else if (c == '<') { return false; }
            else if (c == '>') { return false; }
            else if (c == '*') { return false; }
            else if (c == '?') { return false; }
            else if (c == '/') { return false; }
            else if (c == '$') { return false; }
            else if (c == '!') { return false; }
            else if (c == '\'') { return false; }
            else if (c == '"') { return false; }
            else if (c == ':') { return false; }
            else if (c == '@') { return false; }
            else if (c == '+') { return false; }
            else if (c == '`') { return false; }
            else if (c == '|') { return false; }
            else if (c == '=') { return false; }
            else if (c == ' ') { return false; }
        }
        return true;
    }

    public void InitiateSave()
    {

        if (File.Exists(DeckLoader.FormFilePath(playerInput.text)))
        {
            ShowOverwriteWindow();
        }
        else
        {
            SubmitSave();
        }
    }

    private void SubmitSave()
    {
        deckCustomizer.AcceptAndStore();
        Deck.instance.SaveDeckAndModifiers(playerInput.text);
        CloseSaver();
    }

    public void OverrideAndSave()
    {
        CloseOverwriteWindow();
        SubmitSave();
    }

    public void Exit()
    {
        deckCustomizer.AcceptAndStore();
        CloseSaver();
    }

    public void CloseSaver()
    {
        playerInput.text = "";
        gameObject.SetActive(false);
    }

    public void ShowOverwriteWindow()
    {
        overwriteWindow.SetActive(true);
    }

    public void CloseOverwriteWindow()
    {
        overwriteWindow.SetActive(false);
    }
}
