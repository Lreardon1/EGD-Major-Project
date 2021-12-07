using UnityEngine;

[CreateAssetMenu(fileName = "CardSticker", menuName = "ScriptableObjects/ScriptableCardSticker", order = 1)]
public class ScriptableCardSticker : ScriptableObject
{
    public string stickerName;
    public string stickerSubType;

    public int stickerID;
    public Modifier.ModifierEnum modEnum;

    public Texture2D stickerTexture;
}
