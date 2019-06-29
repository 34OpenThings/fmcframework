using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


public class FMCGameSettingsWindow : EditorWindow
{
    FMCGameSettings _target;
    SerializedObject _serializedObject;
    bool advancedOptionOpened = false;
    Vector2 scrollPos = Vector2.zero;

    [MenuItem("FMC/Settings")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FMCGameSettingsWindow)).titleContent = new GUIContent("FMC Settings");
    }

    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            FMCGameSettings.LoadOrCreateGameSettings(); //automatically creates new Scriptable Object if needed.
            //Debug.Log("FMC Settings initialized!");//Use it when debugging
        }
    }

    public FMCGameSettings Target
    {
        get
        {
            if (!FMCGameSettings.IsValid(_target))
                _target = FMCGameSettings.LoadOrCreateGameSettings();

            return _target;
        }
    }

    void OnGUI()
    {
        FMCGameSettings.LoadOrCreateGameSettings();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        //************************* GAME
        GUISpace();
        EditorGUILayout.LabelField("Game", EditorStyles.boldLabel);
        Target.Public.gameTitle = EditorGUILayout.TextField("Game title", Target.Public.gameTitle);
        Target.Public.tutorialMessage = EditorGUILayout.TextField("Tutorial message", Target.Public.tutorialMessage);
        Target.Public.gameSceneGUID = SceneField("Game scene", Target.Public.gameSceneGUID, fmc.game.GameSceneDefaultName);
        Target.Public.is2DGame = EditorGUILayout.Toggle("Is 2D game", Target.Public.is2DGame);
        Target.Public.gameLogo = SquareField("Game logo", Target.Public.gameLogo, null, 70);

        //************************* ICONS
        GUISpace();
        EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        Target.Public.defaultIcon = SquareField("Default", Target.Public.defaultIcon, null, 100);
        Target.Public.iOSIcon = SquareField("iOS", Target.Public.iOSIcon, Target.Public.defaultIcon);
        Target.Public.androidIcon = SquareField("Android", Target.Public.androidIcon, Target.Public.defaultIcon);
        GUILayout.BeginVertical();
        Target.Public.adaptiveIconBackground = SquareField("Adaptive Back", Target.Public.adaptiveIconBackground, Target.Public.defaultIcon);
        Target.Public.adaptiveIconForeground = SquareField("Adaptive Fore", Target.Public.adaptiveIconForeground, Target.Public.defaultIcon);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //************************* ADVERTISEMENT
        GUISpace(3);
        EditorGUILayout.LabelField("Advertisement", EditorStyles.boldLabel);

        Target.Public.enableAds = EditorGUILayout.Toggle("Enable Ads", Target.Public.enableAds);

        if (Target.Public.enableAds)
        {
            //Admob Android
            EditorGUILayout.LabelField("Admob - Android", EditorStyles.miniBoldLabel);
            Target.Public.AndroidAdMobUseTestIds = EditorGUILayout.Toggle("Use test ids", Target.Public.AndroidAdMobUseTestIds);
            EditorGUI.BeginDisabledGroup(Target.Public.AndroidAdMobUseTestIds);
            Target.Public.AndroidAppId = EditorGUILayout.TextField("App id", Target.Public.AndroidAppId);
            Target.Public.AndroidAdBannerUnitId = EditorGUILayout.TextField("Banner unit id", Target.Public.AndroidAdBannerUnitId);
            Target.Public.AndroidAdInterstitialUnitId = EditorGUILayout.TextField("Interstitials unit id", Target.Public.AndroidAdInterstitialUnitId);
            EditorGUI.EndDisabledGroup();

            //Admob iOS
            EditorGUILayout.LabelField("Admob - iOS", EditorStyles.miniBoldLabel);
            Target.Public.iOSAdMobUseTestIds = EditorGUILayout.Toggle("Use test ids", Target.Public.iOSAdMobUseTestIds);
            EditorGUI.BeginDisabledGroup(Target.Public.iOSAdMobUseTestIds);
            Target.Public.iOSAppId = EditorGUILayout.TextField("App id", Target.Public.iOSAppId);
            Target.Public.iOSAdBannerUnitId = EditorGUILayout.TextField("Banner unit id", Target.Public.iOSAdBannerUnitId);
            Target.Public.iOSAdInterstitialUnitId = EditorGUILayout.TextField("Interstitials unit id", Target.Public.iOSAdInterstitialUnitId);
            EditorGUI.EndDisabledGroup();

            if (!AssetDatabase.IsValidFolder("Assets/GoogleMobileAds"))
                EditorGUILayout.HelpBox("It looks like you don't have Admob. The project won't compile. Install the Admob Unity SDK.", MessageType.Warning);
            EditorGUILayout.HelpBox("Remember to enable also Unity Ads: FMC uses it for videos.", MessageType.Info);
        }

        GUISpace();
        EditorGUILayout.LabelField("Analytics", EditorStyles.boldLabel);
        Target.Public.enableAnalytics = EditorGUILayout.Toggle("Enable Analytics", Target.Public.enableAnalytics);
        if (Target.Public.enableAnalytics)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Firebase"))
                EditorGUILayout.HelpBox("It looks like you don't have Firebase. The project won't compile. Install the Firebase Unity SDK.", MessageType.Warning);
        }

        //************************* ADVANCED
        GUISpace();
        advancedOptionOpened = EditorGUILayout.Foldout(advancedOptionOpened, "Stuff you usually don't need to change", EditorStyles.foldout);
        if (advancedOptionOpened)
        {
            //************************* SPLASH SCREEN SETTINGS
            EditorGUILayout.LabelField("Splash screen", EditorStyles.boldLabel);
            Target.Public.splashScreenSceneGUID = SceneField("Splash screen scene", Target.Public.splashScreenSceneGUID, fmc.game.SplashScreenSceneName);
            Target.Public.splashScreenDuration = EditorGUILayout.FloatField("Duration of splash screen in seconds", Target.Public.splashScreenDuration);
            GUISpace();
            GUILayout.BeginHorizontal();
            Target.Public.splashScreenBackground = SquareField("Splash screen background", Target.Public.splashScreenBackground, null, 100);

            Target.Public.companyLogo = SquareField("Company logo", Target.Public.companyLogo, null, 100);
            if (!Target.Public.companyLogo)
                Target.Public.companyLogo = Resources.Load<Sprite>("companyLogo34");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //************************* BUILDING AND SHIPPING

            EditorGUILayout.LabelField("Building and shipping", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Local path", Target.Public.GamePath);
            EditorGUILayout.LabelField("Internal name", Target.Public.InternalGameName);
            Target.Public.bundleIdPrefix = EditorGUILayout.TextField("Prefix of bundle id", Target.Public.bundleIdPrefix);
            EditorGUILayout.LabelField("Bundle id (iOS+Android)", Target.Public.BundleId);
            Target.Public.companyName = EditorGUILayout.TextField("Company name", Target.Public.companyName);
            Target.Private.appleDeveloperTeamID = EditorGUILayout.TextField("Apple signing team ID", Target.Private.appleDeveloperTeamID);

            Target.Private.keystoreName = EditorGUILayout.TextField("Keystore name", Target.Private.keystoreName);
            Target.Private.keystorePass = EditorGUILayout.TextField("Keystore pass", Target.Private.keystorePass);
            Target.Private.keyaliasName = EditorGUILayout.TextField("Keystore alias name", Target.Private.keyaliasName);
            Target.Private.keyaliasPass = EditorGUILayout.TextField("Keystore pass", Target.Private.keyaliasPass);

            //************************* BUILDING AND SHIPPING
            GUISpace();
            EditorGUILayout.LabelField("Privacy policies", EditorStyles.boldLabel);
            Target.Public.companyPrivacyPolicyURL = EditorGUILayout.TextField("Company URL", Target.Public.companyPrivacyPolicyURL);
            Target.Public.unityPrivacyPolicyURL = EditorGUILayout.TextField("Unity URL", Target.Public.unityPrivacyPolicyURL);
            Target.Public.googlePrivacyPolicyURL = EditorGUILayout.TextField("Google URL", Target.Public.googlePrivacyPolicyURL);
        }

        /*
        SpriteField("Background", "backgroundAtlas", "backgroundSpriteName");
        SpriteField("Company logo", "companyAtlas", "companySpriteName");
        SpriteField("Game logo", "gameLogoAtlas", "gameLogoSpriteName");
        */

        GUISpace(5);
        EditorGUILayout.EndScrollView();

        UpdateUnitySettings(Target);
        Target.SetDirty();
    }

    /// <summary>
    /// This method is called from the fmc toolkit. 
    /// It is used to create the game settings object and to set the unity settings accordingly.
    /// </summary>
    public static void UpdateUnitySettingsFromToolkit()
    {
        FMCGameSettings Target = FMCGameSettings.LoadOrCreateGameSettings();
        UpdateUnitySettings(Target);
    }

    public static void FirstProjectLaunch()
    {
        if (!FindObjectOfType<FMCGameManager>())
        {
            new GameObject("FMC").AddComponent<FMCGameManager>();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), fmc.game.GameSceneDefaultPath);

            FMCGameSettings Target = FMCGameSettings.LoadOrCreateGameSettings();

            //Searching scenes in project
            if (string.IsNullOrEmpty(Target.Public.gameSceneGUID))
            {
                SceneAsset s = GetSceneFromGUID(null, fmc.game.GameSceneDefaultName);
                if (s)
                    Target.Public.gameSceneGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(s));
            }

            if (string.IsNullOrEmpty(Target.Public.splashScreenSceneGUID))
            {
                SceneAsset s = GetSceneFromGUID(null, fmc.game.SplashScreenSceneName);
                if (s)
                    Target.Public.splashScreenSceneGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(s));
            }

            UpdateUnitySettings(Target);
        }
    }

    public static void UpdateUnitySettings(FMCGameSettings Target)
    {
        List<string> scenePaths = new List<string>();
        string splashScreenScenePath = AssetDatabase.GUIDToAssetPath(Target.Public.splashScreenSceneGUID);
        string gameScenePath = AssetDatabase.GUIDToAssetPath(Target.Public.gameSceneGUID);
        if (!fmc.utils.IsNullOrWhiteSpace(splashScreenScenePath)) scenePaths.Add(splashScreenScenePath);
        if (!fmc.utils.IsNullOrWhiteSpace(gameScenePath)) scenePaths.Add(gameScenePath);
        SetEditorBuildSettingsScenes(scenePaths);

        SetAllIcons(Target.Public.defaultIcon, Target.Public.iOSIcon, Target.Public.androidIcon, Target.Public.adaptiveIconBackground, Target.Public.adaptiveIconForeground);

        PlayerSettings.companyName = Target.Public.companyName;
        PlayerSettings.productName = Target.Public.gameTitle;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait; //Currently FMC is for portrait games

        EditorSettings.defaultBehaviorMode = Target.Public.is2DGame ? EditorBehaviorMode.Mode2D : EditorBehaviorMode.Mode3D;

        PlayerSettings.iOS.appleDeveloperTeamID = Target.Private.appleDeveloperTeamID;

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, Target.Public.BundleId);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, Target.Public.BundleId);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, Target.Public.BundleId);

        //Updating define symbols
        string symbols = "";
        if (Target.Public.enableAds)
            symbols += FMCAds.PrecompileDirectiveName + ";";
        if (Target.Public.enableAnalytics)
            symbols += FMCAnalytics.PrecompileDirectiveName + ";";
        SetScriptingDefineSymbols(symbols);
    }

    private static SceneAsset GetSceneFromGUID(string sceneGUID, string nameToSearchIfGUIDIsInvalid)
    {
        SceneAsset gameScene = null;
        if (!fmc.utils.IsNullOrWhiteSpace(sceneGUID))
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGUID);
            gameScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        if (gameScene == null)
            gameScene = SearchSceneByName(nameToSearchIfGUIDIsInvalid);

        return gameScene;
    }

    public static SceneAsset SearchSceneByName(string sceneName)
    {
        //Search scene by name
        var sceneGUIDs = AssetDatabase.FindAssets("t:scene " + sceneName);
        List<SceneAsset> scenes = new List<SceneAsset>();

        foreach (string g in sceneGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                scenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(path));
        }

        if (scenes.Count > 1)
            Debug.LogWarning("\"" + sceneName + "\" search found " + scenes.Count + " matches! Use a more specific search!");

        return scenes.Count > 0 ? scenes[0] : null;
    }

    public static void SetEditorBuildSettingsScenes(List<string> scenePaths)
    {
        // Find valid Scene paths and make a list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        foreach (var sceneAsset in scenePaths)
        {
            if (!string.IsNullOrEmpty(sceneAsset))
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneAsset, true));
        }

        // Set the Build Settings window Scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }

    static void SetAllIcons(Texture2D defaultIcon, Texture2D iOSIcon, Texture2D androidIcon, Texture2D androidAdaptiveIconBackground, Texture2D androidAdaptiveIconForeground)
    {
        if (iOSIcon == null) iOSIcon = defaultIcon;
        if (androidIcon == null) androidIcon = defaultIcon;
        if (androidAdaptiveIconBackground == null) androidAdaptiveIconBackground = defaultIcon;
        if (androidAdaptiveIconForeground == null) androidAdaptiveIconForeground = defaultIcon;

        // IOS
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { defaultIcon });
        SetIcons(BuildTargetGroup.iOS, UnityEditor.iOS.iOSPlatformIconKind.Application, iOSIcon);
        SetIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Legacy, androidIcon);
        SetIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Round, androidIcon);
        SetIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Adaptive, androidAdaptiveIconBackground, androidAdaptiveIconForeground);
    }

    static void SetIcons(BuildTargetGroup group, PlatformIconKind kind, params Texture2D[] icons)
    {
        var platIcons = PlayerSettings.GetPlatformIcons(group, kind);
        for (var i = 0; i < platIcons.Length; i++) //Assign textures to each available icon slot.
            platIcons[i].SetTextures(icons);
        PlayerSettings.SetPlatformIcons(group, kind, platIcons);
    }

    static void SetScriptingDefineSymbols(string symbols)
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbols);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbols);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
    }

    //Helper function used to add the given amount of space
    void GUISpace(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
            EditorGUILayout.Space();
    }

    private string SceneField(string title, string sceneGUID, string nameToSearchIfGUIDIsInvalid)
    {
        GUILayout.BeginHorizontal();
        var style = new GUIStyle(GUI.skin.label);
        style.fixedWidth = 146;
        GUILayout.Label(title, style);

        SceneAsset gameScene = GetSceneFromGUID(sceneGUID, nameToSearchIfGUIDIsInvalid);

        gameScene = EditorGUILayout.ObjectField(gameScene, typeof(SceneAsset), false) as SceneAsset;
        GUILayout.EndHorizontal();

        return gameScene ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(gameScene)) : null;
    }

    private static T SquareField<T>(string name, T objToChange, T defaultValue, int dimension = 60) where T : Object
    {
        T usedTexture = objToChange ? objToChange : defaultValue;
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.LowerCenter;
        style.wordWrap = true;
        style.fixedWidth = dimension;
        style.fixedHeight = style.lineHeight * 2;
        GUILayout.Label(name, style);

        var result = (T)EditorGUILayout.ObjectField(usedTexture, typeof(T), false, GUILayout.Width(dimension), GUILayout.Height(dimension));
        GUILayout.EndVertical();

        if (result != defaultValue)
            return result;
        else
            return null;
    }
}