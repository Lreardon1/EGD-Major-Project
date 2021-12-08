using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField]
    public GameObject customizationCanvas;
    [SerializeField]
    public GameObject editOptionPopup;
    [SerializeField]
    public GameObject newModPopup;
    [SerializeField]
    public GameObject minimapPopup;
    [SerializeField]
    public MinimapManager minimap;
    [SerializeField]
    public PauseManager pauseManager;

    private PlayerInteraction playerInteraction;

    public GameObject player;

    void Start()
    {
        print(PlayerPrefs.GetInt("OnStart"));
        if (!PlayerPrefs.HasKey("OnStart") || PlayerPrefs.GetInt("OnStart") == 0)
        {
            PopUpFollowMap();
            PlayerPrefs.SetInt("OnStart", 1);
        }
    }

    public void ToggleEditOption(bool state)
    {
        if (state)
        {
            editOptionPopup.GetComponent<Animator>().SetBool("close", false);
            editOptionPopup.GetComponent<Animator>().SetBool("open", true);
        }
        else
        {
            editOptionPopup.GetComponent<Animator>().SetBool("close", true);
            editOptionPopup.GetComponent<Animator>().SetBool("open", false);
        }
    }

    public void PopUpNewModifiers()
    {
        newModPopup.GetComponent<Animator>().SetBool("open", true);
        StartCoroutine(ClosePopUp(newModPopup, 5f));
    }

    public void PopUpFollowMap()
    {
        minimapPopup.GetComponent<Animator>().SetBool("open", true);
        StartCoroutine(ClosePopUp(minimapPopup, 8f));
    }

    IEnumerator ClosePopUp(GameObject pop, float time)
    {
        yield return new WaitForSeconds(time);
        pop.GetComponent<Animator>().SetBool("open", false);
    }

    public void OpenCustomization(PlayerInteraction pi)
    {
        pauseManager.PartyHeal();
        minimap.SetVisualActive(false);

        playerInteraction = pi;
        editOptionPopup.SetActive(false);
        LockPlayer();
        customizationCanvas.SetActive(true);
        customizationCanvas.GetComponent<DeckCustomizer>().SetUp();
    }

    public void CloseCustomization()
    {
        UnlockPlayer();
        customizationCanvas.SetActive(false);
        editOptionPopup.SetActive(true);
        playerInteraction.canEditCards = true;

        minimap.SetVisualActive(true);
    }

    public void LockPlayer()
    {
        player.GetComponent<OverworldMovement>().canMove = false;
    }

    public void UnlockPlayer()
    {
        player.GetComponent<OverworldMovement>().canMove = true;
    }
}
