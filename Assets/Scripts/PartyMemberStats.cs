using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberStats : MonoBehaviour
{
    [SerializeField]
    public List<Sprite> partySprites;
    [SerializeField]
    public List<GameObject> partyPrefabs;

    [SerializeField]
    public Image charImage;
    [SerializeField]
    public HealthbarController hpBar;
    [SerializeField]
    public GameObject statusListObj;
    [SerializeField]
    public List<GameObject> statNumDisplays;

    public static Dictionary<string, GameObject> combatPartyMembers = new Dictionary<string, GameObject>();

    public void UpdatePartyMember(string type, bool firstTime)
    {
        if (firstTime)
        {
            if (type == "priest")
            {
                charImage.sprite = partySprites[0];
                LoadFromScript(type, partyPrefabs[0]);
            }
            else if (type == "hunter")
            {
                charImage.sprite = partySprites[1];
                LoadFromScript(type, partyPrefabs[1]);
            }
            else if (type == "mechanist")
            {
                charImage.sprite = partySprites[2];
                LoadFromScript(type, partyPrefabs[2]);
            }
            else if (type == "warrior")
            {
                charImage.sprite = partySprites[3];
                LoadFromScript(type, partyPrefabs[3]);
            }

            SaveToPlayerPrefs(type);
        }

        if (combatPartyMembers.ContainsKey(type))
        {
            LoadFromScript(type, combatPartyMembers[type]);
        }
        else
        {
            LoadFromPlayerPrefs(type);
        }
    }

    /*
     * NAMES AND DESCRIPTIONS OF ALL SAVED PARTY STATS IN PLAYERPREFS (in the format of "type + name")
     * BaseAtk - base attack
     * BaseDef - base defense
     * BaseRes - base resistance
     * BaseSpd - base speed
     * BonusAtk - bonus attack
     * BonusDef - bonus defense
     * BonusRes - bonus resistance percentage
     * BonusSpd - bonus speed
     * MaxHealth - maximum health
     * CurrHealth - current health
     */
    public void LoadFromPlayerPrefs(string type)
    {
        GameObject baseNum, bonusNum;
        baseNum = statNumDisplays[0].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[0].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = PlayerPrefs.GetInt(type + "BaseAtk").ToString();
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + PlayerPrefs.GetInt(type + "BonusAtk").ToString() + ")";

        baseNum = statNumDisplays[1].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[1].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = PlayerPrefs.GetFloat(type + "BaseDef").ToString("0.0");
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + PlayerPrefs.GetFloat(type + "BonusDef").ToString("0.00") + ")";

        baseNum = statNumDisplays[2].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[2].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = PlayerPrefs.GetFloat(type + "BaseRes").ToString("0.0");
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + PlayerPrefs.GetFloat(type + "BonusRes").ToString("0.00") + ")";

        baseNum = statNumDisplays[3].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[3].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = PlayerPrefs.GetInt(type + "BaseSpd").ToString();
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + PlayerPrefs.GetInt(type + "BonusSpd").ToString() + ")";

        hpBar.SetMaxHealth(PlayerPrefs.GetInt(type + "MaxHealth"), PlayerPrefs.GetInt(type + "CurrHealth"));
        hpBar.SetHealth(PlayerPrefs.GetInt(type + "CurrHealth"));
    }

    public void LoadFromScript(string type, GameObject member)
    {
        GameObject baseNum, bonusNum;
        CombatantBasis memberBasis = member.GetComponent<CombatantBasis>();
        baseNum = statNumDisplays[0].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[0].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = memberBasis.attack.ToString();
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + (((memberBasis.attack + memberBasis.attackCardBonus) * memberBasis.attackMultiplier) - memberBasis.attack).ToString() + ")";

        baseNum = statNumDisplays[1].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[1].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = "1.0";
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + (memberBasis.defenseMultiplier - 1.0f).ToString("0.00") + ")";

        baseNum = statNumDisplays[2].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[2].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = "0.0";
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + (memberBasis.resistance).ToString("0.00") + ")";

        baseNum = statNumDisplays[3].transform.GetChild(1).gameObject;
        bonusNum = statNumDisplays[3].transform.GetChild(2).gameObject;
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = memberBasis.speed.ToString();
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + ((memberBasis.speed*memberBasis.speedMultiplier) - memberBasis.speed).ToString() + ")";

        hpBar.SetMaxHealth(memberBasis.totalHitPoints, memberBasis.currentHitPoints);
        hpBar.SetHealth(memberBasis.currentHitPoints);
    }

    public void SaveToPlayerPrefs(string type)
    {
        GameObject baseNum;
        baseNum = statNumDisplays[0].transform.GetChild(1).gameObject;
        PlayerPrefs.SetInt(type + "BaseAtk", int.Parse(baseNum.GetComponent<TMPro.TextMeshProUGUI>().text));
        PlayerPrefs.SetInt(type + "BonusAtk", 0);

        baseNum = statNumDisplays[1].transform.GetChild(1).gameObject;
        PlayerPrefs.SetFloat(type + "BaseDef", float.Parse(baseNum.GetComponent<TMPro.TextMeshProUGUI>().text));
        PlayerPrefs.SetFloat(type + "BonusDef", 0.0f);

        baseNum = statNumDisplays[2].transform.GetChild(1).gameObject;
        PlayerPrefs.SetFloat(type + "BaseRes", float.Parse(baseNum.GetComponent<TMPro.TextMeshProUGUI>().text));
        PlayerPrefs.SetFloat(type + "BonusRes", 0.0f);

        baseNum = statNumDisplays[3].transform.GetChild(1).gameObject;
        PlayerPrefs.SetInt(type + "BaseSpd", int.Parse(baseNum.GetComponent<TMPro.TextMeshProUGUI>().text));
        PlayerPrefs.SetInt(type + "BonusSpd", 0);

        PlayerPrefs.SetInt(type + "MaxHealth", (int)hpBar.slider.maxValue);
        PlayerPrefs.SetInt(type + "CurrHealth", (int)hpBar.slider.value);
    }
}
