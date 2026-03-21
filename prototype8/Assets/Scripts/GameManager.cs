using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Settings")]
    public int totalLevels = 5;

    [Header("State")]
    public int currentLevel = 0;
    public bool isGameActive = false;
    public bool isPaused = false;

    public event Action<int> OnLevelChanged;
    public event Action OnLevelComplete;
    public event Action OnTranfur;
    public event Action OnGameWon;

    public int runSeed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        runSeed = UnityEngine.Random.Range(0, 99999);
    }

    public void StartGame()
    {
        currentLevel = 0;
        runSeed = UnityEngine.Random.Range(0, 99999);
        isGameActive = true;
        isPaused = false;
        LoadLevel(0);
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        OnLevelChanged?.Invoke(levelIndex);
    }

    void Update()
    {
        // No timer - players explore freely until caught or exit
    }

    public void TriggerTranfur()
    {
        isGameActive = false;
        OnTranfur?.Invoke();
    }

    public void CompleteLevel()
    {
        isGameActive = false;
        OnLevelComplete?.Invoke();

        if (currentLevel >= totalLevels - 1)
        {
            OnGameWon?.Invoke();
        }
    }

    public void AdvanceToNextLevel()
    {
        LoadLevel(currentLevel + 1);
        isGameActive = true;
    }

    public void PauseForHint()
    {
        isPaused = true;
        VisualFeedback.Instance?.ShowPauseIndicator();
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.SetCanMove(false);
    }

    public void ResumeFromHint()
    {
        isPaused = false;
        VisualFeedback.Instance?.HidePauseIndicator();
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.SetCanMove(true);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ReturnToMenu()
    {
        isGameActive = false;
        currentLevel = 0;
    }
}
