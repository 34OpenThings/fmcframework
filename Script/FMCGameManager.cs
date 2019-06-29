using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FMCGameManager : MonoBehaviour
{
    public static FMCGameManager Instance { get; protected set; }

    UIManager ui;

    void Awake()
    {
        Instance = this;

        //Creating UI;
        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            GameObject uiPrefab = Resources.Load<GameObject>("FMCUI");
            ui = Instantiate(uiPrefab).GetComponent<UIManager>();
            ui.transform.Translate(-Vector3.forward * 100 + Vector3.right * 10); // move away from camera
        }
    }

    /// <summary>
    /// Spawn a label with the given text on the given target
    /// </summary>
    public void ScoreMark(string text, Color color, GameObject target, float duration, Vector3 offset)
    {
        if (ui)
            ui.ScoreMark(text, color, target, duration, offset);
    }
}
