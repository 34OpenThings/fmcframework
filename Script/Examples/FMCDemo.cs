using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this MonoBehaviour to a game object to show a GUI with main functions
/// </summary>
public class FMCDemo : MonoBehaviour
{
    [SerializeField]
    UILabel videoOutputLabel = null;
    [SerializeField]
    UILabel fpsLabel = null;
    [SerializeField]
    UILabel scoreLabel = null;
    [SerializeField]
    UILabel bestScoreLabel = null;
    [SerializeField]
    UILabel gameStateLabel = null;

    float deltaTime = 0.0f;
    float fps;
    string text = "";

    public void Start()
    {
        gameStateLabel.text = fmc.game.GameState.ToString();
        fmc.events.GameStateChanged += (oldstate, newstate) => { gameStateLabel.text = newstate.ToString(); };

        fmc.events.ResetGame += () => { Debug.Log("NewGame event fired!"); };
        fmc.events.NewBestScore += (score) => { Debug.Log("NewBest event fired! New best is " + score); };
        fmc.events.GameOver += (score) => { Debug.Log("GameOver event fired! Score is " + score); };
        fmc.events.ResumedFromGameOver += () => { Debug.Log("ResumeFromGameOver event fired!"); };
        fmc.events.GameStateChanged += (oldstate, newstate) => { Debug.Log("GameStateChanged event fired! " + oldstate.ToString() + " --> " + newstate.ToString()); };
    }

    public void Update()
    {
        // Calculate simple moving average for time to render screen. 0.1 factor used as smoothing value.
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        fps = 1.0f / deltaTime;
        text = string.Format("{0:0.} fps", fps);

        if (fps >= 59.6)
            text += "\ngranitici";

        fpsLabel.text = text;

        scoreLabel.text = "Score:" + fmc.game.CurrentScore;
        bestScoreLabel.text = "Best:" + fmc.game.BestScore;
    }

    public void ShowBannerAd() { fmc.ads.ShowBanner(); }
    public void HideBannerAd() { fmc.ads.HideBanner(); }
    public void LoadInterstitialAd() { fmc.ads.LoadInterstitial(); }
    public void LoadAndShowInterstitialAd() { fmc.ads.LoadAndShowInterstitial(); }
    public void HideInterstitial() { fmc.ads.HideInterstitial(); }

    public void ShowRewardedVideoAd()
    {
        fmc.ads.ShowRewardedVideo( 
            (fmc.ads.VideoAdShowResult res) => 
            {//Handling video show result
                switch (res)
                {
                    case fmc.ads.VideoAdShowResult.Failed: videoOutputLabel.text = "[FF0000] Video failed"; break;
                    case fmc.ads.VideoAdShowResult.Finished: videoOutputLabel.text = "[00FF00] Video finished!"; break;
                    case fmc.ads.VideoAdShowResult.Skipped: videoOutputLabel.text = "[FF9000] Video skipped!"; break;
                }
            }
        );
    }

    public void AddScore() { fmc.game.Score(); }
    public void NewGame() { fmc.game.ResetGame(); }
    public void GameOver() { fmc.game.GameOver(); }
    public void ResumeFromGameOver() { fmc.game.StartGame(); }
    public void SendHello() { fmc.analytics.Event("Hello"); }

}
