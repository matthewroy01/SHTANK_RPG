using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CombatCreator : EditorWindow
{
    string ActionName = "";
    string ActionDescription = "";
    public List<Instruction> instructions;

    [MenuItem("SHTANK/CombatCreator")]
    public static void ShowWindow()
    {
         GetWindow<CombatCreator>("Combat Creator");
    }

    //Called when the window is opened - Initalize variables here
    public void Awake()
    {
        instructions = new List<Instruction>();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        ActionName = EditorGUI.TextField(new Rect(10, 25, position.width - 20, 20),
            "Action Name:",
            ActionName);
        ActionDescription = EditorGUI.TextField(new Rect(10, 45, position.width - 20, 20),
           "Action Description:",
           ActionDescription);

        for(int i  = 0; i < instructions.Count; i++)
        {
            instructions[i] = (Instruction)EditorGUI.EnumPopup(new Rect(10, 85 + (20 * i), position.width - 20, 20), 
                              "Action " + (i+1).ToString(), instructions[i]);
        }

        if (GUI.Button(new Rect(10, 145, position.width - 20, 20), "Add instruction"))
        {
            instructions.Add(new Instruction());
        }
        if (GUI.Button(new Rect(10, 125, position.width - 20, 20), "Create Action"))
        {
            CreateAsset();
        }
    }

    public void CreateAsset()
    {
        //Set up new object variables
        CombatAction asset = ScriptableObject.CreateInstance<CombatAction>();
        asset.name = ActionName;
        asset.description = ActionDescription;
        asset.InstructionList = new List<char>();

        for(int i = 0; i < instructions.Count; i++)
        {
            asset.InstructionList.Add(System.Convert.ToChar(instructions[i]));
        }
       
        //Set the folder path
        string path = "Assets/Combat Actions";
        string assetPathAndName;
        if (ActionName == "")
        {
            assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(CombatAction).ToString() + ".asset");
        }
        else
        {
            assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + ActionName + ".asset");
        }
        
        //Create asset object
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
