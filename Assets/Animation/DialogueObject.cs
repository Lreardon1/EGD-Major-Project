using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueObject", menuName = "ScriptableObjects/DialogueObject", order = 2)]
public class DialogueObject : ScriptableObject
{
    public string[] dialogue;
    public Texture[] images;
    public bool[] shouldProgresses;
}
