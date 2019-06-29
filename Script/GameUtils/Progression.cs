using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Progression : MonoBehaviour
{
#pragma warning disable 0414
    [SerializeField]
    string progressionName = "Difficulty Progression";
#pragma warning restore 0414

    [SerializeField]
    float initialValue = 1f;

    [SerializeField]
    float finalValue = 10f;

    [SerializeField]
    float maxScoreProgression = 10000;

    [SerializeField]
    AnimationCurve progression;

    [SerializeField]
    bool ignoreLevelSystem = false;

#pragma warning disable 0414
    [SerializeField]
    bool log = false;
#pragma warning restore 0414

    private void Reset()
    {
        progression = AnimationCurve.Linear(0, 0, 1, 1);
    }

    /// <summary>
    /// Evaluates the progression curve basing on the current score
    /// </summary>
    public float Evaluate()
    {
        return EvaluateAt(fmc.game.CurrentScore / maxScoreProgression);
    }

    /// <summary>
    /// Evaluates the progression curve from 0 to 1
    /// </summary>
    public float EvaluateAt(float progr01)
    {
        float res = Mathf.Lerp(initialValue, finalValue, progression.Evaluate(Mathf.Clamp01(progr01)));
        if (!ignoreLevelSystem)
            res *= fmc.game.Settings.difficulty.GetFloatValue(fmc.game.Level);
        return res;
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (log)
            Debug.Log("PROGRESSION - " + progressionName + ": " + Evaluate());
    }
#endif

}
