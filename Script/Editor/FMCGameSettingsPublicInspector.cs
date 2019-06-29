using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMCGameSettingsPublic))]
public class FMCGameSettingsPublicInspector : Editor
///*
{
  public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Open fmc->Settings to edit me!", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("I should be called " + FMCGameSettingsBase.GetAssetName(FMCGameSettingsPublic.GetFullRelativePath()));
        EditorGUILayout.LabelField("I should be placed under " + FMCGameSettingsPublic.GetFullRelativePath());
        EditorGUILayout.LabelField("I will be included in the build!");
    }
}    