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
    public GameObject overwriteWindow;

    public void SetUp()
    {
        saveButton.GetComponent<Button>().interactable = false;
    }

    public void UpdateSaveButton()
    {
        if (playerInput.text == "")
        {
            saveButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            saveButton.GetComponent<Button>().interactable = true;
        }
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
