using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystemUI : MonoBehaviour
{
    [SerializeField]
    UILabel leftLevelLabel = null;
    [SerializeField]
    UILabel rightLevelLabel = null;
    [SerializeField]
    UISprite[] bars = null;
    [SerializeField]
    ParticleSystem particles = null;

    [SerializeField]
    AnimationCurve levelUpScaleCurve = null;

    [SerializeField]
    float levelUpAnimationTime = .4f;

    [SerializeField]
    float popupAnimationTime = .3f;

    [SerializeField]
    float initialDelayOnGameOpen = 1f;

    [SerializeField]
    float fillAnimationSpeed = .3f;

    [SerializeField]
    float stayOnScreenOnceFillCompletedTime = .5f;

    int currentLevel = 1;
    float fillAmount = 0;
    float targetFill = 0;
    int levelsToComplete = 0;

    Vector3 closedScale = new Vector3(1, 0, 1);
    void Start()
    {
        var m = particles.emission;
        m.enabled = false;
        particles.Clear();
        particles.GetComponent<ParticleSystemRenderer>().material.renderQueue = 3001;

        SetBarsFill(0);
        transform.localScale = closedScale;
        GetComponent<UIWidget>().alpha = 0;
        //StartCoroutine(TestRoutine());
        enabled = false;
    }

    void Update()
    {
        float actualTarget = levelsToComplete > 0 ? 1 : targetFill;

        fillAmount = Mathf.MoveTowards(fillAmount, actualTarget, Time.deltaTime * fillAnimationSpeed);

        if (levelsToComplete > 0 && fillAmount >= .99f)
        {
            levelsToComplete--;
            currentLevel++;
            if (currentLevel < fmc.game.Settings.maxLevels)
                fillAmount = 0;
            else
                enabled = false; //max level reached

            //Left/right labels animation
            const float scale = 1.1f;
            var tween = TweenScale.Begin(leftLevelLabel.transform.parent.gameObject, levelUpAnimationTime, Vector3.one * scale);
            tween.animationCurve = levelUpScaleCurve;
            tween = TweenScale.Begin(rightLevelLabel.transform.parent.gameObject, levelUpAnimationTime, Vector3.one * scale);
            tween.animationCurve = levelUpScaleCurve;

            SetLevelLabels(currentLevel, currentLevel + 1);
        }

        if (levelsToComplete <= 0 && targetFill == fillAmount)
        {//Hide
            enabled = false;
            StartCoroutine(HideRoutine());
        }



        SetBarsFill(fillAmount);
    }

    void SetLevelLabels(int left, int right)
    {
        left = Mathf.Clamp(left, 1, fmc.game.Settings.maxLevels-1);
        right = Mathf.Clamp(right, 1, fmc.game.Settings.maxLevels);

        leftLevelLabel.text = left.ToString();
        rightLevelLabel.text = right.ToString();
    }

    void SetBarsFill(float fillAmount)
    {
        int n = bars.Length;
        float amountDiv = 1f / n;

        for (int i = 0; i < n; i++)
        {
            float barFill = Mathf.Clamp01(fillAmount / amountDiv);
            fillAmount = Mathf.Max(0, fillAmount - amountDiv);
            bars[i].transform.GetChild(0).GetComponent<UISprite>().fillAmount = barFill;

            if (barFill > 0 && barFill < 1)
            {//moving particles
                Vector3 [] corners = bars[i].GetComponent<UISprite>().worldCorners;
                Vector3 pos = Vector3.Lerp(corners[1], corners[3], barFill);
                pos.z = particles.transform.position.z;
                particles.transform.position = pos;
            }
        }
    }

    public void Show(int currentLevel, int startingExperience, int pointsToAdd, bool disableAnimation = false)
    {
        this.currentLevel = currentLevel;

        if (currentLevel >= fmc.game.Settings.maxLevels)
        {
            SetLevelLabels(fmc.game.Settings.maxLevels, fmc.game.Settings.maxLevels);
            levelsToComplete = 0;
            targetFill = 1;
            enabled = true;
        }
        else
        {
            int totalPoints = startingExperience + pointsToAdd;
            levelsToComplete = 0;

            int levelDuration = fmc.game.Settings.levelDuration.GetValue(currentLevel);
            while (totalPoints > levelDuration)
            {
                totalPoints -= levelDuration;
                levelsToComplete++;
                levelDuration = fmc.game.Settings.levelDuration.GetValue(currentLevel + levelsToComplete);
            }

            targetFill = (float)totalPoints / levelDuration;
            leftLevelLabel.text = currentLevel.ToString();
            rightLevelLabel.text = (currentLevel + 1).ToString();
        }

        if (disableAnimation)
        {
            SetLevelLabels(currentLevel + levelsToComplete, currentLevel + levelsToComplete + 1);
            levelsToComplete = 0;
            fillAmount = targetFill;
            SetBarsFill(fillAmount);
        }

        StartCoroutine(ShowRoutine());
    }

    public void StartPopup()
    {
        StartCoroutine(StartPopupRoutine());
    }

    IEnumerator TestRoutine()
    {
        yield return new WaitForSeconds(1);
        Show(1, 30, 200);
        yield return new WaitForSeconds(6.3f);
        Show(2, 500, 200);
    }

    /// <summary>
    /// called when the game opens
    /// </summary>
    IEnumerator StartPopupRoutine()
    {
        yield return new WaitForSeconds(initialDelayOnGameOpen);
        Show(fmc.game.Level, fmc.game.Experience, 0, true);
    }

    IEnumerator ShowRoutine()
    {
        TweenScale.Begin(gameObject, popupAnimationTime, Vector3.one);
        TweenAlpha.Begin(gameObject, popupAnimationTime, 1);
        yield return new WaitForSeconds(popupAnimationTime);

        particles.Clear();
        var m = particles.emission;
        m.enabled = true;

        enabled = true;
    }

    IEnumerator HideRoutine()
    {
        var m = particles.emission;
        m.enabled = false;
        yield return new WaitForSeconds(stayOnScreenOnceFillCompletedTime);

        if (!enabled) //maybe a new popup has come
        {
            TweenScale.Begin(gameObject, popupAnimationTime, closedScale);
            TweenAlpha.Begin(gameObject, popupAnimationTime, 0);
        }
    }
}
