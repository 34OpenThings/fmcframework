using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreMark : MonoBehaviour
{
    [SerializeField]
    UILabel textLabel = null;
    [SerializeField]
    TweenPosition tweenPos = null;
    [SerializeField]
    TweenAlpha tweenAlpha = null;

    public void InitScoreMark(string text, Color color, GameObject target, float duration, Vector3 offset)
    {
        transform.OverlayPosition(target.transform);

        TweenPosition.Begin(gameObject, duration, tweenPos.value + offset);

        GetComponent<UILabel>().depth = 999;

        tweenAlpha.duration = duration;
        tweenAlpha.ResetToBeginning();
        tweenAlpha.PlayForward();

        textLabel.color = color;
        textLabel.effectColor = color;
        textLabel.text = text;

        Destroy(gameObject, duration);
    }
}
