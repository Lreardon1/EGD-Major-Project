using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtil
{
    private DialogueGraphView graphView;
    private DialogueContainer container;


    private List<Edge> Edges => graphView.edges.ToList();
    private List<DialogueGraphNode> Nodes => graphView.nodes.ToList().Cast<DialogueGraphNode>().ToList();



    public static GraphSaveUtil GetInstance(DialogueGraphView targetView)
    {
        return new GraphSaveUtil() { graphView = targetView };
    }

    public void SaveGraph(string filename)
    {
        if (!Edges.Any()) return;

        var container = ScriptableObject.CreateInstance<DialogueContainer>();

        var connected = Edges.Where(x => x.input.node != null).ToArray();
        for (var i = 0; i < connected.Length; ++i)
        {
            var outputNode = connected[i].output.node as DialogueGraphNode;
            var inputNode = connected[i].input.node as DialogueGraphNode;

            container.linkData.Add(new NodeLinkData() {
                baseGUID = outputNode.GUID,
                TargetGUID = inputNode.GUID,
                portName = connected[i].output.portName });
        }

        foreach (var node in Nodes.Where(node => !node.entry))
        {
            container.nodeData.Add(new DialogueNodeData
            {
                GUID = node.GUID,
                Text = node.dialogueText,
                pos = node.GetPosition().position
            });
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources/RPS_Dialogue"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "RPS_Dialogue");
        }

        AssetDatabase.CreateAsset(container, $"Assets/Resources/RPS_Dialogue/{filename}.asset");
    }

    public void LoadGraph(string filename)
    {
        container = Resources.Load<DialogueContainer>($"RPS_Dialogue/{filename}"); // TODO
        if (container == null)
        {
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; ++i)
        {
            var connections = container.linkData.Where(x => x.baseGUID == Nodes[i].GUID).ToList();
            for (int j = 0; j < connections.Count; ++j)
            {
                var targetNodeGUID = connections[j].TargetGUID;
                var target = Nodes.First(x => x.GUID == targetNodeGUID);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) target.inputContainer[0]);

                target.SetPosition(new Rect(container.nodeData.First(x => x.GUID == targetNodeGUID).pos,
                    graphView.defaultNodeSize));
            }
        }   
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        graphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in container.nodeData)
        {
            var tempNode = graphView.CreateDialogueNode(nodeData.Text);
            tempNode.GUID = nodeData.GUID;
            graphView.AddElement(tempNode);


            var nodePorts = container.linkData.Where(x => x.baseGUID == nodeData.GUID).ToList();
            nodePorts.ForEach(x => graphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        Nodes.Find(x => x.entry).GUID = container.linkData[0].baseGUID;

        foreach (var node in Nodes)
        {
            if (node.entry) continue;
            Edges.Where(x => x.input.node == node).ToList()
                .ForEach(edge => graphView.RemoveElement(edge));

            graphView.RemoveElement(node);
        }
    }
}
