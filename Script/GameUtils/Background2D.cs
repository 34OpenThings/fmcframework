using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer)), ExecuteInEditMode]
public class Background2D : MonoBehaviour
{
    [SerializeField]
    bool scroll = false;

    [SerializeField]
    GameObject targetToFollow = null;

    [SerializeField]
    public float scrollSpeed = 0;

    [SerializeField]
    Camera targetCam = null;

    SpriteRenderer sr;

    double offset = 0;

    public void Start()
    {
        if (sr)
            sr.drawMode = SpriteDrawMode.Tiled;

        if (!targetCam)
            targetCam = Camera.main;

        sr = GetComponent<SpriteRenderer>();

        if (Application.isPlaying)
        {
            if (scroll)
                sr.material.SetTexture("_MainTex", sr.sprite.texture);

            sr.material.SetFloat("_Offset", 0);
        }

        if (targetToFollow)
            lastPosition = targetToFollow.transform.position.y;
    }

    double lastPosition = 0;
    private void Update()
    {
        if (sr)
        {
            if (scroll && Application.isPlaying)
            {
                double currentScrollDelta = 0;

                if (targetToFollow)
                {
                    double delta = (lastPosition - targetToFollow.transform.position.y) / (targetCam.orthographicSize * 2f); //percentage of screen delta
                    Debug.Log(delta);
                    lastPosition = targetToFollow.transform.position.y;

                    //Handling tiling of the sprite
                    float height = sr.sprite.bounds.size.y;
                    float worldScreenHeight = targetCam.orthographicSize * 2f;
                    currentScrollDelta = delta * worldScreenHeight / height;
                }
                else
                    currentScrollDelta = scrollSpeed * Time.deltaTime * sr.size.y / sr.size.x;

                if (currentScrollDelta != 0)
                {
                    offset -= currentScrollDelta;
                    sr.material.SetFloat("_Offset", (float)offset);
                }
            }
        }
    }
}