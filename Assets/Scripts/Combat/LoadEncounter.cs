using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadEncounter : MonoBehaviour
{
    public Camera mainCam;
    public GameObject eventSystem;

    public Transform cameraParent;

    public Vector3 originalCameraPos = Vector3.zero;
    public Quaternion originalCameraRot = Quaternion.identity;

    public GameObject overWorldDragger;
    public OverworldMovement player;

    public float encounterRate = 0.2f;
    public bool canGetEncounter = false;
    public bool inEncounter = false;

    public float encounterCoolDown = 5f;
    public float timer = 0f;

    public List<GameObject> possibleEnemyPrefabs;
    
    
        // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
    }

    public IEnumerator CheckEncounter()
    {
        while(canGetEncounter && !inEncounter)
        {
            float random = Random.Range(0f, 1f);
            Debug.Log(random);

            if(random < encounterRate && timer <= 0f)
            {
                inEncounter = true;
                LoadCombatScene();
            }

            yield return new WaitForSeconds(1f);
        }

        yield return null;
    }

    public void LoadCombatScene()
    {
        player.canMove = false;
        eventSystem.SetActive(false);
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
        originalCameraPos = mainCam.transform.position;
        originalCameraRot = mainCam.transform.rotation;
        Invoke("SetUpCombatCamera", 0.1f);
    }

    public void SetUpCombatCamera()
    {
        CombatManager cm = FindObjectOfType<CombatManager>();
        cm.encounterScript = this;
        mainCam.transform.SetParent(null);
        cameraParent.gameObject.SetActive(false);
        mainCam.transform.position = cm.cameraPosition;
        mainCam.transform.rotation = cm.cameraRotation;
    }

    public void ReturnToOverWorld()
    {
        cameraParent.gameObject.SetActive(true);
        mainCam.transform.SetParent(cameraParent);
        SceneManager.UnloadSceneAsync("BattleScene");
        eventSystem.SetActive(true);
        mainCam.transform.position = originalCameraPos;
        mainCam.transform.rotation = originalCameraRot;

        Deck.instance.SetDragger(overWorldDragger, true);
        inEncounter = false;
        player.canMove = true;
        timer = encounterCoolDown;
        StartCoroutine("CheckEncounter");
    }

    public void OnTriggerStay(Collider other)
    {
        canGetEncounter = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        canGetEncounter = true;
        StartCoroutine("CheckEncounter");
    }

    public void OnTriggerExit(Collider other)
    {
        StopAllCoroutines();
        canGetEncounter = false;
    }
}
