using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Deck : MonoBehaviour
{
    [SerializeField]
    public GameObject offscreenPos;
    [SerializeField]
    public GameObject draggablePos;

    public static Deck instance;

    public List<GameObject> deck = new List<GameObject>();
    [SerializeField]
    public List<GameObject> starterDeck;
    [SerializeField]
    public List<GameObject> allCards;
    [HideInInspector]
    public List<GameObject> discard = new List<GameObject>();

    public Dictionary<string, List<GameObject>> freeDraggables = new Dictionary<string, List<GameObject>>();
    [SerializeField]
    public List<GameObject> numMods;
    [SerializeField]
    public List<GameObject> elemMods;
    [SerializeField]
    public List<GameObject> utilMods;

    public string sceneToLoad = "CustomizedCardTestScene";
    [SerializeField]
    public GameObject modifierPrefab;

    private DeckCustomizer deckCustomizer;

    IEnumerator LoadNextSceneAfterAllInits()
    {
        yield return null;
        SceneManager.LoadScene(sceneToLoad);
    }

    //enforcing singleton of deck on game start
    void Awake()
    {
        if (instance != this && instance != null)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(gameObject);

        //handle initial deck - CHANGE THIS ONCE ALL CARDS IN
        GameObject[] cards = new GameObject[starterDeck.Count];
        starterDeck.CopyTo(cards);
        deck.AddRange(cards);

        ModifierLookup.LoadModifierTable();
        foreach (GameObject card in allCards)
        {
            card.GetComponent<Card>().InitializeCard();
        }

        //initializing free draggables to start
        freeDraggables = new Dictionary<string, List<GameObject>>();
        freeDraggables["num"] = numMods;
        freeDraggables["element"] = elemMods;
        freeDraggables["utility"] = utilMods;

        StartCoroutine(LoadNextSceneAfterAllInits());
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown("m"))
    //    {
    //        print("attempting to save");
    //        SaveDeckAndModifiers("test");
    //    }

    //    if (Input.GetKeyDown("n"))
    //    {
    //        print("attempting to load");
    //        LoadDeckAndModifiers("test");
    //    }
    //}

    public GameObject Draw()
    {
        if (deck.Count == 0)
            return null;
        else
        {
            GameObject topDeck = deck[0];
            deck.RemoveAt(0);
            return topDeck;
        }
    }

    public void Discard(GameObject c)
    {
        discard.Add(c);
    }

    public void Shuffle()
    {
        deck.AddRange(discard);

        //shuffling based on Knuth shuffle algorithm
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int index = Random.Range(0, i);
            GameObject a = deck[index];
            deck[index] = deck[i];
            deck[i] = a;
        }

        discard.Clear();
    }

    public void ToggleButtons(bool isActive)
    {
        foreach (GameObject card in allCards)
        {
            card.GetComponent<Button>().enabled = isActive;
        }
    }

    public void HideCards()
    {
        foreach (GameObject card in allCards)
        {
            RectTransform trans = card.GetComponent<RectTransform>();
            trans.SetParent(offscreenPos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void HideCard(GameObject card)
    {
        if (allCards.Contains(card))
        {
            RectTransform trans = card.GetComponent<RectTransform>();
            trans.SetParent(offscreenPos.transform);
            trans.anchorMax = new Vector2(0.5f, 0.5f);
            trans.anchorMin = new Vector2(0.5f, 0.5f);
            trans.anchoredPosition = new Vector2(0.5f, 0.5f);
            trans.localPosition = new Vector3(0, 0, 0);
        }
    }

    public void SetDragger(GameObject dragger, bool isCustomization)
    {
        foreach (GameObject card in allCards)
        {
            card.GetComponent<DragDrop>().dragger = dragger;
        }
        if (isCustomization)
        {
            foreach (KeyValuePair<string, List<GameObject>> list in freeDraggables)
            {
                foreach (GameObject drag in list.Value)
                {
                    drag.GetComponent<DragDrop>().dragger = dragger;
                }
            }
        }
    }

    public void AddNewModifier(string type)
    {
        if (ModifierLookup.stringToSpriteConversionTable.ContainsKey(type))
        {
            Sprite newModS = ModifierLookup.stringToSpriteConversionTable[type];
            GameObject newMod = Instantiate(modifierPrefab, draggablePos.transform);
            newMod.GetComponent<Image>().sprite = newModS;
            Modifier.ModifierEnum modEnum = ModifierLookup.stringToType[type];
            newMod.GetComponent<DragDrop>().dropType = modEnum;
            //setting popup text
            newMod.GetComponent<ModifierPopUp>().LookUpText();

            if (modEnum == Modifier.ModifierEnum.NumModifier)
            {
                freeDraggables["num"].Add(newMod);
            }
            else if (modEnum == Modifier.ModifierEnum.SecondaryElement)
            {
                freeDraggables["element"].Add(newMod);
            }
            else if (modEnum == Modifier.ModifierEnum.Utility)
            {
                freeDraggables["utility"].Add(newMod);
            }

            if (deckCustomizer == null)
            {
                deckCustomizer = FindObjectOfType<DeckCustomizer>();
            }

            deckCustomizer.LinkNewMod(newMod, ModifierLookup.stringToType[type]);
        }
        else
        {
            print("ERROR: INVALID MODIFIER NAME: " + type);
        }
    }

    public void SaveDeckAndModifiers(string name)
    {
        string destination = Application.persistentDataPath + "/SavedDecks";
        FileStream file;
        if (!Directory.Exists(destination))
        {
            //creating new directory
            Directory.CreateDirectory(destination);
        }

        destination += "/deck_" + name + ".dat";
        if (File.Exists(destination))
        {
            File.Delete(destination);
            file = File.Create(destination);
        }
        else
        {
            print("create new file");
            file = File.Create(destination);
        }

        //handle writing new data
        DeckData data = new DeckData(deck, freeDraggables);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public void LoadDeckAndModifiers(string name)
    {
        string destination = Application.persistentDataPath + "/SavedDecks";
        FileStream file;
        if (!Directory.Exists(destination))
        {
            print("no custom decks saved");
            return;
        }

        destination += "/deck_" + name + ".dat";
        print(destination);
        if (File.Exists(destination))
        {
            print("reading data");
            file = File.OpenRead(destination);
        }
        else
        {
            print("no file to read");
            return;
        }

        //handle reading file data
        BinaryFormatter bf = new BinaryFormatter();
        DeckData data = (DeckData)bf.Deserialize(file);
        file.Close();

        //rebuilding deck based off read data
        List<string> cardData = data.deckStringData;
        deck.Clear();
        foreach (string card in cardData)
        {
            string[] splitCard = card.Split(',');
            int cardNum = int.Parse(splitCard[0]);
            GameObject currCard = allCards[cardNum];
            Card currCardS = currCard.GetComponent<Card>();
            CardEditHandler currCardEH = currCard.GetComponent<CardEditHandler>();
            //parsing modifiers and updating values
            for (int i = 1; i < splitCard.Length; i++)
            {
                GameObject modifierIcon = currCardS.modifiers[i-1].transform.GetChild(0).GetChild(0).gameObject;
                Modifier mod = currCardEH.activeModifiers[currCardS.modifiers[i - 1]];
                if (splitCard[i] != "") {
                    Sprite parsedModIcon = ModifierLookup.stringToSpriteConversionTable[splitCard[i]];
                    modifierIcon.GetComponent<Image>().sprite = parsedModIcon;
                    mod.DeactivateModifier(currCardS);
                    mod.setSpriteMod(parsedModIcon);
                    mod.ActivateModifier(currCardS);
                }
                else
                {
                    modifierIcon.GetComponent<Image>().sprite = currCardS.transparentSprite;
                    mod.DeactivateModifier(currCardS);
                    mod.setSpriteMod(null);
                }
            }
            deck.Add(currCard);
        }

        //removes any previous modifiers to make room for imported ones
        if (freeDraggables["num"] == null) { freeDraggables["num"] = new List<GameObject>(); }
        else { foreach (GameObject drag in freeDraggables["num"]) { Destroy(drag); } }
        if (freeDraggables["element"] == null) { freeDraggables["element"] = new List<GameObject>(); }
        else { foreach (GameObject drag in freeDraggables["element"]) { Destroy(drag); } }
        if (freeDraggables["utility"] == null) { freeDraggables["utility"] = new List<GameObject>(); }
        else { foreach (GameObject drag in freeDraggables["utility"]) { Destroy(drag); } }
        freeDraggables["num"].Clear();
        freeDraggables["element"].Clear();
        freeDraggables["utility"].Clear();
        List<string> draggableData = data.freeModStringData;
        GameObject modPrefab = allCards[0].GetComponent<CardEditHandler>().spriteEditor;
        foreach (string draggable in draggableData)
        {
            string[] splitDraggable = draggable.Split(',');
            GameObject drag = Instantiate(modPrefab, draggablePos.transform);
            drag.GetComponent<Image>().sprite = ModifierLookup.stringToSpriteConversionTable[splitDraggable[1]];
            freeDraggables[splitDraggable[0]].Add(drag);
        }

        if (deckCustomizer == null)
        {
            deckCustomizer = FindObjectOfType<DeckCustomizer>();
        }

        deckCustomizer.SetUp();
    }

    [System.Serializable]
    public class DeckData
    {
        public List<string> deckStringData;
        public List<string> freeModStringData;

        public DeckData(List<GameObject> deck, Dictionary<string, List<GameObject>> draggables)
        {
            deckStringData = new List<string>();
            //first copying over all cards in deck, using allCards order
            List<GameObject> allCards = Deck.instance.allCards;
            for (int i = 0; i < allCards.Count; i++)
            {
                //if that card exists, then insert a csv entry with all of that card's modifier data into the list
                if (deck.Contains(allCards[i]))
                {
                    string entry = i.ToString();
                    Dictionary<GameObject, Modifier> activeMods = allCards[i].GetComponent<CardEditHandler>().activeModifiers;
                    foreach (KeyValuePair<GameObject, Modifier> mod in activeMods)
                    {
                        entry += "," + mod.Value.spriteParsing;
                    }
                    deckStringData.Add(entry);
                }
            }

            freeModStringData = new List<string>();
            //then track all free draggables to ensure no extra draggables are dragged in
            foreach (KeyValuePair<string, List<GameObject>> dragCategory in draggables)
            {
                foreach (GameObject draggable in dragCategory.Value)
                {
                    string entry = dragCategory.Key;
                    entry += "," + ModifierLookup.spriteConversionTable[draggable.GetComponent<Image>().sprite];
                    freeModStringData.Add(entry);
                }
            }
        }
    }
}
