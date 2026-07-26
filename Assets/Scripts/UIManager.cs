using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI levelText;

    [Header("Abilities UI")]
    public Button freezeButton;
    public Image freezeCooldownOverlay;
    public TextMeshProUGUI freezeCooldownText;
    public float freezeCooldownDuration = 10f;
    public float freezeDuration = 4f;

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI loseScoreText;

    private float _freezeCooldownTimer = 0f;
    private bool _isFreezeOnCooldown = false;
    private int _lastSecondPlayed = -1;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.Score != null) GameManager.Instance.Score.OnScoreChanged += UpdateScore;
            if (GameManager.Instance.Combo != null) GameManager.Instance.Combo.OnComboChanged += UpdateCombo;
            if (GameManager.Instance.Timer != null) GameManager.Instance.Timer.OnTimeChanged += UpdateTimer;
        }

        if (freezeButton != null)
        {
            freezeButton.onClick.AddListener(OnFreezeButtonClicked);
        }

        ResetFreezeButtonUI();
    }

    private void Update()
    {
        if (_isFreezeOnCooldown)
        {
            _freezeCooldownTimer -= Time.deltaTime;

            if (_freezeCooldownTimer <= 0f)
            {
                ResetFreezeButtonUI();
            }
            else
            {
                UpdateFreezeButtonCooldownUI();
            }
        }
    }

    #region Freeze Ability Logic (NEW)

    public void OnFreezeButtonClicked()
    {
        if (_isFreezeOnCooldown) return;

        if (GameManager.Instance != null && GameManager.Instance.Conveyor != null)
        {
            GameManager.Instance.Conveyor.ActivateFreezeAbility(freezeDuration);
        }

        if (freezeButton != null)
        {
            freezeButton.transform.DOKill();
            freezeButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.15f);
        }

        _isFreezeOnCooldown = true;
        _freezeCooldownTimer = freezeCooldownDuration;
        if (freezeButton != null) freezeButton.interactable = false;
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.freezeAbilitySound);
    }

    private void UpdateFreezeButtonCooldownUI()
    {
        if (freezeCooldownText != null)
        {
            freezeCooldownText.gameObject.SetActive(true);
            freezeCooldownText.text = Mathf.CeilToInt(_freezeCooldownTimer).ToString();
        }

        if (freezeCooldownOverlay != null)
        {
            freezeCooldownOverlay.gameObject.SetActive(true);
            freezeCooldownOverlay.fillAmount = _freezeCooldownTimer / freezeCooldownDuration;
        }
    }

    private void ResetFreezeButtonUI()
    {
        _isFreezeOnCooldown = false;
        _freezeCooldownTimer = 0f;

        if (freezeButton != null) freezeButton.interactable = true;
        if (freezeCooldownOverlay != null) freezeCooldownOverlay.gameObject.SetActive(false);
        if (freezeCooldownText != null) freezeCooldownText.gameObject.SetActive(false);
    }

    #endregion

    public void UpdateLevelUI(int currentLevel)
    {
        if (levelText == null) return;

        levelText.text = $"LEVEL {currentLevel}";

        levelText.transform.DOKill();
        levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
    }

    private void UpdateScore(int score)
    {
        if (scoreText == null) return;
        scoreText.text = score.ToString();
        scoreText.transform.DOKill();
        scoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
    }

    private void UpdateCombo(int hits, int multiplier)
    {
        if (comboText == null) return;
        comboText.text = multiplier > 1 ? $"Combo x{multiplier}" : "";
    }

    private void UpdateTimer(float time)
    {
        int currentSeconds = Mathf.CeilToInt(time);

        if (currentSeconds <= 10 && currentSeconds > 0 && currentSeconds != _lastSecondPlayed)
        {
            _lastSecondPlayed = currentSeconds;
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.timerWarningSound);
        }

        timerText.text = currentSeconds.ToString();
    }

    public void SetBestScore(int best) => bestScoreText.text = $"Best: {best}";

    public void ShowMenu() => menuPanel.SetActive(true);
    public void HideMenu() => menuPanel.SetActive(false);

    public void ShowWin(int score, int best)
    {
        winPanel.SetActive(true);
        winScoreText.text = $"Score: {score}";
        SetBestScore(best);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.winSound);
    }
    public void HideWin() => winPanel.SetActive(false);

    public void ShowLose(int score, int best)
    {
        losePanel.SetActive(true);
        loseScoreText.text = $"Score: {score}";
        SetBestScore(best);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.loseSound);
    }
    public void HideLose() => losePanel.SetActive(false);
    public void OnPlayButtonClicked() => GameManager.Instance.StartGame();
    public void OnRestartButtonClicked() => GameManager.Instance.RestartGame();
}
