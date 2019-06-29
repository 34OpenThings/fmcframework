using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This data will not be included in the build.
/// This should never be used when not in UNITY_EDITOR
/// </summary>
public class FMCGameSettingsPrivate : FMCGameSettingsBase   
{
#if UNITY_EDITOR
    public static new string GetFullRelativePath() { return "Editor/Resources/FMCGameSettingsPrivate.asset"; }
    public static FMCGameSettingsPrivate LoadOrCreateGameSettingsFile() { return LoadOrCreateGameSettingsFile<FMCGameSettingsPrivate>(GetFullRelativePath()); }
#endif

    public string appleDeveloperTeamID = "";

    public string keystoreName = "fmc.keystore";
    public string keystorePass = "";
    public string keyaliasName = "fmc";
    public string keyaliasPass = "";
}
