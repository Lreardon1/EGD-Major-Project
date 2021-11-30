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
    public bool debug = false;

    private void Start()
    {
        isPaused = false;

        //loading in all current party members
        if (PlayerPrefs.HasKey("priest"))
        {
            if (PlayerPrefs.GetInt("priest") == 1)
            {
                AddPartyMember("priest", false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("priest", 0);
        }

        if (PlayerPrefs.HasKey("hunter"))
        {
            if (PlayerPrefs.GetInt("hunter") == 1)
            {
                AddPartyMember("hunter", false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("hunter", 0);
        }

        if (PlayerPrefs.HasKey("mechanist"))
        {
            if (PlayerPrefs.GetInt("mechanist") == 1)
            {
                AddPartyMember("mechanist", false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("mechanist", 0);
        }

        if (PlayerPrefs.HasKey("warrior"))
        {
            if (PlayerPrefs.GetInt("warrior") == 1)
            {
                AddPartyMember("warrior", false);
            }
        }
        else
        {
            PlayerPrefs.SetInt("warrior", 0);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        sound.volume = soundVolume.value;
        
        //DEBUG
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (debug)
            {
                AddPartyMember("priest", true);
                AddPartyMember("hunter", true);
                AddPartyMember("mechanist", true);
                AddPartyMember("warrior", true);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayerPrefs.DeleteAll();
        }
    }

    public void AddPartyMember(string type, bool firstTime)
    {
        PlayerPrefs.SetInt(type, 1);
        GameObject newMember = Instantiate(partyMemberStatPrefab, partyView.transform);
        newMember.GetComponent<PartyMemberStats>().UpdatePartyMember(type, firstTime);
        currPartyMembers.Add(type, newMember);
    }

    public void RefreshPartyView()
    {
        foreach (KeyValuePair<string, GameObject> partyMember in currPartyMembers)
        {
            partyMember.Value.GetComponent<PartyMemberStats>().UpdatePartyMember(partyMember.Key, false);
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
        currPartyMembers[type].GetComponent<PartyMemberStats>().UpdatePartyMember(type, false);
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
