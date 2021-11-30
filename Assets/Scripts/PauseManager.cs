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
    public GameObject refWindow;
    public AudioSource sound;

    private Dictionary<string, GameObject> currPartyMembers = new Dictionary<string, GameObject>();
    [SerializeField]
    public GameObject partyMemberStatPrefab;
    [SerializeField]
    public GameObject partyView;

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

    public void AddPartyMember(string type)
    {
        GameObject newMember = Instantiate(partyMemberStatPrefab, partyView.transform);
        newMember.GetComponent<PartyMemberStats>().UpdatePartyMember(type);
        currPartyMembers.Add(type, newMember);
    }

    private void RefreshPartyView()
    {
        foreach (KeyValuePair<string, GameObject> partyMember in currPartyMembers)
        {
            partyMember.Value.GetComponent<PartyMemberStats>().UpdatePartyMember(partyMember.Key);
        }
    }

    public void SavePartyView()
    {
        foreach (KeyValuePair<string, GameObject> partyMember in currPartyMembers)
        {
            partyMember.Value.GetComponent<PartyMemberStats>().SaveToPlayerPrefs(partyMember.Key);
        }
    }

    public void LinkCombatant(string type, GameObject combatant)
    {
        PartyMemberStats.combatPartyMembers[type] = combatant;
        currPartyMembers[type].GetComponent<PartyMemberStats>().UpdatePartyMember(type);
    }

    public void RemoveCombatant(string type)
    {
        PartyMemberStats.combatPartyMembers.Remove(type);
    }

    public void PauseGame()
    {
        isPaused = !isPaused;
        canvas.gameObject.SetActive(!canvas.gameObject.active);
        options.gameObject.SetActive(false);
        refWindow.SetActive(false);
        if (isPaused)
        {
            Time.timeScale = 0f;
            RefreshPartyView();
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
