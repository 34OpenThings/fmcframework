using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Defines a linear growth that goes to infinity.
/// The growth factor defines the steepness of the line.
/// The growth perturbation adds a sin value to the equation, making the growth a curve. 
/// NOTE: The perturbation is always additive, so the game will get harder by increasing it.
/// </summary>
[System.Serializable]
public class FMCGrowth
{
    public string name = "Growth";
    public float firstLevelValue = 1000;
    public float growthFactor = .1f;
    public float growthPerturbation = .1f;

    public FMCGrowth(string name, float firstLevelValue, float growthFactor, float growthPerturbation)
    {
        this.name = name;
        this.firstLevelValue = firstLevelValue;
        this.growthFactor = growthFactor;
        this.growthPerturbation = growthPerturbation;
    }

    public int GetValue(int level)
    {
        return (int)GetFloatValue(level);
    }

    public float GetFloatValue (int level)
    {
        float x = ((float)level) - 1;
        return firstLevelValue * (1 + growthFactor * x * (1 + (Mathf.Sin(x) + 1) * growthPerturbation));
    }
}

/// <summary>
/// Contains both Public and Private Settings. Use in Editor.
/// </summary>
public class FMCGameSettings
{
    public FMCGameSettingsPublic Public { get; protected set; }
    public FMCGameSettingsPrivate Private { get; protected set; }

    protected FMCGameSettings(FMCGameSettingsPublic publicSettings, FMCGameSettingsPrivate privateSettings)
    {
        Public = publicSettings;
        Private = privateSettings;
    }

    public static bool IsValid(FMCGameSettings settings)
    {
        return settings != null && settings.Public && settings.Private;
    }

#if UNITY_EDITOR

    public static FMCGameSettings LoadOrCreateGameSettings()
    {
        return new FMCGameSettings(FMCGameSettingsPublic.LoadOrCreateGameSettingsFile(), FMCGameSettingsPrivate.LoadOrCreateGameSettingsFile());
    }

    public void SetDirty()
    {
        EditorUtility.SetDirty(Public);
        EditorUtility.SetDirty(Private);
    }
#endif
}

public class FMCGameSettingsBase : ScriptableObject
{
    public static string GetFullRelativePath() { return "Resources/FMCGameSettings.asset"; }

    public static string[] GetFolders(string relativePath)
    {
        return Path.GetDirectoryName(relativePath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/')
            .Split('/');
    }
    public static string GetAssetName(string relativePath) { return Path.GetFileNameWithoutExtension(relativePath); }
    public static string GetFileName(string relativePath) { return Path.GetFileName(relativePath); }

#if UNITY_EDITOR
    /// <summary>
    /// Creates the Scriptable Object asset.
    /// </summary>
    protected static T LoadOrCreateGameSettingsFile <T>(string relativePath) where T : FMCGameSettingsBase
    {
        T target = Resources.Load<T>(GetAssetName(relativePath));
        if (target == null)
        {//No asset found inside project, creating a new one...
            T asset = CreateInstance<T>();

            //Let's create path through Unity's very unintelligent way of doing it
            string containingPath = "Assets";
            string[] folders = GetFolders(relativePath);
            foreach (string folder in folders)
            {
                string folderPath = Path.Combine(containingPath, folder);
                if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
                    UnityEditor.AssetDatabase.CreateFolder(containingPath, folder);
                containingPath = folderPath;
            }

            UnityEditor.AssetDatabase.CreateAsset(asset, Path.Combine("Assets", relativePath));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            target = asset;

            Debug.Log("Created new settings file under: " + relativePath);
        }

        return target;
    }
#endif
}
