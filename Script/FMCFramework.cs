using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMCFramework
{
    //Singleton instance
    static FMCFramework _instance;
    public static FMCFramework Instance
    {
        get
        {
            if (_instance == null)
                new FMCFramework();
            return _instance;
        }
        set { _instance = value; }
    }

    public FMCGameInstance GameInstance { get; protected set; }
    public FMCGameSettingsPublic GameSettings { get; protected set; }
    public FMCSaveSystem SaveSystem { get; protected set; }

    public FMCAds Ads { get; protected set; }
    public FMCAnalytics Analytics { get; protected set; }

    public FMCFramework()
    {
        _instance = this;

        GameSettings = FMCGameSettingsPublic.GetGameSettings();
        if (GameSettings == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("FMC: unable to find game settings. Creating a new settings asset...");
            FMCGameSettings.LoadOrCreateGameSettings();
#else
            Debug.LogError("FMC error: unable to find game settings. Check FMC settings.");
#endif
        }

        GameInstance = new FMCGameInstance();
        SaveSystem = new FMCSaveSystem();

        Ads = new FMCAds();

#if ACTIVATE_FIREBASE
        if(GameSettings.enableAnalytics)
            Analytics = new FMCFirebaseAnalytics();
        else
#endif
        {
            Analytics = new FMCTestAnalytics();
        }

        //Binding game events to analytics
        fmc.events.ResetGame += () => { fmc.analytics.Event(fmc.analytics.EventName_ResetGame); };
        fmc.events.GameStarted += () => { fmc.analytics.Event(fmc.analytics.EventName_GameStarted); };
        fmc.events.NewBestScore += (long score) => { fmc.analytics.Event(fmc.analytics.EventName_NewBestScore, fmc.analytics.EventParamName_Score, score); };
        fmc.events.GameOver += (long score) => { fmc.analytics.Event(fmc.analytics.EventName_GameOver, fmc.analytics.EventParamName_Score, score); };
        fmc.events.ResumedFromGameOver += () => { fmc.analytics.Event(fmc.analytics.EventName_ResumeFromGameOver); };
        fmc.events.LevelUp += (int newLevel) => { fmc.analytics.Event(fmc.analytics.EventName_LevelUp); };

        if (fmc.game.Settings.enableLevelSystem)
        {
            if (fmc.game.Settings.testLevel > 0)
            {
                fmc.game.Level = fmc.game.Settings.testLevel;
                fmc.game.Experience = 30;
            }
        }
        else
        {
            fmc.game.Level = 1;
            fmc.game.Experience = 0;
        }
    }
}

/// <summary>
/// No-brainer wrapper of every framework function, plus some utils
/// </summary>
public static class fmc
{
    #region Game functions

    public static class game
    {
        /// <summary>
        /// Gets the game settings
        /// </summary>
        public static FMCGameSettingsPublic Settings { get { return FMCFramework.Instance.GameSettings; } }

        public const string GamePropertyName_Score = "fmc_score";
        public const string GamePropertyName_BestScore = "fmc_best_score";
        public const string GamePropertyName_Level = "fmc_level";
        public const string GamePropertyName_Experience = "fmc_experience";

        //Game properties
        public const string GDPRPromptedPropertyName = "fmc_is_gdpr_prompted";
        public const string WantsPersonalizedAdsPropertyName = "fmc_wants_personalized_ads";

        public const string SplashScreenSceneName = "FMCSplashScreen";
        public const string GameSceneDefaultName = "GameScene";
        public const string GameSceneDefaultPath = "Assets/_content/" + GameSceneDefaultName + ".unity";

        public static FMCGameState GameState { get { return FMCFramework.Instance.GameInstance.GameState; } }

        /// <summary>
        /// Get and set game score during the game.
        /// </summary>
        public static int CurrentScore
        {
            get { return FMCFramework.Instance.GameInstance.GetProperty(GamePropertyName_Score, 0); }
        }

        /// <summary>
        /// Score some points. The amount of points given is editable on the game design tool and depends on the current level.
        /// Use the multiplier to give more points.
        /// The function returns the actual number of points given.
        /// </summary>
        public static int Score(float multiplier = 1)
        {
            float points = Settings.singleScoreAtLevel.GetValue(Level);
            points *= multiplier;
            int actualPoints = Mathf.CeilToInt(points);
            FMCFramework.Instance.GameInstance.SetProperty(GamePropertyName_Score, CurrentScore + actualPoints);
            return actualPoints;
        }

        /// <summary>
        /// Game best score. Automatically saved on disk.
        /// </summary>
        public static int BestScore
        {
            get { return FMCFramework.Instance.GameInstance.GetProperty(GamePropertyName_BestScore, 0); }
            set { FMCFramework.Instance.GameInstance.SetProperty(GamePropertyName_BestScore, value, true); }
        }

        /// <summary>
        /// Current level. Automatically saved on disk.
        /// </summary>
        public static int Level
        {
            get { return FMCFramework.Instance.GameInstance.GetProperty(GamePropertyName_Level, 1); }
            set { FMCFramework.Instance.GameInstance.SetProperty(GamePropertyName_Level, value, true); }
        }

        /// <summary>
        /// Current experience, meaning the amount of points gained from the last level.
        /// Will be reset to 0 when hitting a new level.
        /// </summary>
        public static int Experience
        {
            get { return FMCFramework.Instance.GameInstance.GetProperty(GamePropertyName_Experience, 0); }
            set { FMCFramework.Instance.GameInstance.SetProperty(GamePropertyName_Experience, value, true); }
        }

        /// <summary>
        /// Clears the score and changes the state of the game to Reset.
        /// Attach to fmc.events.ResetGame to handle a callback
        /// </summary>
        public static void ResetGame()
        {
            FMCFramework.Instance.GameInstance.SetProperty(GamePropertyName_Score, 0);
            FMCFramework.Instance.GameInstance.ResetGame();
        }

        /// <summary>
        /// Starts the game. Does not set the score back to zero.
        /// Can also be used to handle revives or second chances.
        /// </summary>
        public static void StartGame()
        {
            FMCFramework.Instance.GameInstance.GoToState(FMCGameState.Playing);
        }

        /// <summary>
        /// Compares score with best score and performs the appropriate calls to analytics.
        /// Does not determine the end of the game: there could be a revive or second chance. 
        /// Use GameOver() and ResetGame() to properly reset the game.
        /// </summary>
        public static void GameOver()
        {
            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                if (events.NewBestScore != null) events.NewBestScore.Invoke(CurrentScore);
            }

            FMCFramework.Instance.GameInstance.GoToState(FMCGameState.GameOver);
        }

        /// <summary>
        /// Changes the state to TitleScreen
        /// </summary>
        public static void GoToTitleScreen()
        {
            FMCFramework.Instance.GameInstance.GoToState(FMCGameState.TitleScreen);
        }
    }

    public static class events
    {
        /// <summary>
        /// Hook this to properly reset all gameplay-related values.
        /// Called when a new game is going to start
        /// </summary>
        public static Action ResetGame
        {
            get { return FMCFramework.Instance.GameInstance.ResetGameAction; }
            set { FMCFramework.Instance.GameInstance.ResetGameAction = value; }
        }

        /// <summary>
        /// Called when the game is starting. 
        /// This can also happen after a GameOver, when a second chance is given:
        /// do not assume that this means that a new game is starting!
        /// </summary>
        public static Action GameStarted
        {
            get { return FMCFramework.Instance.GameInstance.GameStartedAction; }
            set { FMCFramework.Instance.GameInstance.GameStartedAction = value; }
        }

        /// <summary>
        /// Called after a GameOver, when the score of the last game was higher than
        /// the current best score.
        /// </summary>
        public static Action<long> NewBestScore
        {
            get { return FMCFramework.Instance.GameInstance.NewBestAction; }
            set { FMCFramework.Instance.GameInstance.NewBestAction = value; }
        }

        /// <summary>
        /// Called when the game ends.
        /// This does not imply that the game will be reset, a second chance could be given!
        /// </summary>
        public static Action<long> GameOver
        {
            get { return FMCFramework.Instance.GameInstance.GameOverAction; }
            set { FMCFramework.Instance.GameInstance.GameOverAction = value; }
        }

        /// <summary>
        /// Called after a game over when a second chance is given.
        /// </summary>
        public static Action ResumedFromGameOver
        {
            get { return FMCFramework.Instance.GameInstance.ResumedFromGameOverAction; }
            set { FMCFramework.Instance.GameInstance.ResumedFromGameOverAction = value; }
        }

        /// <summary>
        /// Called when the player is going back to title screen
        /// </summary>
        /// <returns></returns>
        public static Action GoingToTitleScreen
        {
            get { return FMCFramework.Instance.GameInstance.GoingToTitleScreenAction; }
            set { FMCFramework.Instance.GameInstance.GoingToTitleScreenAction = value; }
        }

        /// <summary>
        /// Called every time the game state changes.
        /// The first parameter is the old state, the second is the new one.
        /// </summary>
        public static Action<FMCGameState, FMCGameState> GameStateChanged
        {
            get { return FMCFramework.Instance.GameInstance.GameStateChanged; }
            set { FMCFramework.Instance.GameInstance.GameStateChanged = value; }
        }

        /// <summary>
        /// Called every time the player gains experience
        /// </summary>
        public static Action<int> GainedExperience
        {
            get { return FMCFramework.Instance.GameInstance.GainingExperienceAction; }
            set { FMCFramework.Instance.GameInstance.GainingExperienceAction = value; }
        }

        /// <summary>
        /// Called every time the player levels up
        /// </summary>
        public static Action<int> LevelUp
        {
            get { return FMCFramework.Instance.GameInstance.LevelUpAction; }
            set { FMCFramework.Instance.GameInstance.LevelUpAction = value; }
        }
    }

    public class utils
    {
        /// <summary>
        /// Makes the given transform have the red arrow pointed towards
        /// the target
        /// </summary>
        public static void LookAt2D(Transform t, Vector3 target)
        {
            Vector3 norTar = (target - t.position).normalized;
            float angle = SignedAngle2D(t.right, norTar);
            t.rotation *= Quaternion.Euler(0, 0, angle);
        }

        public static float SignedAngle2D(Vector2 a, Vector2 b)
        {
            return Vector3.SignedAngle(a, b, Vector3.one);
        }

        public static Quaternion RandomRotation2D(float min = 0, float max = 360)
        {
            return Quaternion.Euler(0, 0, UnityEngine.Random.Range(min, max));
        }

        public static Vector2 GetCameraSize()
        {
            return GetCameraSize(Camera.main);
        }

        public static Vector2 GetCameraSize(Camera targetcam)
        {
            float cameraSizeY = targetcam.orthographicSize * 2;
            return new Vector2(cameraSizeY * Screen.width / Screen.height, cameraSizeY);
        }

        public static bool IsInCameraBounds(SpriteRenderer sr)
        {
            Vector2 cameraPos = Camera.main.transform.position;
            Vector2 cameraSize = GetCameraSize();
            Vector2 objPos = sr.transform.position;
            Vector2 objSize = sr.bounds.size;

            Rect camera = new Rect(cameraPos - cameraSize / 2, cameraSize);
            Rect obj = new Rect(objPos - objSize / 2, objSize);
            return camera.Overlaps(obj);
        }

        /// <summary>
        /// They represent the position an anchor can attach to
        /// </summary>
        public enum CameraAnchor : uint
        {//Bitmasked. I added the constants to the corners to make them appear below the others in Unity inspector
            Center = 1,
            Left = 2,
            Right = 4,
            Top = 8,
            Bottom = 16,
            TLCorner = Top | Left + 32,
            TRCorner = Top | Right + 64,
            BLCorner = Bottom | Left + 128,
            BRCorner = Bottom | Right + 256
        };

        /// <summary>
        /// Gets the world position of the requested point of the edge/corner of the camera.
        /// If you want, you can specify an padding calculated in percentage of the screen.
        /// i.e. TLCorner, .3, .4 will return the point at 30% of width and 40% of height, starting from the Top Left.
        /// Leave padding to zero if you just want the corresponding point of the CameraAnchor.
        /// Padding will only work if the anchor touches the corresponding border. i.e. paddingX will not affect Top anchor.
        /// </summary> 
        public static Vector2 GetCameraPosition(CameraAnchor anchor, float paddingX = 0, float paddingY = 0)
        {
            return GetCameraPosition(Camera.main, anchor, paddingX, paddingY);
        }

        public static Vector2 GetCameraPosition(Camera targetCam, CameraAnchor anchor, float paddingX = 0, float paddingY = 0)
        {
            Vector2 pos = targetCam.transform.position;
            Vector2 extent = GetCameraSize() / 2;

            pos.x += anchor.Contains(CameraAnchor.Right) ? Mathf.Lerp(extent.x, -extent.x, paddingX) : 0;
            pos.x += anchor.Contains(CameraAnchor.Left) ? Mathf.Lerp(-extent.x, extent.x, paddingX) : 0;
            pos.y += anchor.Contains(CameraAnchor.Top) ? Mathf.Lerp(extent.y, -extent.y, paddingY) : 0;
            pos.y += anchor.Contains(CameraAnchor.Bottom) ? Mathf.Lerp(-extent.y, extent.y, paddingY) : 0;

            return pos;
        }

        public static bool IsNullOrWhiteSpace(string s)
        {
            return s == null || s.Trim() == "";
        }

        public static Vector2 GetRandomDirection()
        {
            return UnityEngine.Random.insideUnitCircle.normalized;
        }
    }

    public class effects
    {
        /// <summary>
        /// Implement the interface to have your own kaboom effects
        /// </summary>
        public interface IKaboomable
        {
            void Kaboom(Color explosionColor, float explosionScale, bool destroy);
        }

        /// <summary>
        /// Spawns a boom effect at the given position
        /// </summary>
        public static void Boom(Vector3 position) { Boom(position, Color.white, 1f); }

        /// <summary>
        /// Spawns a boom effect at the given position
        /// </summary>
        public static void Boom(Vector3 position, Color color, float scale)
        {
            var explosionPrefab = Resources.Load("BoomExplosion");

            GameObject explosion = (GameObject)UnityEngine.Object.Instantiate(explosionPrefab, position, Camera.main.transform.rotation);
            UnityEngine.Object.Destroy(explosion, 3f);
            foreach (SpriteRenderer sr in explosion.GetComponentsInChildren<SpriteRenderer>(true))
                sr.color = color;

            explosion.transform.localScale *= scale;
        }

        /// <summary>
        /// Given a gameObject, makes it explode.
        /// Works with sprites.
        /// </summary>
        public static void Kaboom(GameObject go, Color explosionColor, float explosionScale = 1, bool destroyObject = true)
        {
            IKaboomable b = go.GetComponent<IKaboomable>();
            if (b != null)
                b.Kaboom(explosionColor, explosionScale, destroyObject);
            else
            {
                float fadeTime = .3f;

                TweenAlpha.Begin(go, fadeTime, 0);
                if (destroyObject)
                    UnityEngine.Object.Destroy(go, fadeTime);

                Boom(go.transform.position, explosionColor, explosionScale);
            }
        }

        /// <summary>
        /// Kabooms all the objects containing the given Type
        /// </summary>
        public static void KaboomAll<T>(Color explosionColor, float explosionScale = 1, bool destroyObject = true) where T : MonoBehaviour
        {
            T[] targets = UnityEngine.Object.FindObjectsOfType<T>();
            foreach (T e in targets) Kaboom(e.gameObject, explosionColor, explosionScale, destroyObject);
        }

        /// <summary>
        /// Want to restore an object you made explode?
        /// (Only works if object is not destroyed)
        /// </summary>
        public static void UnKaboom(GameObject go)
        {
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr)
            {
                Color newColor = sr.color;
                newColor.a = 1;
                sr.color = newColor;
            }
        }

        /// <summary>
        /// Spawn a label with the given text on the given target
        /// </summary>
        public static void ScoreMark(string text, Color color, GameObject target, float duration) { ScoreMark(text, color, target, duration, new Vector3(0, 30, 0)); }

        /// <summary>
        /// Spawn a label with the given text on the given target
        /// </summary>
        public static void ScoreMark(string text, Color color, GameObject target, float duration, Vector3 offset)
        {
            if (FMCGameManager.Instance)
                FMCGameManager.Instance.ScoreMark(text, color, target, duration, offset);
        }
    }

#endregion

#region Advertisement & Analytics

    public static class ads
    {
        /// <summary>
        /// Duplicated from UnityEngine.Advertisements
        /// </summary>
        public enum VideoAdShowResult
        {
            Failed = 0,
            Skipped = 1,
            Finished = 2
        }

        /// <summary>
        /// Shows a rewarded video ad.
        /// If you want, you can bind a callback which will tell you what happened while the video was playing.
        /// You should only reward the user if the result is "Finished".
        /// </summary>
        public static void ShowRewardedVideo(Action<VideoAdShowResult> callback = null) { FMCFramework.Instance.Ads.ShowRewardedVideoAd(callback); }

        /// <summary>
        /// Requests a banner and shows it in the given position of the screen
        /// </summary>
        public static void ShowBanner(BannerAdPosition position = BannerAdPosition.Bottom) { FMCFramework.Instance.Ads.RequestAndShowBanner(position); }

        /// <summary>
        /// Hides the banner if it is currently loaded.
        /// </summary>
        public static void HideBanner() { FMCFramework.Instance.Ads.DestroyBannerIfLoaded(); }

        /// <summary>
        /// Loads an interstitial ad. 
        /// This does not show it! Use ShowInterstitial() after some time or use LoadAndShowInterstitial() to show the interstitial.
        /// </summary>
        public static void LoadInterstitial() { FMCFramework.Instance.Ads.RequestInterstitial(false); }

        /// <summary>
        /// Shows a previously loaded interstitial.
        /// If the interstitial is not loaded, this calls has no effect.
        /// </summary>
        public static void ShowInterstitial() { FMCFramework.Instance.Ads.ShowInterstitial(); }

        /// <summary>
        /// Requests an interstitial ad and shows it. Time may pass before the ad is shown.
        /// A better approach is to call LoadInterstitial() first and then show it when needed with ShowInterstitial()
        /// </summary>
        public static void LoadAndShowInterstitial() { FMCFramework.Instance.Ads.RequestInterstitial(true); }

        /// <summary>
        /// Hides the interstitial if it is currently loaded.
        /// </summary>
        public static void HideInterstitial() { FMCFramework.Instance.Ads.DestroyInterstitialIfLoaded(); }
    }

    public static class analytics
    {
        public const string EventName_UnityAdWatched = "ad_unity_rewarded_video";
        public const string EventName_ResetGame = "game_reset_game";
        public const string EventName_GameStarted = "game_started";
        public const string EventName_GameOver = "game_over";
        public const string EventName_ResumeFromGameOver = "resume_from_game_over";
        public const string EventName_NewBestScore = "game_new_best_score";
        public const string EventName_LevelUp = "level_up";

        public const string EventParamName_Score = "score";

        public static void Event(string eventName) { FMCFramework.Instance.Analytics.Event(eventName); }
        public static void Event(string eventName, string paramName, int param) { FMCFramework.Instance.Analytics.Event(eventName, paramName, param); }
        public static void Event(string eventName, string paramName, long param) { FMCFramework.Instance.Analytics.Event(eventName, paramName, param); }
        public static void Event(string eventName, string paramName, double param) { FMCFramework.Instance.Analytics.Event(eventName, paramName, param); }
        public static void Event(string eventName, string paramName, string param) { FMCFramework.Instance.Analytics.Event(eventName, paramName, param); }
        public static void Event(string name, params FMCAnalyticsParameter[] parameters) { FMCFramework.Instance.Analytics.Event(name, parameters); }
    }

#endregion

}

public static class ExtensionMethods
{
    /// <summary>
    /// Check if this anchor contains the given one.
    /// i.e. TRCorner will contain Top and Right
    /// </summary>
    public static bool Contains(this fmc.utils.CameraAnchor anchor, fmc.utils.CameraAnchor other)
    {
        return (anchor & other) > 0;//God will forgive me for this
    }
}