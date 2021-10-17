using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPS_Card : MonoBehaviour
{
    public enum CardType
    {
        Water,
        Fire,
        Nature
    }

    public enum Result
    {
        Win, 
        Loss, 
        Tie
    }

    public static Color GetColor(CardType type)
    {
        switch (type)
        {
            case CardType.Water:
                return Color.blue;
            case CardType.Fire:
                return Color.red;
            case CardType.Nature:
                return Color.green;
        }
        return Color.black;
    }

    public static Result GetPlayResult(RPS_Card card1, RPS_Card card2)
    {
        switch (card1.type)
        {
            case CardType.Water:
                if (card2.type == CardType.Fire)
                    return Result.Win;
                else if (card2.type == CardType.Nature)
                    return Result.Loss;
                break;
            case CardType.Fire:
                if (card2.type == CardType.Nature)
                    return Result.Win;
                else if (card2.type == CardType.Water)
                    return Result.Loss;
                break;
            case CardType.Nature:
                if (card2.type == CardType.Water)
                    return Result.Win;
                else if (card2.type == CardType.Fire)
                    return Result.Loss;
                break;
        }

        return Result.Tie;
    }

    public static Dictionary<CardType, int> CreateDeck(int fireCount, int waterCount, int natureCount)
    {
        Dictionary<CardType, int> cards = new Dictionary<CardType, int>();
        cards.Add(CardType.Fire, fireCount);
        cards.Add(CardType.Water, waterCount);
        cards.Add(CardType.Nature, natureCount);
        return cards;
    }
    
    public RPS_Card(CardType type)
    {
        this.type = type;
    }

    public CardType type;
}
