using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LevelManager levelManager = (LevelManager)target;

        if (GUILayout.Button("Create Level JSON"))
        {
            levelManager.CreateLevelFile();
        }
    }
}
