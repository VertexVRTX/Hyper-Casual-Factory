using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class JuicyUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TimerManager timerManager;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Timer")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.red;

    [Header("Level UI")]
    [SerializeField] private TMP_Text levelText;

    private Tween _timerPulseTween;
    private Vector3 _scoreInitialScale;
    private Vector3 _timerInitialScale;

    private void Awake()
    {
        if (scoreText != null) _scoreInitialScale = scoreText.transform.localScale;
        if (timerText != null) _timerInitialScale = timerText.transform.localScale;
    }

    private void OnEnable()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
        }

        if (timerManager != null)
        {
            timerManager.OnTimeChanged += HandleTimeChanged;
        }
    }

    private void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= HandleScoreChanged;
        }

        if (timerManager != null)
        {
            timerManager.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText == null) return;

        scoreText.text = newScore.ToString();

        scoreText.transform.DOKill(true);
        scoreText.transform.localScale = _scoreInitialScale;

        scoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, vibrato: 5, elasticity: 0.5f)
                 .OnComplete(() => scoreText.transform.localScale = _scoreInitialScale);
    }

    private void HandleTimeChanged(float timeRemaining)
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds.ToString();
        if (timeRemaining <= 10f && timeRemaining > 0f)
        {
            if (_timerPulseTween == null || !_timerPulseTween.IsActive())
            {
                StartTimerPulse();
            }
        }
        else
        {
            StopTimerPulse();
        }
    }

    private void StartTimerPulse()
    {
        timerText.color = warningTimerColor;

        _timerPulseTween = timerText.transform.DOScale(_timerInitialScale * 1.25f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopTimerPulse()
    {
        if (_timerPulseTween != null)
        {
            _timerPulseTween.Kill();
            _timerPulseTween = null;
        }

        if (timerText != null)
        {
            timerText.transform.localScale = _timerInitialScale;
            timerText.color = normalTimerColor;
        }
    }

    public void UpdateLevelUI(int currentLevel)
    {
        if (levelText != null)
        {
            levelText.text = $"LEVEL {currentLevel}";

            levelText.transform.DOKill(true);
            levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f);
        }
    }
}
