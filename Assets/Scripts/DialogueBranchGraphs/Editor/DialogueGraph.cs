using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private string fileName;
    private DialogueGraphView graphView; 

    [MenuItem("Graph/Graph Dialogue Window")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("RPS Dialogue Graph");
    }


    private void OnEnable()
    {
        ConstructGraphView();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView { name = "Dialogue Graph" };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);

        GenToolBar();
    }

    private void GenToolBar()
    {
        var tb = new Toolbar();

        var fileNameField = new TextField("File Name:");
        fileNameField.SetValueWithoutNotify("New RPS Dialogue");
        fileNameField.MarkDirtyRepaint();
        fileNameField.RegisterValueChangedCallback(evt => { fileName = evt.newValue; });
        tb.Add(fileNameField);

        tb.Add(new Button(() => SaveData()) { text = "Save Data" });
        tb.Add(new Button(() => LoadData()) { text = "Load Data" });

        Button nodeCreateButton = new Button(() =>
        {
            var node = graphView.CreateDialogueNode("Dia Node");
            graphView.AddElement(node);
        });

        nodeCreateButton.text = "Create Node";
        tb.Add(nodeCreateButton);

        rootVisualElement.Add(tb);
    }

    private void LoadData()
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name", "Enter a valid file name", "OK");
        }

        var saveUtil = GraphSaveUtil.GetInstance(graphView);

        saveUtil.LoadGraph(fileName);
    }

    private void SaveData()
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name", "Enter a valid file name", "OK");
        }

        var saveUtil = GraphSaveUtil.GetInstance(graphView);

        saveUtil.SaveGraph(fileName);
    }
}
