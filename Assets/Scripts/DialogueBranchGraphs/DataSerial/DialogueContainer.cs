using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLinkData> linkData;
    public List<DialogueNodeData> nodeData;
}
