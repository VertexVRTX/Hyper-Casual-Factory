using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string BestScoreKey = "BestScore";
    public int BestScore { get; private set; }

    public void LoadBest()
    {
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public void TrySaveBest(int score)
    {
        if (score > BestScore)
        {
            BestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }
    }
}
