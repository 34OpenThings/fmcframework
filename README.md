# fmcframework

A simple framework for building small games with Unity.

Features:

* Simple customizable loading screen
* Infinite runner game flow.
* Save system
* Automated and tweakable level system
* GDPR compliance form
* Analytics (using Firebase, but can be changed easily)
* Banners, Intertitials, VideoAds. (Everything uses Admob except for videos, which use UnityAds)

## Import guide

This framework requires [NGUI](https://assetstore.unity.com/packages/tools/gui/ngui-next-gen-ui-2413) to work.
To use the framework, just download the [FMC Toolkit](https://github.com/34OpenThings/fmctoolkit) and insert the link of this repo inside your config file.

## Usage guide

FMC is designed to ease the developing of _simple_ mobile games.

## What FMC does

* Implements the game state machine and takes care of transitioning to scenes and game states (loading screen, title screen, game screen, game over screen).
* Provides a production-ready UIManager.
* Shows a GDPR compliance form, asks for acceptance and shows the appropriate ads. Forget about GDPR.
* Has a very simple framework you can use to perform non gameplay-related actions (see below).
* Collects Analytics.
* Shows Banners, Intertitials and VideoAds. This is taken care of by the UIManager.
* Provides a level system with automatic difficulty, points and experience. The system is tweakable by a designer.

## What you have to do

* Open FMC->Settings and set everything properly.
* Open FMC->Game design tool and set everything properly. Default will be ok for most games.
* Create a game scene and make the game. Quick tips:
    * Use fmc.events to enable/disable your gameobjects (see _fmc.events_).
    * You usually need to increase the score. Use _fmc.game.Score();_ to add points. Points are retrieved from the Level System.
    * Tell fmc when there is a game over with _fmc.game.GameOver();_.
* When the game is ready, integrate analytics and ads. The game will also work if you ignore this step. See below to know how to.

### Game states and events

FMC is built to be very simple.

![Alt text](
https://g.gravizo.com/svg?%20digraph%20G%20{%20SplashScreen%20[shape=box];%20TitleScreen%20[shape=box];%20GameOver%20[shape=box];%20Playing%20[shape=box];%20SplashScreen%20-%3E%20TitleScreen%20TitleScreen%20-%3E%20Playing;%20[label=%22%20GameStarted%22]]%20Playing%20-%3E%20GameOver;%20[label=%22%20GameOver%22]%20GameOver%20-%3E%20Playing;%20[label=%22%20GameStarted*%20%22]]%20GameOver%20-%3E%20TitleScreen;%20[label=%22GoingToTitleScreen%22]]%20}
)
<!---
NOTE: This is the original gravizo code, but GitLab does not like this directly. Paste this in your browser bar and get the resolved URL.
https://g.gravizo.com/svg?
  digraph G {
    SplashScreen [shape=box];
    TitleScreen [shape=box];
    GameOver [shape=box];
    Playing [shape=box];
    SplashScreen -> TitleScreen
    TitleScreen -> Playing; [label=" GameStarted"]]
    Playing -> GameOver; [label=" GameOver"]
    GameOver -> Playing; [label=" GameStarted* "]]
    GameOver -> TitleScreen; [label="GoingToTitleScreen"]]
  }
-->
[Boxes are **game states**, arrows are **events**]

*When GameStarted is called from the GameOver state, two other events may occur:
* OnReset - if the game was reset.
* ResumedFromGameOver - if a second chance was given.

All fmc functions can be accessed by typing fmc. Check out the GameScene inside fmc and FMCDemo.cs to get some examples.

### fmc.game

Use this to access all game-related functions and properties. 

* Set Score via fmc.game.Score(). The amount of points to add is deduced from the current level by the Level System. 
* Controlling and transitioning through game states:
    * Call fmc.game.GameOver() when the player loses. This sets the BestScore if necessary.
    * Use fmc.game.GameState to check the current game state. If you do this everytime, you might prefer hooking to an event using fmc.events.

### fmc.events

Use this to hook to any game-related event. Be careful when choosing the appropriate event: Check the game cycle.
Do not assume that fmc.events.GameOver is called when the game ends: there could be a second chance.
Any reset function should be hooked to ResetGame.

You will also probably need to activate / deactivate gameplay objects on when the game starts and ends.
Simply use:

        fmc.events.GameStarted += () => { gameObject.SetActive(true); };
        fmc.events.GameOver += (score) => { gameObject.SetActive(false); };

### fmc.ads

You should not care about ads: the FMC flow will take care of them, showing them basing on the game state.

fmc.ads is used to show ads. There are three types of ads: banners, interstitials (single image) and rewarded video ads.

* Banner can be shown at every position of the screen. By default it will appear at the bottom.
* Interstitial ads can be loaded before being shown via fmc.ads.LoadInterstitialAd(). If you do not care about showing them quickly, just use fmc.ads.LoadAndShowInterstitialAd().
* Rewarded video ads display a video on the screen and return a result. You can check the result and perform appropriate actions:
    
        fmc.ads.ShowRewardedVideo( (fmc.ads.VideoAdShowResult res) => 
            {//Handling video show result
                switch (res)
                {
                    case fmc.ads.VideoAdShowResult.Failed: Debug.Log("Video failed"); break;
                    case fmc.ads.VideoAdShowResult.Finished: Debug.Log("Video finished!"); break;
                    case fmc.ads.VideoAdShowResult.Skipped: Debug.Log("Video skipped!"); break;
                }
            }
        );

### fmc.analytics
Use this to send custom analytics. You can use all the fmc.analytics.Event() overrides to send anything you want.
Remember to be consistent with the names of the parameters.
Note that fmc already collects this data for you:
* Unity ad watched 
* Reset game 
* Game started 
* Game over
* Resume from game over 
* New best score
* Admob ads (collected by Firebase)

## Integration with Ads and Analytics services
When the game is ready, you can integrate Ads and Analytics.
You can also integrate only one of the two. Remember that Ads are required for second chance.

FMC uses both UnityAds and AdMob for ads, while it uses Firebase for analytics.
* **UnityAds**:
    * Create a Unity account and follow Unity instructions.
    * Enable Ads in the Editor, in the Services tab.
* **Admob**:
    * Create an Admob account, register the app and follow their instructions.
    * Integrate the Admob SDK for Unity by installing the package. https://developers.google.com/admob/unity/start
    * Insert the ids on the ScriptableObject located in _Content/_FMCUtils/Resources/GameSettings.
    * Also set the appID as described in Google's guide. This is not ideal, but **mandatory and important**: https://developers.google.com/admob/unity/start (You have to update a manifest file with your app id)
* **Firebase**:
    * Create a Firebase account, register the app and follow their instructions. 
    * Integrate the Admob SDK for Unity by installing the package. https://firebase.google.com/docs/unity/setup (You need only the Analytics SDK)
    * Put the google-services.json file in the project. It can be placed anywhere, but use _Content/Certifications to be consistent between fmc projects.
    * Go to _Content/_FMCUtils/Resources/GameSettings and tick EnableAnalytics. If google-services.json is missing, enabling this will cause errors.
    * Use debug view to debug events https://firebase.google.com/docs/analytics/debugview

When you have completed this, go to FMC->Settings and enable Ads and/or Analytics. FMC will enable the calls to the external code.
If anything goes wrong, just untick the options and ponder about your mistakes.
