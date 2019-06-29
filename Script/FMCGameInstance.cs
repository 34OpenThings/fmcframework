using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum FMCGameState { SplashScreen, TitleScreen, Playing, GameOver }

public class FMCGameInstance
{
    bool gameWasReset = true; //distinguish between fresh game and second chance
    int lastScore = 0; //used from level system to calculate the amounts of points to sum to level up on second chance

    Dictionary<string, object> gameProperties;

    #region game events

    public Action ResetGameAction;
    public Action GameStartedAction;
    public Action<long> NewBestAction; // (newBestScore)
    public Action<long> GameOverAction; // (score)
    public Action ResumedFromGameOverAction;
    public Action GoingToTitleScreenAction;
    public Action<int> GainingExperienceAction; // (experienceGained)
    public Action<int> LevelUpAction; // (newLevel)
    public Action<FMCGameState, FMCGameState> GameStateChanged; // (oldState, newState)

    #endregion

    public FMCGameState GameState { get; private set; }

    public FMCGameInstance()
    {
        gameProperties = new Dictionary<string, object>();
        GameState = FMCGameState.SplashScreen;

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name != "FMCSplashScreen")
        {
            //FMCGameInstance should be created inside the first scene (Splash screen)
            //If we are already in the game scene, it means that we started inside the scene in the editor.
            //Let's simulate the state change event to make the transition consistent
            GoToState(FMCGameState.TitleScreen);
        }
#endif
    }

    /// <summary>
    /// Sets a game parameter. 
    /// If bPersistent is true, the parameter is automatically saved on disk.
    /// Mind that the given type has to be serializable in order to be saved.
    /// </summary>
    public void SetProperty<T>(string key, T value, bool isPersistent = false)
    {
        if (gameProperties.ContainsKey(key))
            gameProperties[key] = value;
        else
            gameProperties.Add(key, value);

        if (isPersistent)
            FMCFramework.Instance.SaveSystem.Write(key, value);
    }

    public T GetProperty<T>(string key, T defaultValue)
    {
        if (!gameProperties.ContainsKey(key))
        {
            //A property can either be volatile or persistent (saved on disk).
            //With this method, it will work either way: if it's not a saved property it will return the default value.
            //This causes a very little overhead but this method is executed only the first time a certain parameter is requested, so it's not a problem.
            SetProperty(key, FMCFramework.Instance.SaveSystem.Read(key, defaultValue));
        }

        return (T)gameProperties[key];
    }

    public void GoToGameScene()
    {
        SceneManager.LoadScene(1);
        GoToState(FMCGameState.TitleScreen);
    }

    /// <summary>
    /// Updates the state machine of the GameInstance and fires the appropriate events
    /// </summary>
    public void GoToState(FMCGameState newState)
    {
        if (newState == FMCGameState.GameOver)
        {
            if (fmc.game.Settings.enableLevelSystem && fmc.game.Level < fmc.game.Settings.maxLevels)
            {
                int pointsGained;
                if (fmc.game.Settings.quickLevelUp)
                {//always finish the level
                    pointsGained = fmc.game.Settings.levelDuration.GetValue(fmc.game.Level) - fmc.game.Experience + 10;
                }
                else
                {//normal behavior
                    pointsGained = fmc.game.CurrentScore;
                    if (!gameWasReset)
                        pointsGained -= lastScore; //player will only get difference if it had a second chance
                    lastScore = fmc.game.CurrentScore;
                }

                if (GainingExperienceAction != null) GainingExperienceAction.Invoke(pointsGained);
                GainExperience(pointsGained);
            }
            if (GameOverAction != null) GameOverAction.Invoke(fmc.game.CurrentScore);
        }
        else if (newState == FMCGameState.Playing)
        {
            if (GameState == FMCGameState.GameOver && !gameWasReset)
                if (ResumedFromGameOverAction != null) ResumedFromGameOverAction.Invoke();

            if (GameStartedAction != null) GameStartedAction.Invoke();

            gameWasReset = false;
        }
        else if (newState == FMCGameState.TitleScreen)
            if (GoingToTitleScreenAction != null) GoingToTitleScreenAction.Invoke();

        if (GameStateChanged != null) GameStateChanged.Invoke(GameState, newState);
        GameState = newState;
    }

    /// <summary>
    /// Increases experience and levels up
    /// </summary>
    private void GainExperience(int pointsGained)
    {
        int exp = fmc.game.Experience + pointsGained;
        int levelDuration = fmc.game.Settings.levelDuration.GetValue(fmc.game.Level);
        while (exp >= levelDuration)
        {
            exp -= levelDuration;
            fmc.game.Level++;
            levelDuration = fmc.game.Settings.levelDuration.GetValue(fmc.game.Level);
            if (LevelUpAction != null) LevelUpAction.Invoke(fmc.game.Level);
        }
        fmc.game.Experience = exp;
    }

    public void ResetGame()
    {
        lastScore = 0;
        gameWasReset = true;
        if (ResetGameAction != null) ResetGameAction.Invoke();
    }
}
