using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FMCGameDesignWindow : EditorWindow
{
    FMCGameSettings _target;
    Vector2 scrollPos = Vector2.zero;

    Color difficultyColor = Color.white;
    Color levelDurationColor = Color.red;
    Color singleScoreColor = Color.blue;
    Color levelUpRateColor = Color.green * .4f;

    [MenuItem("FMC/Game design tool")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FMCGameDesignWindow)).titleContent = new GUIContent("FMC Game design tool");
    }

    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            FMCGameSettings.LoadOrCreateGameSettings(); //automatically creates new Scriptable Objects if needed.
            //Debug.Log("FMC Settings initialized!");//Use it when debugging
        }
    }

    public FMCGameSettings Target
    {
        get
        {
            if (!FMCGameSettings.IsValid(_target))
                    _target = FMCGameSettings.LoadOrCreateGameSettings();

            return _target;
        }
    }

    AnimationCurve curve = new AnimationCurve();
    bool recapTableOpen = true;

    void OnGUI()
    {
        // /*
        ColorUtility.TryParseHtmlString("#E23400", out difficultyColor);
        ColorUtility.TryParseHtmlString("#1F87F1", out levelDurationColor);
        ColorUtility.TryParseHtmlString("#238E3C", out singleScoreColor);
        ColorUtility.TryParseHtmlString("#B237A8", out levelUpRateColor);
        /*/
        difficultyColor = EditorGUILayout.ColorField(difficultyColor);
        levelDurationColor = EditorGUILayout.ColorField(levelDurationColor);
        singleScoreColor = EditorGUILayout.ColorField(singleScoreColor);
        levelUpRateColor = EditorGUILayout.ColorField(levelUpRateColor);
        //*/

        FMCGameSettings.LoadOrCreateGameSettings();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        //************************* GAME

        GUISpace();
        Target.Public.enableLevelSystem = EditorGUILayout.Toggle("Enable level system",Target.Public.enableLevelSystem);

        if (Target.Public.enableLevelSystem)
        {
            EditorGUILayout.LabelField("Testing", EditorStyles.miniBoldLabel);
            Target.Public.testLevel = Mathf.Clamp(Target.Public.testLevel, 0, Target.Public.maxLevels);
            string debugText = "Test level";
            if (Target.Public.testLevel < 1) debugText += " (OFF)";
            Target.Public.testLevel = EditorGUILayout.IntSlider(debugText, Target.Public.testLevel, 0, Target.Public.maxLevels);
            Target.Public.quickLevelUp = EditorGUILayout.Toggle("Quick level-up", Target.Public.quickLevelUp);
            EditorGUILayout.LabelField("Level system", EditorStyles.miniBoldLabel);

            Target.Public.maxLevels = EditorGUILayout.IntSlider("Max levels", Target.Public.maxLevels, 3, 100);

            GUISpace();
            Target.Public.difficulty = GrowthField(Target.Public.difficulty, 1, 1);
            Func<int, float> difficultyFunc = (x) => { return Target.Public.difficulty.GetFloatValue(x); };
            DrawHistogramCurve(1, Target.Public.maxLevels, difficultyFunc, difficultyColor);

            GUISpace();
            Target.Public.singleScoreAtLevel = GrowthField(Target.Public.singleScoreAtLevel, 10, 100);
            Func<int, float> singleScoreFunc = (x) => { return Target.Public.singleScoreAtLevel.GetValue(x); };
            DrawHistogramCurve(1, Target.Public.maxLevels, singleScoreFunc, singleScoreColor);

            GUISpace();
            Target.Public.levelDuration = GrowthField(Target.Public.levelDuration, 100, 100000);
            Func<int, float> levelDurationFunc = (x) => { return Target.Public.levelDuration.GetValue(x); };
            DrawHistogramCurve(1, Target.Public.maxLevels, levelDurationFunc, levelDurationColor);

            GUISpace();
            recapTableOpen = EditorGUILayout.Foldout(recapTableOpen, "Recap table", EditorStyles.foldout);
            if (recapTableOpen)
            {
                DrawTable(Target.Public.maxLevels, (int)position.width);
            }
        }
        else
        {
            Target.Public.singleScoreAtLevel.firstLevelValue = EditorGUILayout.Slider("Single score",(int)Target.Public.singleScoreAtLevel.firstLevelValue, 10, 100);
        }

        EditorGUILayout.EndScrollView();
        Target.SetDirty();
    }

    private FMCGrowth GrowthField(FMCGrowth g, int minFl, int maxFl)
    {
        EditorGUILayout.LabelField(g.name, EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        if (minFl < maxFl)
            g.firstLevelValue = EditorGUILayout.IntSlider("First level value", (int)g.firstLevelValue, minFl, maxFl);
        else
        {
            g.firstLevelValue = minFl;
            EditorGUILayout.LabelField("(First level value will always be " + g.firstLevelValue + ")");
            GUISpace();
        }

        g.growthFactor = EditorGUILayout.Slider("Growth factor", g.growthFactor, 0, 10);

        g.growthPerturbation = EditorGUILayout.Slider("Growth Perturbation", g.growthPerturbation, 0, 1);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        return g;
    }

    public void DrawTable(int maxLevels, int tableWidth)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        EditorGUILayout.LabelField("As level increase, each progression will multiply by some amount, so <color=#" + ColorUtility.ToHtmlStringRGB(difficultyColor) + "> difficulty will rise.</color>", style);
        EditorGUILayout.LabelField("Every time the player does something good, it will <color=#" + ColorUtility.ToHtmlStringRGB(singleScoreColor) + ">score some points.</color>", style);
        EditorGUILayout.LabelField("The player has to accumulate a certain amount of <color=#" + ColorUtility.ToHtmlStringRGB(levelDurationColor) + ">points to finish the level.</color>", style);
        EditorGUILayout.LabelField("Therefore, we can deduce <color=#" + ColorUtility.ToHtmlStringRGB(levelUpRateColor) + ">how many times the user will have to score to level-up.</color>", style);
        GUISpace();

        int row = 0;
        int cellWidth = 60;
        EditorGUILayout.BeginVertical();
        int numberInARow = Mathf.Clamp(Mathf.FloorToInt(tableWidth / (cellWidth + 3.6f)), 1, 100000);
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < maxLevels; i++)
        {
            DrawLevelDataBox(i + 1, cellWidth);
            row++;
            if (row >= numberInARow)
            {
                row = 0;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUISpace();
                EditorGUILayout.BeginHorizontal();
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    public void DrawLevelDataBox(int level, int width)
    {
        EditorGUILayout.BeginVertical();

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            fixedWidth = width,
            fontStyle = FontStyle.Bold,
            fontSize = 9
        };
        style.fixedHeight = style.lineHeight * 1.2f;
        style.alignment = TextAnchor.MiddleRight;

        GUILayout.Label("LVL " + level, style);

        style.fontStyle = FontStyle.Normal;

        float levelDifficulty = Target.Public.difficulty.GetFloatValue(level);
        int levelDuration = Target.Public.levelDuration.GetValue(level);
        int singleScore = Target.Public.singleScoreAtLevel.GetValue(level);
        int levelUpRate = Mathf.CeilToInt((float)levelDuration / singleScore);

        style.normal.textColor = difficultyColor;
        GUILayout.Label("x"+Math.Round(levelDifficulty,2), style);

        style.normal.textColor = singleScoreColor;
        GUILayout.Label(singleScore.ToString(), style);

        style.normal.textColor = levelDurationColor;
        GUILayout.Label(levelDuration.ToString(), style);

        style.normal.textColor = levelUpRateColor;
        GUILayout.Label(levelUpRate.ToString(), style);
        EditorGUILayout.EndHorizontal();
    }

    void DrawHistogramCurve(int start, int end, Func<int, float> function, Color color)
    {
        EditorGUILayout.BeginHorizontal();

        int pointsToDraw = end - start + 2;

        Keyframe[] kfs = new Keyframe[pointsToDraw * 4 + 2];
        kfs[0] = new Keyframe(start - 1, 0);
        kfs[kfs.Length - 1] = new Keyframe(end + 1, 0);

        float rectangleExt = (float)end / pointsToDraw / 8f;
        float rectangleInt = rectangleExt * .999f;

        for (int i = 0; i <= end - start; i++)
        {
            int si = start + i;
            float currentValue = function(si);

            kfs[i * 4 + 1] = new Keyframe(si - rectangleExt, 0);
            kfs[i * 4 + 2] = new Keyframe(si - rectangleInt, currentValue);
            kfs[i * 4 + 3] = new Keyframe(si + rectangleInt, currentValue);
            kfs[i * 4 + 4] = new Keyframe(si + rectangleExt, 0);
        }

        curve.keys = kfs;
        curve = EditorGUILayout.CurveField(curve, color, Rect.zero, GUILayout.Height(60));
        EditorGUILayout.LabelField("Max:\n" + Math.Round(function(end), 2).ToString(), GUILayout.MaxWidth(55), GUILayout.Height(60));
        EditorGUILayout.EndHorizontal();
    }

    //Helper function used to add the given amount of space
    void GUISpace(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
            EditorGUILayout.Space();
    }
}
