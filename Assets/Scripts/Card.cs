using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Initial Settings")]
    //[SerializeField]
    public int manaCost = 0;
    //[SerializeField]
    public Image cardImage;
    //[SerializeField]
    public string cardStartText;

    public CardActionTemplate onPlayScript;

    List<string> cardText = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        onPlayScript.OnPlay();
    }

    public void Refresh()
    {

    }
}
