using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    public DialogueGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        AddElement(GenerateEntryNode());
    }

    public Port GeneratePort(DialogueGraphNode node, Direction portDir, Port.Capacity cap = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDir, cap, typeof(int));
    }

    public DialogueGraphNode GenerateEntryNode()
    {
        var node = new DialogueGraphNode()
        {
            title = "ENTRY",
            GUID = Guid.NewGuid().ToString(),
            dialogueText = "ENTRY",
            entry = true
        };

        node.SetPosition(new Rect(100, 200, 100, 150));

        Port genPort = GeneratePort(node, Direction.Output);
        genPort.portName = "NEXT";
        node.outputContainer.Add(genPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        return node;
    }

    public DialogueGraphNode CreateDialogueNode(string nodeName)
    {
        var node = new DialogueGraphNode()
        {
            title = nodeName,
            GUID = Guid.NewGuid().ToString(),
            dialogueText = nodeName,
        };

        node.SetPosition(new Rect(Vector2.zero, defaultNodeSize));


        Port inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.outputContainer.Add(inputPort);

        Button button = new Button(() =>
        {
            AddChoicePort(node);
        });
        button.text = "New Branch";
        node.titleContainer.Add(button);

        var diaField = new TextField(string.Empty);
        diaField.RegisterValueChangedCallback(evt =>
        {
            node.dialogueText = evt.newValue;
            node.title = evt.newValue;
        });
        diaField.SetValueWithoutNotify(node.title);
        node.mainContainer.Add(diaField);

        diaField = new TextField(string.Empty);
        diaField.RegisterValueChangedCallback(evt =>
        {
            node.dialogueText = evt.newValue;
            node.title = evt.newValue;
        });
        diaField.SetValueWithoutNotify(node.title);
        node.mainContainer.Add(diaField);

        node.RefreshExpandedState();
        node.RefreshPorts();


        return node;
    }

    public void AddChoicePort(DialogueGraphNode node, string portName = null)
    {
        var genPort = GeneratePort(node, Direction.Output);

        var oldLabel = genPort.contentContainer.Q<Label>("type");
        genPort.contentContainer.Remove(oldLabel);

        var outputPortCount = node.outputContainer.Query("connector").ToList().Count;
        if (string.IsNullOrWhiteSpace(portName))
            genPort.portName = "Choice " + outputPortCount;
        else
            genPort.portName = portName;

        var textField = new TextField
        {
            name = string.Empty,
            value = genPort.portName
        };
        textField.RegisterValueChangedCallback(evt => genPort.portName = evt.newValue);
        genPort.contentContainer.Add(new Label("   "));
        genPort.contentContainer.Add(textField);

        var deletebutton = new Button(() => { RemovePort(node, genPort); }) { text = "X" };
        genPort.contentContainer.Add(deletebutton);


        node.outputContainer.Add(genPort);
    }

    private void RemovePort(DialogueGraphNode node, Port genPort)
    {
        var targetEdge = edges.ToList()
            .Where(x => x.output.portName == genPort.portName
            && x.output.node == genPort.node);

        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }
        node.outputContainer.Remove(genPort);
        node.RefreshExpandedState();
        node.RefreshPorts();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) => 
        {
            var portView = port;
            if (startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }
}
