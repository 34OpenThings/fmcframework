using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component will cycle the given colors on the camera backgrounds.
/// The change can happen over time or points acquired.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraColorBackground : MonoBehaviour
{
    /// <summary>
    /// Will the background change after x seconds or after x points?
    /// </summary>
    public enum OnScreenEvaluationPrinciple { Time, Score };

    [SerializeField]
    Color[] colors = { Color.white };

    [SerializeField]
    public OnScreenEvaluationPrinciple basedOn;

    [SerializeField]
    public float colorOnScreenValue = 5f;

    [SerializeField]
    public float colorChangeAnimationTime = 1f;

    [SerializeField]
    public bool transitionManually = false;

    [SerializeField]
    public bool activeInGameOnly = true;

    [SerializeField]
    public bool resetOnGameOver = true;

    int colorIndex;
    float count;

    bool isTransitioning = false;

#pragma warning disable 0109
    new Camera camera = null;
#pragma warning restore 0109


    int PrevIndex { get { return (colorIndex + colors.Length - 1) % colors.Length; } }
    int NextIndex { get { return (colorIndex + 1) % colors.Length; } }
    Color fromColor;//used to have smooth transition

    void Start()
    {
        camera = GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        count = 0;
        isTransitioning = false;
        camera.backgroundColor = colors[0];

        fmc.events.GameStarted += () =>
        {
            enabled = true;
        };

        fmc.events.GameOver += (score) =>
        {
            if(resetOnGameOver)
                TransitionTo(0);
        };

        if (activeInGameOnly)
            enabled = false;
    }

    void Update()
    {
        count += Time.deltaTime;

        if (isTransitioning)
        {
            float perc = count / colorChangeAnimationTime;
            if (perc >= 1)
            {
                isTransitioning = false;
                count = 0;
                camera.backgroundColor = colors[colorIndex];

                if (transitionManually || (activeInGameOnly && fmc.game.GameState != FMCGameState.Playing))
                    enabled = false;
            }
            else
            {
                camera.backgroundColor = Color.Lerp(fromColor, colors[colorIndex], perc);
            }
        }
        else
        {
            if (transitionManually || (activeInGameOnly && fmc.game.GameState != FMCGameState.Playing))
                enabled = false;

            if (!transitionManually)
            {
                if (basedOn == OnScreenEvaluationPrinciple.Time)
                {
                    if (count >= colorOnScreenValue)
                        Transition();
                }
                else
                {//Color is based on score
                    long index = (fmc.game.CurrentScore / Mathf.CeilToInt(colorOnScreenValue)) % colors.Length;
                    if (index != colorIndex)
                        TransitionTo((int)index);
                }
            }
        }
    }

    public void Transition()
    {
        TransitionTo(NextIndex);
    }

    public void TransitionTo(int index)
    {
        fromColor = camera.backgroundColor;
        index = Mathf.Clamp(index, 0, colors.Length);
        count = 0;
        colorIndex = index;
        isTransitioning = true;
        enabled = true;
    }
}
