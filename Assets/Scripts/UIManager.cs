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

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI loseScoreText;

    private void OnEnable()
    {
        var gm = GameManager.Instance;
    }

    private void Start()
    {
        GameManager.Instance.Score.OnScoreChanged += UpdateScore;
        GameManager.Instance.Combo.OnComboChanged += UpdateCombo;
        GameManager.Instance.Timer.OnTimeChanged += UpdateTimer;
    }

    public void UpdateLevelUI(int currentLevel)
    {
        if (levelText == null) return;

        levelText.text = $"LEVEL {currentLevel}";

        levelText.transform.DOKill();
        levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
        scoreText.transform.DOKill();
        scoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f);
    }

    private void UpdateCombo(int hits, int multiplier)
    {
        comboText.text = multiplier > 1 ? $"Combo x{multiplier}" : "";
    }

    private void UpdateTimer(float time)
    {
        timerText.text = Mathf.CeilToInt(time).ToString();
    }

    public void SetBestScore(int best) => bestScoreText.text = $"Best: {best}";

    public void ShowMenu() => menuPanel.SetActive(true);
    public void HideMenu() => menuPanel.SetActive(false);

    public void ShowWin(int score, int best)
    {
        winPanel.SetActive(true);
        winScoreText.text = $"Score: {score}";
        SetBestScore(best);
    }
    public void HideWin() => winPanel.SetActive(false);

    public void ShowLose(int score, int best)
    {
        losePanel.SetActive(true);
        loseScoreText.text = $"Score: {score}";
        SetBestScore(best);
    }
    public void HideLose() => losePanel.SetActive(false);
    public void OnPlayButtonClicked() => GameManager.Instance.StartGame();
    public void OnRestartButtonClicked() => GameManager.Instance.RestartGame();
}
