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
    public MinimapManager minimap;
    [SerializeField]
    public PauseManager pauseManager;

    private PlayerInteraction playerInteraction;

    public GameObject player;

    void Start()
    {
        //PopUpNewModifiers();
    }

    public void ToggleEditOption(bool state)
    {
        print("in anim before: " + editOptionPopup.GetComponent<Animator>().GetBool("open"));
        print("set state: " + state);
        editOptionPopup.GetComponent<Animator>().SetBool("open", state);
    }

    public void PopUpNewModifiers()
    {
        newModPopup.GetComponent<Animator>().SetBool("open", true);
        StartCoroutine(CloseNewModPopUp());
    }

    IEnumerator CloseNewModPopUp()
    {
        yield return new WaitForSeconds(5);
        newModPopup.GetComponent<Animator>().SetBool("open", false);
    }

    public void OpenCustomization(PlayerInteraction pi)
    {
        pauseManager.PartyHeal();
        minimap.SetVisualActive(false);

        playerInteraction = pi;
        editOptionPopup.GetComponent<OptionPopUp>().SetVisibility(false);
        LockPlayer();
        customizationCanvas.SetActive(true);
        customizationCanvas.GetComponent<DeckCustomizer>().SetUp();
    }

    public void CloseCustomization()
    {
        UnlockPlayer();
        customizationCanvas.SetActive(false);
        editOptionPopup.GetComponent<OptionPopUp>().SetVisibility(true);
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
