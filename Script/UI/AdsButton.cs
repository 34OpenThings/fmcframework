using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIButton))]
public class AdsButton : MonoBehaviour
{
    [SerializeField]
    UISprite progressBar = null;
    UIButton button = null;

    float count = 0;
    float duration = 1;

    public Action OnAdWatched;

    public void Awake()
    {
        button = GetComponent<UIButton>();
        button.onClick.Add(new EventDelegate(OnButtonPressed));
    }

    public void ShowAdsButton(float duration)
    {
        gameObject.SetActive(true);
        enabled = true;
        button.isEnabled = true;

        this.duration = duration;
        count = duration;
        button.SetState(UIButtonColor.State.Normal, true);
        progressBar.fillAmount = 1;
    }

    public void HideAdsButton()
    {
        gameObject.SetActive(false);
    }

    public void FixedUpdate()
    {
        count -= Time.deltaTime;
        progressBar.fillAmount = count / duration;

        if (count <= 0)
        {
            enabled = false;
           // button.SetState(UIButtonColor.State.Disabled, true);
            button.isEnabled = false;
        }
    }

    public void OnButtonPressed()
    {
        HideAdsButton();
        fmc.ads.ShowRewardedVideo((result) =>
        {
            if (result == fmc.ads.VideoAdShowResult.Finished)
                if (OnAdWatched != null) OnAdWatched.Invoke();
        });
    }
}
