using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class FbxToPrefabConverter : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;
    
    private readonly List<GameObject> _fbxObjects = new();
    private Label _fbxCountLabel;
    
    [MenuItem("Tools/Fbx To Prefab Converter")]
    public static void ShowWindow()
    {
        var converterWindow = CreateInstance<FbxToPrefabConverter>();
        converterWindow.ShowModal();
    }
        
    public void CreateGUI()
    {
        titleContent.text = "Fbx to Prefab Converter";
        visualTreeAsset.CloneTree(rootVisualElement);
        
        _fbxCountLabel = rootVisualElement.Q<Label>("FbxCountLabel");
        
        var clearButton = rootVisualElement.Q<Button>("ClearButton");
        clearButton.clicked += () => 
        {
            _fbxObjects.Clear();
            UpdateFbxCountText();
        } ;
        
        var dropArea = rootVisualElement.Q<VisualElement>("FbxDropArea");
        dropArea.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        });
        dropArea.RegisterCallback<DragPerformEvent>(evt =>
        {
            DragAndDrop.AcceptDrag();
            
            foreach (var objectReference in DragAndDrop.objectReferences)
            {
                if (objectReference is GameObject go && !_fbxObjects.Contains(go))
                {
                    _fbxObjects.Add(go);
                }
            }
            UpdateFbxCountText();
        });
        
        var createPrefabsButton = rootVisualElement.Q<Button>("CreatePrefabsButton");
        createPrefabsButton.clicked += ConvertFbxToPrefabs;
    }
    
    private void ConvertFbxToPrefabs()
    {
        var savePath = EditorUtility.OpenFolderPanel("Select folder to save prefabs", "", "");
        
        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }
        
        Debug.Log("Prefabs saved at: " + savePath);
        foreach (var gameObject in _fbxObjects)
        {
            PrefabUtility.SaveAsPrefabAsset(gameObject, savePath + "/" + gameObject.name + ".prefab");
        }
    }

    private void UpdateFbxCountText()
    {
        _fbxCountLabel.text = $"{_fbxObjects.Count} filed added";
    }
}