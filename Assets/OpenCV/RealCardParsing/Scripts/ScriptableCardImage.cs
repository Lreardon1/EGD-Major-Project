using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "ScriptableObjects/ScriptableCardImage", order = 1)]
public class ScriptableCardImage : ScriptableObject
{
    public string cardName;
    public int cardID;
    public CardParser.CardElement cardElement;
    public CardParser.CardType cardType;

    public Texture2D cardTexture;
}
