using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class implements a simple UI Manager for fmc games.
/// Automatically handles title screen, game over with best, watch ad to revive.
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField]
    UIButton allScreenButton = null;

    [Header("Title screen UI")]
    [SerializeField]
    UIWidget titleSreenUI = null;
    [SerializeField]
    UI2DSprite gameLogoSprite = null;
    [SerializeField]
    UILabel titleScreenBestScoreLabel = null;
    [SerializeField]
    UIButton playButton = null;
    [SerializeField]
    UIButton gdprButton = null;
    [SerializeField]
    GDPRPrompt gdprPromptPrefab = null;
    [SerializeField]
    float UITransitionsDuration = .1f;

    [Header("Game UI")]
    [SerializeField]
    UIWidget gameUI = null;
    [SerializeField]
    UILabel gameBestScoreLabel = null;
    [SerializeField]
    UILabel gameScoreLabel = null;
    [SerializeField]
    UILabel tutorialLabel = null;
    [SerializeField]
    ScoreMark scoreMarkPrefab = null;

    [Header("Game over UI")]
    [SerializeField]
    UIWidget gameOverUI = null;
    [SerializeField]
    UIWidget gameOverNormalUI = null;
    [SerializeField]
    UIWidget gameOverSecondChanceUI = null;
    [SerializeField]
    UILabel gameOverScoreLabel = null;
    [SerializeField]
    UILabel gameOverBestScoreLabel = null;
    [SerializeField]
    UILabel newBestScoreLabel = null;
    [SerializeField]
    AdsButton gameOverAdsButton = null;
    [SerializeField]
    UIButton goToTitleScreenButton = null;
    [SerializeField]
    float minTimeToRestart = .6f;
    [SerializeField]
    float adsButtonDuration = 2f;

    [Header("Level system UI")]
    [SerializeField]
    LevelSystemUI levelSystemUI = null;

    bool hasLost;
    float gameOverTime = 0;

    private void Awake()
    {
        titleSreenUI.gameObject.SetActive(true);
        titleSreenUI.alpha = 0;

        gameLogoSprite.sprite2D = fmc.game.Settings.gameLogo;
        gameLogoSprite.MakePixelPerfect();
        gameLogoSprite.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
        Transform bottomAnchorTarget = gameLogoSprite.bottomAnchor.target;
        gameLogoSprite.SetAnchor(gameLogoSprite.leftAnchor.target.gameObject, 5, 5, -5, -5);
        gameLogoSprite.bottomAnchor.target = bottomAnchorTarget;
        gameLogoSprite.bottomAnchor.absolute = -5;
        gameLogoSprite.ResetAndUpdateAnchors();


        gdprButton.onClick.Add(new EventDelegate(ShowGDPRPrompt));
        playButton.onClick.Add(new EventDelegate(OnPlayButtonPressed));
        allScreenButton.onClick.Add(new EventDelegate(OnPlayButtonPressed));

        gameUI.gameObject.SetActive(true);
        gameUI.alpha = 0;
        tutorialLabel.text = fmc.game.Settings.tutorialMessage;

        gameOverUI.gameObject.SetActive(true);
        gameOverUI.alpha = 0;
        gameOverSecondChanceUI.gameObject.SetActive(false);
        newBestScoreLabel.gameObject.SetActive(false);

        allScreenButton.gameObject.SetActive(false);

        fmc.ads.LoadInterstitial();

        hasLost = false;
    }

    void Start()
    {
        fmc.events.GameStateChanged += (oldState, newState) => { UpdateUIFromState(newState); };

        fmc.events.NewBestScore += (score) =>
        {
            newBestScoreLabel.gameObject.SetActive(true);
            UITweener tween = newBestScoreLabel.GetComponent<UITweener>();
            tween.ResetToBeginning();
            tween.PlayForward();
        };

        UpdateUIFromState(fmc.game.GameState);

        fmc.events.ResetGame += () =>
        {
            hasLost = false;
            newBestScoreLabel.gameObject.SetActive(false);
        };

        fmc.events.GainedExperience += (experienceGained) =>
        {
            levelSystemUI.Show(fmc.game.Level, fmc.game.Experience, experienceGained);
        };

        gameOverAdsButton.OnAdWatched += () => 
        {
            gameOverNormalUI.gameObject.SetActive(false);
            gameOverSecondChanceUI.gameObject.SetActive(true);
        };

        goToTitleScreenButton.onClick.Add(new EventDelegate(() => 
        {
            fmc.ads.HideBanner();
            fmc.game.GoToTitleScreen(); 
        }));

        if (fmc.game.Settings.enableLevelSystem)
            levelSystemUI.StartPopup();
    }

    void Update ()
    {
        if (fmc.game.GameState == FMCGameState.Playing)
            gameScoreLabel.text = fmc.game.CurrentScore.ToString();
        else if (fmc.game.GameState == FMCGameState.GameOver
                && Input.GetKeyDown(KeyCode.Escape)
                && Time.time - gameOverTime > minTimeToRestart)
        {//Handling back button in android devices
            fmc.ads.HideBanner();
            fmc.game.GoToTitleScreen();
        }
    }

    void UpdateUIFromState(FMCGameState state)
    {
        switch (state)
        {
            case FMCGameState.TitleScreen:
                titleScreenBestScoreLabel.text = fmc.game.BestScore.ToString();
                break;
            case FMCGameState.Playing:
                gameBestScoreLabel.text = fmc.game.BestScore.ToString();
                newBestScoreLabel.gameObject.SetActive(false);
                if (!fmc.utils.IsNullOrWhiteSpace(tutorialLabel.text) && !hasLost)
                {
                    tutorialLabel.gameObject.SetActive(true);
                    tutorialLabel.alpha = 1;
                    TweenAlpha.Begin(tutorialLabel.gameObject, 1, 0, 2);
                }
                break;
            case FMCGameState.GameOver:
                gameOverSecondChanceUI.gameObject.SetActive(false);
                gameOverNormalUI.gameObject.SetActive(true);

                if (!hasLost)
                    gameOverAdsButton.ShowAdsButton(adsButtonDuration);
                else
                {
                    fmc.ads.ShowInterstitial(); //Showing interstitial only on second run game over
                    fmc.ads.ShowBanner(); //updating banner on second run game over
                }

                hasLost = true;
                gameOverTime = Time.time;
                gameOverBestScoreLabel.text = fmc.game.BestScore.ToString();
                gameOverScoreLabel.text = fmc.game.CurrentScore.ToString();
                break;
        }

        allScreenButton.gameObject.SetActive(state == FMCGameState.GameOver);
        TweenAlpha.Begin(titleSreenUI.gameObject, UITransitionsDuration, state == FMCGameState.TitleScreen ? 1 : 0);
        TweenAlpha.Begin(gameUI.gameObject, UITransitionsDuration, state == FMCGameState.Playing ? 1 : 0);
        TweenAlpha.Begin(gameOverUI.gameObject, UITransitionsDuration, state == FMCGameState.GameOver ? 1 : 0);
    }

    void OnPlayButtonPressed()
    {
        if (Time.time - gameOverTime > minTimeToRestart)
        {
            if(!gameOverSecondChanceUI.gameObject.activeInHierarchy)
                fmc.game.ResetGame();

            if(fmc.game.GameState == FMCGameState.TitleScreen)
                fmc.ads.ShowBanner();

            fmc.game.StartGame();
        }
    }

    void ShowGDPRPrompt()
    {
        SetButtonsEnabled(false);
        fmc.ads.HideBanner();
        GDPRPrompt prompt = NGUITools.AddChild(gameObject, gdprPromptPrefab.gameObject).GetComponent<GDPRPrompt>();
        prompt.GDPRPrompted += () => 
        {
            SetButtonsEnabled(true);
        };
    }

    void SetButtonsEnabled(bool value)
    {
        playButton.enabled = value;
        gdprButton.enabled = value;
    }

    /// <summary>
    /// Spawn a label with the given text on the given target
    /// </summary>
    public void ScoreMark(string text, Color color, GameObject target, float duration, Vector3 offset)
    {
        ScoreMark mark = NGUITools.AddChild(gameObject, scoreMarkPrefab.gameObject).GetComponent<ScoreMark>();
        mark.InitScoreMark(text, color, target, duration, offset);
    }
}
