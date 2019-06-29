using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMCGameSettingsPrivate))]
public class FMCGameSettingPrivateInspector : Editor
///*
{
  public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Open fmc->Settings to edit me!", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("I should be called " + FMCGameSettingsBase.GetAssetName(FMCGameSettingsPrivate.GetFullRelativePath()));
        EditorGUILayout.LabelField("I should be placed under " + FMCGameSettingsPrivate.GetFullRelativePath());
        EditorGUILayout.LabelField("I will not be included in the build!");
    }
}    