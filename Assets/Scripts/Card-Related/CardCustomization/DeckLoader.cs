using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DeckLoader : MonoBehaviour
{
    public LoadableDeck selectedDeck = null;
    [SerializeField]
    public GameObject content;
    [SerializeField]
    public GameObject loadButton;
    [SerializeField]
    public GameObject loadableDeckPrefab;

    public void GenerateLoadableDecks()
    {
        loadButton.GetComponent<Button>().interactable = false;

        string saveLocation = Application.persistentDataPath + "/SavedDecks";
        if (!Directory.Exists(saveLocation))
        {
            return;
        }

        foreach (string fileName in Directory.GetFiles(saveLocation))
        {
            string deckName = IsolateName(fileName);
            GameObject loadable = Instantiate(loadableDeckPrefab, content.transform);
            LoadableDeck l = loadable.GetComponent<LoadableDeck>();
            l.dl = this;
            l.deckName = deckName;
            loadable.transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = deckName;
        }
    }

    public void InitiateLoad()
    {
        Deck.instance.LoadDeckAndModifiers(selectedDeck.deckName);
        CloseLoader();
    }

    public static string IsolateName(string path)
    {
        int nameIndex = path.IndexOf("deck_") + 5;
        int length = path.Length - 4 - nameIndex;
        return path.Substring(nameIndex, length);
    }

    public static string FormFilePath(string name)
    {
        return Application.persistentDataPath + "/SavedDecks/deck_" + name + ".dat";
    }

    public void CloseLoader()
    {
        //clearing our any loaded deck buttons
        int numButtons = content.transform.childCount;
        for (int i = 0; i < numButtons; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }
        selectedDeck = null;
        gameObject.SetActive(false);
    }
}
