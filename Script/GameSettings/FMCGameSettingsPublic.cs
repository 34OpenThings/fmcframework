using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This class holds all game settings shipped inside the build
/// </summary>
public class FMCGameSettingsPublic : FMCGameSettingsBase
{
    public static new string GetFullRelativePath() { return "Resources/FMCGameSettingsPublic.asset"; }
    public static FMCGameSettingsPublic GetGameSettings() { return Resources.Load<FMCGameSettingsPublic>(GetAssetName(GetFullRelativePath())); }

#if UNITY_EDITOR
    public static FMCGameSettingsPublic LoadOrCreateGameSettingsFile() { return LoadOrCreateGameSettingsFile<FMCGameSettingsPublic>(GetFullRelativePath()); }
#endif

    //************************* GENERAL SETTINGS

    public string[] scenesToBuild;

    public bool is2DGame = true;

    public string GamePath { get { return Path.GetDirectoryName(Application.dataPath); } }
    public string InternalGameName { get { return new DirectoryInfo(GamePath).Name.Trim(); } }
    public string BundleId { get { return bundleIdPrefix + InternalGameName; } }

    public string companyName = "MyGreatCompany";
    public string bundleIdPrefix = "com.mygreatcompany.";

    public string companyPrivacyPolicyURL = "";
    public string unityPrivacyPolicyURL = @"https://unity3d.com/legal/privacy-policy";
    public string googlePrivacyPolicyURL = @"https://policies.google.com/technologies/partner-sites";

    public string gameTitle = "My awesome game";
    public string splashScreenSceneGUID = null;
    public string gameSceneGUID = null;
    public string tutorialMessage = "";

    public Texture2D defaultIcon;
    public Texture2D androidIcon;
    public Texture2D iOSIcon;
    public Texture2D adaptiveIconBackground;
    public Texture2D adaptiveIconForeground;

    //************************* SPLASH SCREEN SETTINGS

    public float splashScreenDuration = 1f;

    public Sprite splashScreenBackground;
    public Sprite companyLogo;
    public Sprite gameLogo;

    //************************* ADVERTISEMENT & ANALYTICS
    public bool enableAds = false;
    public bool enableAnalytics = false;

    public bool AndroidAdMobUseTestIds = true;
    public string AndroidAppId = "";
    public string AndroidAdBannerUnitId = "";
    public string AndroidAdInterstitialUnitId = "";

    public bool iOSAdMobUseTestIds = true;
    public string iOSAppId = "";
    public string iOSAdBannerUnitId = "";
    public string iOSAdInterstitialUnitId = "";

    //************************* GAME DESIGN
    public bool enableLevelSystem = true;
    public int testLevel = 0;
    public bool quickLevelUp = false;
    public int maxLevels = 20;
    public FMCGrowth difficulty = new FMCGrowth("Difficulty", 1, .1f, .05f);
    public FMCGrowth levelDuration = new FMCGrowth("Level duration", 1000, 1.5f, .05f);
    public FMCGrowth singleScoreAtLevel = new FMCGrowth("Single score at level", 10, .2f, 0f);
}
