using System;
using UnityEngine;

public enum GameMode
{
    Journey,
    Gem
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameMode CurrentMode { get; private set; }

    public event Action<GameMode> OnSetMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
        // Load luật chơi tương ứng
        ApplyModeSettings();

        OnSetMode?.Invoke(mode);
    }

    private void ApplyModeSettings()
    {
        switch (CurrentMode)
        {
            case GameMode.Journey:
                // Setup luật cho survival
                break;
            case GameMode.Gem:
                // Setup luật cho time attack
                break;
        }
    }
}
