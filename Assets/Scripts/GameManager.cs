using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static bool shouldAutoStart = false;

    [Header("Refs")]
    public ConveyorSpawner Conveyor;
    public ScoreManager Score;
    public ComboManager Combo;
    public TimerManager Timer;
    public UIManager UI;
    public SaveManager SaveManager;
    public InputManager Input;
    public LevelManager Level;

    [Header("Rules")]
    public int scoreToWin = 200;
    public int maxLives = 3;

    [Header("Combo Settings")]
    public int currentCombo = 0;

    private GameStateMachine _fsm;
    private int _lives;

    public MenuState Menu;
    public PlayingState Playing;
    public WinState Win;
    public LoseState Lose;

    private void Awake()
    {
        Instance = this;
        _fsm = new GameStateMachine();
        Menu = new MenuState(this);
        Playing = new PlayingState(this);
        Win = new WinState(this);
        Lose = new LoseState(this);

        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        _lives = maxLives;
        SaveManager.LoadBest();
        UI.SetBestScore(SaveManager.BestScore);

        FindObjectOfType<ContainerShuffler>()?.ShuffleContainers();

        if (shouldAutoStart)
        {
            shouldAutoStart = false;
            _fsm.Initialize(Playing);

            StartGameLogic();
        }
        else
        {
            _fsm.Initialize(Menu);
        }
    }

    private void Update() => _fsm.Tick();

    public void StartGame()
    {
        StartGameLogic();
        _fsm.ChangeState(Playing);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSound);
    }

    private void StartGameLogic()
    {
        Score.ResetScore();
        Combo.ResetCombo();
        Timer.ResetTimer();
        if (Level != null) Level.ResetLevel();
        _lives = maxLives;
    }

    public void OnBoxMissed()
    {
        _lives--;
        Combo.ResetCombo();
        if (_lives <= 0) _fsm.ChangeState(Lose);
    }

    public void OnCorrectSort(int points)
    {
        Combo.AddHit();
        Score.AddScore(points * Combo.Multiplier);

        if (Level != null) Level.CheckLevelUp(Score.CurrentScore);
    }

    public void OnWrongSort()
    {
        Combo.ResetCombo();
    }

    public void OnTimerEnded()
    {
        _fsm.ChangeState(Score.CurrentScore >= scoreToWin ? (IGameState)Win : Lose);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        shouldAutoStart = true;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSound);
    }

    public int CalculateScoreWithCombo(int baseScore)
    {
        currentCombo++;

        int multiplier = 1 + (currentCombo / 3);
        int finalScore = baseScore * multiplier;

        if (currentCombo >= 3 && currentCombo % 3 == 0)
        {

        }

        return finalScore;
    }

    public void ResetCombo()
    {
        currentCombo = 0;
    }

    public void OnGameFinished()
    {
        if (Input != null) Input.enabled = false;

        if (Conveyor != null) Conveyor.StopSpawning();

        Time.timeScale = 0f;
    }

    public void OnGameStarted()
    {
        Time.timeScale = 1f;
        if (Input != null) Input.enabled = true;

        if (Conveyor != null) Conveyor.StartSpawning();
    }
}
