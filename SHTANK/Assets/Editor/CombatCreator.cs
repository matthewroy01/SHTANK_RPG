using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CombatCreator : EditorWindow
{
    string ActionName = "";
    string ActionDescription = "";
    List<Instruction> instructions;
    int listSize = 0;
    Instruction testInstruction;

    [MenuItem("SHTANK/CombatCreator")]
    public static void ShowWindow()
    {
         GetWindow<CombatCreator>("Combat Creator");
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

        testInstruction = (Instruction)EditorGUI.EnumPopup(new Rect(10, 65, position.width - 20, 20), testInstruction);

        if (GUILayout.Button("Create Action"))
        {
            CreateAsset();
        }


    }


    //Found on the Unify Community website http://wiki.unity3d.com/index.php/CreateScriptableObjectAsset
    public void CreateAsset()
    {
        Instruction test = testInstruction;
        CombatAction asset = ScriptableObject.CreateInstance<CombatAction>();
        asset.name = ActionName;
        asset.description = ActionDescription;
        asset.InstructionList = new List<char>();
        asset.InstructionList.Add(System.Convert.ToChar(test));

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets/Combat Actions";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(CombatAction).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
