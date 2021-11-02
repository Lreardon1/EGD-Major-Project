using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CombatantBasis.Status GetStatusResult(Card.Element primaryType, Card.Element secondaryType)
    {
        if (primaryType == Card.Element.None && secondaryType == Card.Element.None)
        {
            return CombatantBasis.Status.None;
        }

        switch (primaryType)
        {
            case Card.Element.Fire:
                switch(secondaryType)
                {
                    case Card.Element.Earth:
                        return CombatantBasis.Status.Molten;
                    case Card.Element.Water:
                        return CombatantBasis.Status.Vaporise;
                    case Card.Element.Air:
                        return CombatantBasis.Status.Lightning;
                    case Card.Element.Dark:
                        return CombatantBasis.Status.Hellfire;
                    default:
                        return CombatantBasis.Status.Burn;
                }
            case Card.Element.Water:
                switch (secondaryType)
                {
                    case Card.Element.Light:
                        return CombatantBasis.Status.HolyWater;
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Vaporise;
                    case Card.Element.Air:
                        return CombatantBasis.Status.Ice;
                    default:
                        return CombatantBasis.Status.Wet;
                }
            case Card.Element.Air:
                switch (secondaryType)
                {
                    case Card.Element.Water:
                        return CombatantBasis.Status.Ice;
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Lightning;
                    default:
                        return CombatantBasis.Status.Gust;
                }
            case Card.Element.Earth:
                switch (secondaryType)
                {
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Molten;
                    case Card.Element.Dark:
                        return CombatantBasis.Status.Corruption;
                    default:
                        return CombatantBasis.Status.Earthbound;
                }
            case Card.Element.Light:
                switch (secondaryType)
                {
                    case Card.Element.Water:
                        return CombatantBasis.Status.HolyWater;
                    case Card.Element.Dark:
                        return CombatantBasis.Status.Blight;
                    default:
                        return CombatantBasis.Status.Holy;
                }
            case Card.Element.Dark:
                switch (secondaryType)
                {
                    case Card.Element.Earth:
                        return CombatantBasis.Status.Corruption;
                    case Card.Element.Light:
                        return CombatantBasis.Status.Blight;
                    case Card.Element.Fire:
                        return CombatantBasis.Status.Hellfire;
                    default:
                        return CombatantBasis.Status.Fallen;
                }
        }

        return CombatantBasis.Status.None;
    }
}
