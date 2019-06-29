using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField]
    UI2DSprite backgroundSprite = null;

    [SerializeField]
    UI2DSprite companySprite = null;

    [SerializeField]
    GDPRPrompt gdprPromptPrefab = null;

    void Awake ()
    {
        backgroundSprite.sprite2D = fmc.game.Settings.splashScreenBackground;
        companySprite.sprite2D = fmc.game.Settings.companyLogo;

        StartCoroutine(SplashScreenCoroutine());
        FMCFramework.Instance.GameInstance.GoToState(FMCGameState.SplashScreen);
    }

    private void Start()
    {
        if (fmc.game.Settings.companyLogo)
        {
            companySprite.MakePixelPerfect();
            companySprite.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
            companySprite.SetAnchor(companySprite.leftAnchor.target.gameObject, 20, 0, -20, 0);
            companySprite.UpdateAnchors();
        }
    }

    IEnumerator SplashScreenCoroutine()
    {
        float splashScreenTime = fmc.game.Settings.splashScreenDuration;

#if UNITY_EDITOR
        splashScreenTime = .4f;
        FMCFramework.Instance.GameInstance.SetProperty(fmc.game.GDPRPromptedPropertyName, false); // uncomment to always show GDPR stuff
#endif

        yield return new WaitForSeconds(splashScreenTime);

        bool hasAlreadyAccepted = FMCFramework.Instance.GameInstance.GetProperty(fmc.game.GDPRPromptedPropertyName, false); ;

        if (!hasAlreadyAccepted)
        {
            GDPRPrompt prompt = NGUITools.AddChild(gameObject, gdprPromptPrefab.gameObject).GetComponent<GDPRPrompt>();
            prompt.GDPRPrompted += () => { FMCFramework.Instance.GameInstance.GoToGameScene(); };
        }
        else
        {
            FMCFramework.Instance.GameInstance.GoToGameScene();
        }
    }
}
