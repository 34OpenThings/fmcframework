using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if ACTIVATE_ADS_CALLS
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
#endif

/// <summary>
/// Defines a screen anchoring and position.
/// Duplicated from GoogleMobileAds.Api.AdPosition in AdPosition.cs
/// </summary>
public enum BannerAdPosition
{
    Top = 0,
    Bottom = 1,
    TopLeft = 2,
    TopRight = 3,
    BottomLeft = 4,
    BottomRight = 5,
    Center = 6
}

public class FMCAds
{
    public const string PrecompileDirectiveName = "ACTIVATE_ADS_CALLS"; // used by game settings to activate third party calls. Has to match the name of the directives used below.

    public FMCAds()
    {
#if ACTIVATE_ADS_CALLS
        MobileAds.SetiOSAppPauseOnBackground(true);
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        InitAdmobIdentifiers();
#endif
    }

    /// <summary>
    /// Shows a video ad.
    /// Pass the callback to be notified when the ad is closed. 
    /// You can open this class to find a template method to handle the callback.
    /// </summary>
    public void ShowRewardedVideoAd(Action<fmc.ads.VideoAdShowResult> callback)
    {
#if ACTIVATE_ADS_CALLS
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions { resultCallback = (ShowResult result) => 
            {//Binding a lambda to perform the cast to fmc's enum and fire the given callback 
                if (callback != null)
                    callback.Invoke((fmc.ads.VideoAdShowResult)result);
            }};

            Advertisement.Show("rewardedVideo", options);

            FMCFramework.Instance.Analytics.Event(fmc.analytics.EventName_UnityAdWatched);
        }
#else
        callback.Invoke(fmc.ads.VideoAdShowResult.Finished);
#endif

    }

    //Admob **************************************************************
    //********************************************************************

#if ACTIVATE_ADS_CALLS
    #region Admob ids

    string appId =
#if UNITY_ANDROID
    "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IPHONE
    "ca-app-pub-3940256099942544~1458002511";
#else
    "unexpected_platform";
#endif

    // These ad units are configured to always serve test ads.
    string adBannerUnitId =
#if UNITY_EDITOR
    "unused";
#elif UNITY_ANDROID
    "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
    "ca-app-pub-3940256099942544/2934735716";
#else
    "unexpected_platform";
#endif

    // These ad units are configured to always serve test ads.
    string adInterstitialUnitId =
#if UNITY_EDITOR
     "unused";
#elif UNITY_ANDROID
    "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
    "ca-app-pub-3940256099942544/4411468910";
#else
    "unexpected_platform";
#endif

    #endregion

    private BannerView bannerView;
    private InterstitialAd interstitial;

    public bool IsInterstitialReady { get { return interstitial != null && interstitial.IsLoaded(); } }
#else
    public bool IsInterstitialReady { get { return false; } }

#endif

#if ACTIVATE_ADS_CALLS
    private void InitAdmobIdentifiers()
    {
#if UNITY_ANDROID
        if (!fmc.game.Settings.AndroidAdMobUseTestIds)
        {
            if(!string.IsNullOrEmpty(fmc.game.Settings.AndroidAppId))
                appId = fmc.game.Settings.AndroidAppId;

            if(!string.IsNullOrEmpty(fmc.game.Settings.AndroidAdBannerUnitId))
                adBannerUnitId = fmc.game.Settings.AndroidAdBannerUnitId;

            if(!string.IsNullOrEmpty(fmc.game.Settings.AndroidAdInterstitialUnitId))
                adInterstitialUnitId = fmc.game.Settings.AndroidAdInterstitialUnitId;
        }
#elif UNITY_IPHONE
        if (!fmc.game.Settings.iOSAdMobUseTestIds)
        {
            if(!string.IsNullOrEmpty(fmc.game.Settings.iOSAppId))
                appId = fmc.game.Settings.iOSAppId;

            if(!string.IsNullOrEmpty(fmc.game.Settings.iOSAdBannerUnitId))
                adBannerUnitId = fmc.game.Settings.iOSAdBannerUnitId;

            if(!string.IsNullOrEmpty(fmc.game.Settings.iOSAdInterstitialUnitId))
                adInterstitialUnitId = fmc.game.Settings.iOSAdInterstitialUnitId;
        }
#endif
    }
#endif
    
    public void RequestAndShowBanner (BannerAdPosition bannerPosition)
    {
#if ACTIVATE_ADS_CALLS
        AdPosition position = (AdPosition)bannerPosition;
        // Clean up banner ad before creating a new one.
        if (bannerView != null)
            bannerView.Destroy();

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adBannerUnitId, AdSize.SmartBanner, position);

        // Register for ad events.
        bannerView.OnAdLoaded += HandleAdLoaded;
        bannerView.OnAdFailedToLoad += HandleAdFailedToLoad;
        bannerView.OnAdOpening += HandleAdOpened;
        bannerView.OnAdClosed += HandleAdClosed;
        bannerView.OnAdLeavingApplication += HandleAdLeftApplication;

        Debug.Log("LOADING BANNER!");

        // Load a banner ad.
        bannerView.LoadAd(CreateAdRequest());
#endif
    }

    public void DestroyBannerIfLoaded()
    {
#if ACTIVATE_ADS_CALLS
        if (bannerView != null)
            bannerView.Destroy();
#endif
    }

    public void RequestInterstitial(bool automaticallyShowWhenReady, bool loadNextAdWhenClosing = true)
    {
#if ACTIVATE_ADS_CALLS
        // Clean up interstitial ad before creating a new one.
        if (interstitial != null)
            interstitial.Destroy();

        // Create an interstitial.
        interstitial = new InterstitialAd(adInterstitialUnitId);

        // Register for ad events.
        interstitial.OnAdLoaded += HandleInterstitialLoaded;

        if (automaticallyShowWhenReady)
            interstitial.OnAdLoaded += (object sender, EventArgs args) => { ShowInterstitial(); };

        //interstitial.OnAdFailedToLoad += HandleInterstitialFailedToLoad;
        //interstitial.OnAdOpening += HandleInterstitialOpened;
        //interstitial.OnAdLeavingApplication += HandleInterstitialLeftApplication;

        if (loadNextAdWhenClosing)
            interstitial.OnAdClosed += HandleInterstitialClosed;

        // Load an interstitial ad.
        interstitial.LoadAd(CreateAdRequest());
#endif
    }

    public void ShowInterstitial()
    {
#if ACTIVATE_ADS_CALLS
        if (IsInterstitialReady)
            interstitial.Show();
        else
            Debug.Log("Interstitial is not ready yet");
#endif
    }

    public void DestroyInterstitialIfLoaded()
    {
#if ACTIVATE_ADS_CALLS
        if (interstitial != null)
            interstitial.Destroy();
#endif
    }

#if ACTIVATE_ADS_CALLS
    // Returns an ad request with custom ad targeting.
    private AdRequest CreateAdRequest()
    {
        // https://developers.google.com/android/reference/com/google/android/gms/ads/AdRequest.Builder

        if (FMCFramework.Instance.GameInstance.GetProperty(fmc.game.WantsPersonalizedAdsPropertyName, false))
        {
            Debug.Log("Requesting Personalized Ad");
            return new AdRequest.Builder()
                //.TagForChildDirectedTreatment(false) //child directed treatment COPPA
                .Build();
        }
        else
        {
            Debug.Log("Requesting NON Personalized Ad");
            return new AdRequest.Builder()
               .AddExtra("npa", "1") //Non personalized ads!
               .Build();
        }
    }
#endif

#if ACTIVATE_ADS_CALLS
#region Banner callback handlers

    public void HandleAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLoaded event received");
        Debug.Log("LOADED BANNER!");

    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.LogError("ADMOBBBB " + args.Message);
        Debug.Log("HandleFailedToReceiveAd event received with message: " + args.Message);
    }

    public void HandleAdOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
        Debug.Log("OPENED BANNER!");
    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleAdClosed event received");
    }

    public void HandleAdLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLeftApplication event received");
    }

#endregion

#region Interstitial callback handlers

    public void HandleInterstitialLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleInterstitialLoaded event received");
    }

    public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleInterstitialFailedToLoad event received with message: " + args.Message);
    }

    public void HandleInterstitialOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleInterstitialOpened event received");
    }

    public void HandleInterstitialClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleInterstitialClosed event received");

        RequestInterstitial(false);
    }

    public void HandleInterstitialLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleInterstitialLeftApplication event received");
    }

#endregion
#endif
}