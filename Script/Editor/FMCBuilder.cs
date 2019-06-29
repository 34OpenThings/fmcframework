using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class FMCBuilder
{
    public const string BuildFolder = "_fmcbuilds/";

    //[MenuItem("FMC/Build Android + iOS")]
    static void BuildAndroidAndIOS()
    {
        BuildAndroid();
        BuildIOS();
        CreateFMCInfoFile();
    }

    //[MenuItem("FMC/Build Android")]
    static void BuildAndroid()
    {
        Build(BuildTarget.Android);
        CreateFMCInfoFile();
    }

    //[MenuItem("FMC/Build iOS")]
    static void BuildIOS()
    {
        Build(BuildTarget.iOS);
        CreateFMCInfoFile();
    }

    static void Build(BuildTarget target)
    {
        var gameSettings = FMCGameSettings.LoadOrCreateGameSettings();
        string iOSBuildPath = BuildFolder + gameSettings.Public.InternalGameName + "_xcode";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();

        if (target == BuildTarget.Android)
        {
            FillAndroidPublishingSettings();
            buildPlayerOptions.locationPathName = BuildFolder + gameSettings.Public.InternalGameName + ".apk";
        }
        else //iOS
            buildPlayerOptions.locationPathName = iOSBuildPath;

        buildPlayerOptions.target = target;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            if(target == BuildTarget.iOS)
            {
                ChangeXcodePlist(iOSBuildPath);
                Debug.Log("Updated Xcode plist file to declare it does not uses ecryption.");
            }
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    public static string[] GetBuildScenes()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        int i = 0;
        Debug.Log(EditorBuildSettings.scenes.Length + " scenes");

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes[i] = scene.path;

            i++;
            Debug.Log(scene.path);
        }
        return scenes;
    }

    static void CreateFMCInfoFile()
    {
        var gameSettings = FMCGameSettings.LoadOrCreateGameSettings();

        FileStream fs = new FileStream(BuildFolder + "fmcinfo.xml", FileMode.Create);
        XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
        w.Formatting = Formatting.Indented;

        w.WriteStartDocument();

        w.WriteStartElement("data");
        {
            w.WriteStartElement("gametitle");
            w.WriteString(gameSettings.Public.gameTitle);
            w.WriteEndElement();

            w.WriteStartElement("bundleid");
            w.WriteString(gameSettings.Public.BundleId);
            w.WriteEndElement();
        }
        w.WriteEndElement();

        w.WriteEndDocument();
        w.Flush();
        fs.Close();
    }

   // [MenuItem("FMC/Increment versions")]
    static void IncrementVersions()
    {
        int newVersion = PlayerSettings.Android.bundleVersionCode + 1;

        PlayerSettings.Android.bundleVersionCode = newVersion;
        PlayerSettings.iOS.buildNumber = newVersion.ToString();
        PlayerSettings.bundleVersion = "1.0." + newVersion;

        AssetDatabase.SaveAssets(); // should only be project version
    }

    [MenuItem("FMC/Fill Android publishing settings")]
    static void FillAndroidPublishingSettings()
    {
        var gameSettings = FMCGameSettings.LoadOrCreateGameSettings();
        if (File.Exists(gameSettings.Private.keystoreName))
        {
            PlayerSettings.Android.keystoreName = gameSettings.Private.keystoreName;
            PlayerSettings.Android.keystorePass = gameSettings.Private.keystorePass;
            PlayerSettings.Android.keyaliasName = gameSettings.Private.keyaliasName;
            PlayerSettings.Android.keyaliasPass = gameSettings.Private.keyaliasPass;
        }
        else
            Debug.Log("Keystore file not found, unable to sign!");
    }

    /*
     * When uploading to App store, Apple prevents you from starting tests if you don't tell them
     * whether you app uses encryption. This script will hard code a "no" answer into the plist file.
     */
    public static void ChangeXcodePlist(string pathToBuiltProject)
    {
        // Get plist file
        string plistPath = Path.Combine(pathToBuiltProject,"Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // add key
        var rootDict = plist.root;
        var key = "ITSAppUsesNonExemptEncryption";
        rootDict.SetString(key, "false");

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
