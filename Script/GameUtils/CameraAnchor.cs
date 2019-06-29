using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this component on an object to make it stay at a certain camera angle.
/// Z position will be preserved.
/// </summary>
[ExecuteInEditMode]
public class CameraAnchor : MonoBehaviour
{
    [Header("Position anchor")]

    [SerializeField]
    Camera targetCam = null;

    [SerializeField]
    public fmc.utils.CameraAnchor stayAt = fmc.utils.CameraAnchor.Center;

    [SerializeField]
    public bool updateEveryFrame = true;

    [SerializeField]
    public Vector2 offset = Vector2.zero;

    [Header("Fill anchor")]

    [SerializeField, Range(0f, 1f)]
    public float fillWidth = 0;

    [SerializeField, Range(0f, 1f)]
    public float fillHeight = 0;

    [SerializeField, Range(0f, 1f)]
    public float paddingX = 0;

    [SerializeField, Range(0f, 1f)]
    public float paddingY = 0;

    [SerializeField]
    public bool affectScaling = true;

    SpriteRenderer spriteRenderer;


    public bool UpdateEveryFrame
    {
        get { return updateEveryFrame; }
        set
        {
            updateEveryFrame = value;
#if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isPlaying)
#endif
                enabled = value;
        }
    }

    void Start()
    {
        if (!targetCam)
            targetCam = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();
        Resize();
        UpdatePosition();
        UpdateEveryFrame = updateEveryFrame; // just to toggle enabling
    }

    void Update()
    {
        Resize();
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (targetCam)
        {
            Vector2 actualOffset = offset;

            if (fillWidth > 0)
            {
                int direction = 0;
                direction = stayAt.Contains(fmc.utils.CameraAnchor.Right) ? 1 : direction;
                direction = stayAt.Contains(fmc.utils.CameraAnchor.Left) ? -1 : direction;

                actualOffset.x -= spriteRenderer.sprite.bounds.size.x / 2f * transform.lossyScale.x * direction;
            }

            if (fillHeight > 0)
            {
                int direction = 0;
                direction = stayAt.Contains(fmc.utils.CameraAnchor.Top) ? 1 : direction;
                direction = stayAt.Contains(fmc.utils.CameraAnchor.Bottom) ? -1 : direction;

                actualOffset.y -= spriteRenderer.sprite.bounds.size.y / 2f * transform.lossyScale.y * direction;
            }

            Vector3 pos = fmc.utils.GetCameraPosition(targetCam, stayAt, paddingX, paddingY) + actualOffset;
            pos.z = transform.position.z;
            transform.position = pos;
        }
    }

    void Resize()
    {
        if (spriteRenderer && targetCam)
        {
            float worldScreenHeight = targetCam.orthographicSize * 2f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            if (affectScaling)
            {
                float width = spriteRenderer.sprite.bounds.size.x;
                float height = spriteRenderer.sprite.bounds.size.y;
                Vector3 newScale = transform.localScale;
                if (fillWidth > 0)
                    newScale.x = worldScreenWidth / width * fillWidth * (1 - paddingX);
                if (fillHeight > 0)
                    newScale.y = worldScreenHeight / height * fillHeight * (1 - paddingY);
                transform.localScale = newScale;
            }

            spriteRenderer.size = new Vector2(worldScreenWidth * fillWidth * (1 - paddingX), worldScreenHeight * fillHeight * (1 - paddingY));
        }
    }
}
