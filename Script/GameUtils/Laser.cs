using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shoots a laser from the given position to the end position. 
/// Requires a sprite with the pivot at the bottom.
/// </summary>
public class Laser : MonoBehaviour
{
    TweenScale tweenScale;
    TweenPosition tweenPos;

    [SerializeField]
    float width = .2f;

    void Start()
    {
        tweenScale = GetComponent<TweenScale>();
        tweenPos = GetComponent<TweenPosition>();
    }

    public void Shoot(Vector3 start, Vector3 end, float duration)
    {
        transform.position = start;
        transform.localScale = Vector3.zero;

        float angle = -fmc.utils.SignedAngle2D((Vector2)(end - start), Vector2.up);
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, angle);

        tweenScale.ResetToBeginning();
        tweenScale.to = new Vector3(width, (start - end).magnitude, 1);
        tweenScale.duration = duration;
        tweenScale.PlayForward();

        tweenPos.ResetToBeginning();
        tweenPos.from = start;
        tweenPos.to = end;
        tweenPos.duration = duration;
        tweenPos.PlayForward();
    }
}
