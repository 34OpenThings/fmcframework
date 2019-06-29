using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GDPRPrompt : MonoBehaviour
{
    public Action GDPRPrompted;

    [SerializeField]
    float transitionAnimationsDuration = .2f;

    [Header("Main UI")]
    [SerializeField]
    UIWidget mainUI = null;

    [SerializeField]
    UIButton policyButton = null;
    [SerializeField]
    UIButton partnersButton = null;

    [SerializeField]
    UIButton useCustomOptionsButton = null;
    [SerializeField]
    UIButton acceptAllButton = null;

    [SerializeField]
    UIToggle readPrivacyPolicyToggle = null;
    [SerializeField]
    UIToggle wantsPersonalizedAdsToggle = null;

    [SerializeField]
    float waitTimeWhenAcceptAllIsPressed = 0;

    [Header("Partners UI")]
    [SerializeField]
    UIWidget partnersUI = null;

    [SerializeField]
    UIButton partnersBackButton = null;
    [SerializeField]
    UIButton partnersUnityButton = null;
    [SerializeField]
    UIButton partnersGoogleButton = null;

    private void Start()
    {
        //****** Main UI

        policyButton.onClick.Add(new EventDelegate(()=> { OpenURL(fmc.game.Settings.companyPrivacyPolicyURL); }));

        //You can write {company_name} in any label inside GDPR prompt and it will be substituted
        UILabel[] labels = GetComponentsInChildren<UILabel>(true);
        foreach (UILabel label in labels)
        {
            label.text = label.text.Replace("{company_name}", fmc.game.Settings.companyName);
            label.text = label.text.Replace("{game_title}", fmc.game.Settings.gameTitle);
        }

        readPrivacyPolicyToggle.value = FMCFramework.Instance.GameInstance.GetProperty(fmc.game.GDPRPromptedPropertyName, false);
        wantsPersonalizedAdsToggle.value = FMCFramework.Instance.GameInstance.GetProperty(fmc.game.WantsPersonalizedAdsPropertyName, false);

        readPrivacyPolicyToggle.onChange.Add(new EventDelegate(() =>
        {//When privacy policy toggle changes, we may disable the button
            useCustomOptionsButton.isEnabled = HasAcceptedEveryMandatoryCheck() ? true : false;
        }));

        //****** Partners UI

        partnersButton.onClick.Add(new EventDelegate(TransitionToPartnersUI));
        partnersBackButton.onClick.Add(new EventDelegate(TransitionToMainUI));

        partnersUnityButton.onClick.Add(new EventDelegate(() => { OpenURL(fmc.game.Settings.unityPrivacyPolicyURL); }));
        partnersGoogleButton.onClick.Add(new EventDelegate(() => { OpenURL(fmc.game.Settings.googlePrivacyPolicyURL); }));

        Show();
    }


    public void Show()
    {
        GetComponent<UIPanel>().alpha = 0;
        mainUI.alpha = 1;
        partnersUI.alpha = 0;

        TweenAlpha.Begin(gameObject, .1f, 1);
        gameObject.SetActive(true);
        useCustomOptionsButton.onClick.Add(new EventDelegate(OnPlayClicked));
        acceptAllButton.onClick.Add(new EventDelegate(OnAcceptAllAndPlay));
    }

    private void TransitionToMainUI()
    {
        mainUI.gameObject.SetActive(true);
        TweenAlpha.Begin(partnersUI.gameObject, transitionAnimationsDuration, 0);
        TweenAlpha.Begin(mainUI.gameObject, transitionAnimationsDuration, 1, transitionAnimationsDuration);
    }

    private void TransitionToPartnersUI()
    {
        partnersUI.gameObject.SetActive(true);
        TweenAlpha.Begin(mainUI.gameObject, transitionAnimationsDuration, 0);
        TweenAlpha.Begin(partnersUI.gameObject, transitionAnimationsDuration, 1, transitionAnimationsDuration);
    }

    public void Hide()
    {
        TweenAlpha.Begin(gameObject, .1f, 0).onFinished.Add(new EventDelegate(()=> 
        {
            if (GDPRPrompted != null)
                GDPRPrompted.Invoke();
            Destroy(this);
        }));
    }

    bool HasAcceptedEveryMandatoryCheck()
    {
        return readPrivacyPolicyToggle.value;
    }

    void OnAcceptAllAndPlay()
    {
        TickAndLock(readPrivacyPolicyToggle);
        TickAndLock(wantsPersonalizedAdsToggle);

        useCustomOptionsButton.isEnabled = false;
        acceptAllButton.enabled = false;

        StartCoroutine(WaitAndPlay());
    }

    void TickAndLock(UIToggle toggle)
    {
        toggle.value = true;
        toggle.enabled = false;
    }

    void OnPlayClicked()
    {
        if (HasAcceptedEveryMandatoryCheck())
        {
            FMCFramework.Instance.GameInstance.SetProperty(fmc.game.GDPRPromptedPropertyName, true, true);
            FMCFramework.Instance.GameInstance.SetProperty(fmc.game.WantsPersonalizedAdsPropertyName, wantsPersonalizedAdsToggle.value, true);

            Hide();
        }
    }

    void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    IEnumerator WaitAndPlay()
    {
        yield return new WaitForSeconds(waitTimeWhenAcceptAllIsPressed);
        OnPlayClicked();
    }
}
