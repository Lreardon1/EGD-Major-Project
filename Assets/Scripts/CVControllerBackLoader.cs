using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CVControllerBackLoader : MonoBehaviour
{
    [Header("Controller")]
    public CombatManager cm;
    public GameObject cvPanel;
    public GameObject regularPanel;
    [Header("Controller Images")]
    public RawImage planeImage;
    public RawImage goodSeeImage;
    public RawImage stickerImage1;
    public RawImage stickerImage2;
    public RawImage stickerImage3;
    public Image progressIndicator;
    [Header("Controller Text")]
    public TMP_Text playText;
    public TMP_Text cardText;

    public GameObject cvManagerPrefab;

    IEnumerator StartCV()
    {
        GameObject go = Instantiate(cvManagerPrefab);
        CardParserManager cmp = go.GetComponent<CardParserManager>();
        yield return new WaitForEndOfFrame();
        cmp.ActivateCVForCombat(this);
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (CombatManager.IsInCVMode && CardParserManager.instance != null)
        {
            CardParserManager.instance.ActivateCVForCombat(this);
            cvPanel.SetActive(true);
            regularPanel.SetActive(false);
        }
        else if (CombatManager.IsInCVMode)
        {
            StartCoroutine(StartCV());
        }
    }
}
