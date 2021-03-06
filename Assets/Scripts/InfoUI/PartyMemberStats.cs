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
    [SerializeField]
    public InfoPopUp specialPopUp;

    public static Dictionary<string, GameObject> combatPartyMembers = new Dictionary<string, GameObject>();

    public void UpdatePartyMember(string type, bool firstTime)
    {
        if (type == "priest")
        {
            charImage.sprite = partySprites[0];
            specialPopUp.title = "Blessing";
            specialPopUp.description = "This Special Action heals the entire party by 6 HP.";
        }
        else if (type == "hunter")
        {
            charImage.sprite = partySprites[1];
            specialPopUp.title = "Rain of Arrows";
            specialPopUp.description = "This Special Action deals damage to all enemies in combat based on a reduced percentage of her Attack stat.";
        }
        else if (type == "mechanist")
        {
            charImage.sprite = partySprites[2];
            specialPopUp.title = "Elemental Flurry";
            specialPopUp.description = "This Special Action inflicting a random negative status effect to all opponents or a positive status effect to all party members.";
        }
        else if (type == "warrior")
        {
            charImage.sprite = partySprites[3];
            specialPopUp.title = "Counterstance";
            specialPopUp.description = "This Special Action acts the same as the Block action, but allows for a counterattack without requiring an attack card to be played.";
        }

        if (firstTime)
        {
            if (type == "priest")
            {
                LoadFromScript(type, partyPrefabs[0]);
            }
            else if (type == "hunter")
            {
                LoadFromScript(type, partyPrefabs[1]);
            }
            else if (type == "mechanist")
            {
                LoadFromScript(type, partyPrefabs[2]);
            }
            else if (type == "warrior")
            {
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
        baseNum.GetComponent<TMPro.TextMeshProUGUI>().text = (PlayerPrefs.GetFloat(type + "BaseRes") * 100).ToString("0.0");
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + (PlayerPrefs.GetFloat(type + "BonusRes") * 100).ToString("0.00") + ")%";

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
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + (((memberBasis.attack + memberBasis.attackCardBonus) * memberBasis.attackMultiplier) - memberBasis.attack).ToString("0.00") + ")";

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
        bonusNum.GetComponent<TMPro.TextMeshProUGUI>().text = "+(" + ((memberBasis.speed*memberBasis.speedMultiplier) - memberBasis.speed).ToString("0.00") + ")";

        hpBar.SetMaxHealth(memberBasis.totalHitPoints, memberBasis.currentHitPoints);
        hpBar.SetHealth(memberBasis.currentHitPoints);
    }

    public void FullHeal(string type)
    {
        PlayerPrefs.SetInt(type + "CurrHealth", (int)hpBar.slider.maxValue);
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
        PlayerPrefs.SetFloat(type + "BaseRes", float.Parse(baseNum.GetComponent<TMPro.TextMeshProUGUI>().text) / 100);
        PlayerPrefs.SetFloat(type + "BonusRes", 0.0f);

        baseNum = statNumDisplays[3].transform.GetChild(1).gameObject;
        PlayerPrefs.SetInt(type + "BaseSpd", int.Parse(baseNum.GetComponent<TMPro.TextMeshProUGUI>().text));
        PlayerPrefs.SetInt(type + "BonusSpd", 0);

        PlayerPrefs.SetInt(type + "MaxHealth", (int)hpBar.slider.maxValue);
        PlayerPrefs.SetInt(type + "CurrHealth", (int)hpBar.slider.value);
    }
}
