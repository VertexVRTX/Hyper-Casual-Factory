using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public int CurrentLevel { get; private set; } = 1;
    public int ScoreToNextLevel => CurrentLevel * 150;

    public void ResetLevel()
    {
        CurrentLevel = 1;

        if (GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.UpdateLevelUI(CurrentLevel);
        }
    }

    public void CheckLevelUp(int currentScore)
    {
        if (currentScore >= ScoreToNextLevel)
        {
            CurrentLevel++;
            if (GameManager.Instance.UI != null)
            {
                GameManager.Instance.UI.UpdateLevelUI(CurrentLevel);
            }
        }
    }

    public BoxMechanicState GetRandomMechanicForLevel()
    {
        float rand = Random.value;

        if (CurrentLevel == 1) return BoxMechanicState.Normal;

        if (CurrentLevel == 2)
        {
            if (rand < 0.3f) return BoxMechanicState.Sealed;
            return BoxMechanicState.Normal;
        }

        if (CurrentLevel == 3)
        {
            if (rand < 0.25f) return BoxMechanicState.Sealed;
            if (rand < 0.50f) return BoxMechanicState.Frozen;
            return BoxMechanicState.Normal;
        }

        if (rand < 0.2f) return BoxMechanicState.Sealed;
        if (rand < 0.4f) return BoxMechanicState.Frozen;
        if (rand < 0.6f) return BoxMechanicState.Glass;

        return BoxMechanicState.Normal;
    }
}
